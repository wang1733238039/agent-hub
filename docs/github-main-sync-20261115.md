# Personal GitHub main integration

Date: 2026-11-15. Authorized target: `wang1733238039/agent-hub`.

## Scope and history

This integration joins local personal main
`384c64fa11c25fe77e719e85c54a7a64adfcc5f0` and the existing personal GitHub main
`6979168c52b5cdb495b771de38aa93146ad7801d`. Both histories are retained. The
previous local main is preserved by branch `codex/before-github-sync-20261115`.
The obsolete `wang1733238039/codex-mobile` destination is replaced with the
authorized personal repository. `easyaigc/agent-hub` remains read-only.

## Integrated behavior

- Retain PR3 filtered-event readiness, concurrent snapshot ordering and
  historical/live message identity repairs. Earlier-message controls remain
  usable while a background latest-history read is pending.
- Retain authenticated Windows cold-thread activation and observed desktop
  stream profiles 11/13, together with newer archive bounds, peer inspection
  and native private-state protections.
- Retain personal continuous voice, the initial 15-message reading window,
  package identities, signing configuration and Android update page. Update
  discovery now points to this repository's releases. No release is created.
- Retain the newer native storage, localization, migration, settings and
  subagent implementation. No remote-machine acceptance record is reused as
  proof for this Windows integration.
- Preserve remote feature IDs F73 (usage), F74 and F75. The personal voice
  feature is F76; older personal references are mapped to F76. A new source
  fingerprint record covers the actual integrated changes without rewriting
  historical source hashes.

## Verification

The integrated source passes frozen-lockfile installation, workspace build
(7 tasks), type checking (11 tasks) and 41 targeted desktop tests covering
stream compatibility, activation, known-owner recovery, live windows, stream
generation and archive bounds. ESLint also passes.

Android application and instrumentation-test Kotlin compilation pass in this
Windows environment, with compiler warnings. All 139 mobile-core JVM tests
pass with no failures or skips. These are not physical-device acceptance.

The Windows private-state suite passes all 84 tests, including isolated native
compilation and DACL checks. The separate history-identity, desktop-platform,
Windows helper-path and Windows-control smoke checks pass.

Mobile parity validates 76 feature definitions, their current source hashes,
generated tables, reading policy, contract vectors, design and localization.
Formatting, GitHub synchronization fixtures, ESLint, iOS runner fixtures,
TypeScript checks, basic service smoke and history smoke pass.

The full `pnpm check` does not pass: it stops at the Windows short/long path
assertion in `scripts/runtime-smoke.mjs:899`. That script and generation-settings
implementation are unchanged from the fetched personal main. The earlier
[PR3 integration record](pr3-main-integration-20261115.md) contains the existing
clean-baseline reproduction of this path discrepancy. Later steps in the
fail-fast command were not executed as part of that command.

Three additional fixture attempts are not passes: the prepared-bridge fixture
explicitly rejects Windows as POSIX-only; the first-input fixture fails while
loading its isolated bridge configuration; and the large-conversation fixture
fails on Windows private-state validation before reaching its assertions. These
fixture/environment boundaries remain unresolved; none is substituted for real
desktop or phone evidence. No permission checks or assertions were disabled.
Private logs stay in ignored `.local/github-sync-*.log` files.

## Delivery boundary

This is source synchronization only. It does not install a new APK, replace
the running Host, modify existing device identity or publish an app release.
No new physical Android run, Tailscale/WAN test, iOS build or macOS acceptance
has been performed for this integrated candidate. Historical device evidence
in earlier records remains tied to its original code and environment.
