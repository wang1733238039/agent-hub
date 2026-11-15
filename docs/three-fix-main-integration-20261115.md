# Three repair branches integrated into personal main

Date: 2026-11-15. Target: wang1733238039/agent-hub only.

## Scope and retained history

The starting personal main is cd3d4c25a8ebcb6500a4bf1083c4050307922bf6.
It already contained separately ported PR3 repairs, but did not contain the
original branch ancestry or all complementary new-chat and Windows changes.
This integration joins all three original histories with normal merge commits:

| Branch                               | Integrated tip                           |
| ------------------------------------ | ---------------------------------------- |
| codex/windows-cold-thread-recovery   | 5ba50a221d2e2187ecaa37f1534e65a3f4b2b9a2 |
| codex/mobile-new-chat-destination    | 1c770eb80d773dfd3b9ff59717cadc786af30829 |
| codex/fix-empty-thread-event-reloads | d4e247ef453b7df80c8184a7bcfb08a67c6c61ef |

The starting main remains available at
codex/before-three-fix-integration-20261115. Original branches are retained.
No upstream push, force push, APK installation or production deployment is part
of this source integration.

## Reviewed resolutions

- Preserve PR3 filtered-event readiness, snapshot ordering, usable earlier-page
  controls and history/live message identity reconciliation.
- Restore the actionable new-chat project/folder picker and draft destination.
  Retain later guards against stale navigation, connection and selection targets,
  including project paging, unavailable-state guidance and first-send creation.
- Enable the opted-in Windows default private bridge while preserving explicit
  drivers, existing policy and stable Host identity. Retain durable explicit-open
  receipts so an unknown result remains queryable after restart without replay.
- Resolve the trusted desktop CLI payload before choosing an identical official
  staged copy. Remove an automatically merged older shortcut that would otherwise
  bypass the newer ambiguity check.
- Preserve desktop stream profiles 11/13, packaged activation, private-state
  protections, resource identity rules, personal application/signing identities,
  continuous voice, updater configuration and the 15-message reading window.
- Retain supporting Host metadata-process lifecycle changes and the newer iOS
  restoration work carried by the original branch histories. Historical device
  evidence remains historical; it does not certify this combined candidate.

## Candidate verification

- Workspace build: 7 successful tasks. Type checking: 11 successful tasks.
- Android application and instrumentation Kotlin compilation: successful with
  existing deprecation warnings; no phone installation or instrumentation run.
- Fresh shared-core JVM run: 139 tests, 29 suites, zero failures or skips.
- Targeted desktop/bridge/open-receipt/lifecycle/stream regression runner:
  64 tests, 63 passed, zero failed, one explicitly skipped POSIX-only scenario.
  The macOS Trace inventory file also self-skips its body without macOS lsof;
  its runner-level file result is not Windows inventory acceptance.
- Native Windows private-state suite: 84 passed, zero failures or skips,
  including isolated C# compilation and actual Windows permission checks.
- History identity, desktop platform and Windows staged-CLI recovery smoke
  checks passed. These fixture results do not establish production phone control.
- The 76-feature parity manifest, generated tables, localization, protocol vectors,
  workspace formatting and ESLint passed during candidate verification.

The first targeted run reported three failures. One fixture used URL.pathname
as a Windows executable path, producing a duplicated drive prefix. It now uses
fileURLToPath; its catchable-SIGTERM/shebang shutdown scenario is explicitly
POSIX-only and is not claimed as Windows shutdown evidence. For the two delayed
close scenarios, Windows does not reproduce the POSIX inherited stdout-holder
behavior. The Windows fixture now delays only the close notification after a
real child exit. All replacement-fence and no-replay assertions remain in place;
this is controlled notification evidence, not a Windows inherited-pipe result.
The original failed log is retained privately.

## Checks that did not pass

The full pnpm check was executed and stopped at scripts/runtime-smoke.mjs:899:
the configured writable-root spelling differs from Windows realpath output.
That script is unchanged from the starting personal main. The earlier
[main synchronization record](github-main-sync-20261115.md) and
[PR3 integration record](pr3-main-integration-20261115.md) retain prior evidence.
Later commands in the fail-fast chain were not executed by that full run.

The separately attempted pnpm check:skill reported 83 tests: 76 passed,
5 failed and 2 skipped. The failures occur in the unchanged probe-host fixtures
at Windows private-file validation, before their expected endpoint/error
assertions. Neither the probe implementation nor those assertions were weakened.
Static skill inspection passed with an existing entry-point warning; only the
two bridge configuration reference paragraphs changed in this integration.

Private logs remain in ignored .local/three-fix-*.log files. The record does not
claim a fully passing repository suite, iOS/Xcode compilation, physical Android
acceptance, Tailscale/WAN acceptance or production Host replacement.
