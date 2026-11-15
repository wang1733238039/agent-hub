import assert from 'node:assert/strict';
import { execFile } from 'node:child_process';
import { join } from 'node:path';
import { promisify } from 'node:util';
import { fileURLToPath } from 'node:url';
import { test } from 'node:test';
import { openOriginalDesktopThread } from '../packages/codex-adapter/dist/desktop-prepared-bridge.js';
import { windowsPowerShellEnvironment } from '../packages/codex-adapter/dist/desktop-platform.js';

const run = promisify(execFile);
const helper = fileURLToPath(new URL('../packages/codex-adapter/windows/', import.meta.url));
const powershell = join(
  process.env.SystemRoot ?? 'C:\\Windows',
  'System32/WindowsPowerShell/v1.0/powershell.exe',
);
const options = { windowsHide: true, timeout: 15_000, env: windowsPowerShellEnvironment() };

test('desktop navigation rejects invalid remote IDs before launching anything', async () => {
  for (const id of ['not-a-thread', 'codex://threads/example', '"; exit 0; #'])
    await assert.rejects(openOriginalDesktopThread(id), /ID/);
});

test(
  'native packaged activation rejects invalid targets and recognizes unpackaged owners',
  {
    skip: process.platform !== 'win32',
  },
  async () => {
    const { stdout } = await run(
      powershell,
      [
        '-NoProfile',
        '-NonInteractive',
        '-File',
        fileURLToPath(new URL('./fixtures/windows-desktop-open.ps1', import.meta.url)),
        '-Source',
        join(helper, 'desktop-open.cs'),
      ],
      options,
    );
    assert.match(stdout, /NATIVE_DESKTOP_OPEN_BOUNDARIES_OK/);
  },
);

test(
  'PowerShell entry rejects malformed IDs before pipe access',
  {
    skip: process.platform !== 'win32',
  },
  async () => {
    await assert.rejects(
      run(
        powershell,
        [
          '-NoProfile',
          '-NonInteractive',
          '-File',
          join(helper, 'desktop-open.ps1'),
          '-ThreadId',
          'invalid;command',
        ],
        options,
      ),
      (error) => error.code === 1 && /ThreadId/.test(error.stderr),
    );
  },
);
