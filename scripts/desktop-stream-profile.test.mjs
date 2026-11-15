import assert from 'node:assert/strict';
import { randomUUID } from 'node:crypto';
import { createServer } from 'node:net';
import { test } from 'node:test';
import {
  desktopFixtureEndpoint,
  listenDesktopFixture,
  prepareDesktopFixtureIpc,
} from './fixtures/desktop-ipc-transport.mjs';

const { DesktopIpc, desktopTurns } = await import('../packages/codex-adapter/dist/desktop-ipc.js');
const { DesktopFrameDecoder } = await import('../packages/codex-adapter/dist/desktop-framing.js');

for (const version of [11, 13]) {
  test(
    'stream profile ' + version + ' follows the original owner and applies canonical patches',
    async () => {
      const id = randomUUID();
      const path = desktopFixtureEndpoint('./stream-profile-' + randomUUID() + '.sock');
      let peer;
      const requests = [];
      const send = (value) => {
        const body = Buffer.from(JSON.stringify(value));
        const header = Buffer.alloc(4);
        header.writeUInt32LE(body.length);
        peer.write(header.subarray(0, 2));
        peer.write(Buffer.concat([header.subarray(2), body]));
      };
      const broadcast = (change, overrides = {}) =>
        send({
          type: 'broadcast',
          method: 'thread-stream-state-changed',
          version,
          sourceClientId: 'original-owner',
          params: { hostId: 'local', conversationId: id, change },
          ...overrides,
        });
      const state = {
        id,
        hostId: 'local',
        threadRuntimeStatus: { type: 'active' },
        turns: [],
        turnHistory: {
          kind: 'canonical',
          history: {
            islands: [{ entries: [{ value: 'turn-key' }] }],
            entitiesByKey: {
              'turn-key': {
                turnId: 'turn',
                status: 'inProgress',
                items: [{ id: 'reply', type: 'agentMessage', text: 'First' }],
              },
            },
          },
        },
      };
      const server = createServer((socket) => {
        peer = socket;
        socket.on('error', () => {});
        const decoder = new DesktopFrameDecoder((bytes) => {
          const message = JSON.parse(bytes.toString('utf8'));
          requests.push(message);
          if (message.type === 'request') {
            assert(['initialize', 'thread-owner-discovery'].includes(message.method));
            send({
              type: 'response',
              requestId: message.requestId,
              resultType: 'success',
              method: message.method,
              handledByClientId: 'original-owner',
              result:
                message.method === 'initialize'
                  ? { clientId: 'reader' }
                  : { supportsUntrustedAppInput: true },
            });
          } else if (message.method === 'thread-stream-following-changed') {
            broadcast({ type: 'snapshot', revision: 1, conversationState: state });
          }
        });
        socket.on('data', (chunk) => decoder.push(chunk));
      });
      let ipc;
      try {
        await listenDesktopFixture(server, path);
        ipc = prepareDesktopFixtureIpc(new DesktopIpc(path));
        await ipc.connect();
        assert.equal(desktopTurns(await ipc.follow(id))[0].items[0].text, 'First');
        assert(ipc.hasOwner(id));
        const received = () =>
          new Promise((resolve, reject) => {
            const timer = setTimeout(() => reject(Error('Missing stream delivery')), 2000);
            ipc.onSnapshot = () => {
              clearTimeout(timer);
              resolve();
            };
            ipc.onUnavailable = () => {
              clearTimeout(timer);
              resolve();
            };
          });
        const next = received();
        broadcast({
          type: 'patches',
          baseRevision: 1,
          revision: 2,
          patches: [
            {
              op: 'replace',
              path: ['turnHistory', 'history', 'entitiesByKey', 'turn-key', 'items', 0, 'text'],
              value: 'First second',
            },
          ],
          ...(version === 13 ? { acceptedTextChanges: [] } : {}),
        });
        await next;
        assert.equal(desktopTurns(ipc.state(id))[0].items[0].text, 'First second');
        assert(ipc.hasOwner(id));
        const refused = received();
        broadcast({ type: 'snapshot', revision: 3, conversationState: state }, { version: 14 });
        await refused;
        assert.equal(ipc.hasOwner(id), false, 'Unknown wire versions must revoke control');
        assert(!requests.some((r) => r.method?.startsWith('thread-follower-')));
      } finally {
        ipc?.close();
        peer?.destroy();
        await new Promise((resolve) => server.close(resolve));
      }
    },
  );
}
