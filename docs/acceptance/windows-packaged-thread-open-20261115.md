# Windows packaged desktop: explicit old-thread activation

## Failure and cause

The installed adapter already supported stream profiles 11 and 13. An old,
unarchived thread had readable persisted history but no live original owner.
Its explicit Host open stayed `opening` and became `outcome-unknown` after
31.5 seconds. Reading history did not load it into the original desktop.

The Windows helper used only `Start-Process codex://threads/<id>`, leaving
application selection to the default URI association. The current Store app
did not receive the requested navigation through that path. Native Codex
navigation loaded the same original thread successfully. A separate targeted
Windows `IApplicationActivationManager` probe also restored an unarchived
thread, retaining the same original desktop pipe PID.

The local scheme registry contained only its URL marker. This observation
alone does not prove invalid registration for an MSIX application. The repair
addresses the proven missing behavior: activation of the authenticated running
application rather than dependence on default protocol dispatch.

## Change

- Keep the authenticated original IPC pipe and same-user/session checks.
- Query AUMID from that process's retained handle and activate that exact
  packaged application with the fixed thread URI and no model input.
- Only `APPMODEL_ERROR_NO_APPLICATION` permits the existing unpackaged URI
  path. Other errors stay unknown; there is no activation or input replay.
- Continue waiting for a fresh exact thread owner before returning `opened`.
  A dispatcher PID, launcher exit or readable history is not confirmation.
- Include the new C# helper and pipe helper in the prepared adapter digest.
- If the Host runs elevated while the authenticated desktop is ordinary,
  launch only the fixed navigation helper with the same user's interactive
  shell token and a clean environment. The helper reconnects and validates the
  original pipe again before activation. It cannot submit model input, stop
  the desktop, choose arbitrary commands or request UAC. This path is necessary
  even when the scheduled task declares `Limited`: this machine's actual
  scheduled Host still receives an elevated token.

This Windows Host behavior is shared by iOS and Android. Their UI, drafts,
operation IDs and message submission contracts are unchanged.

## Current evidence

- Workspace build: seven projects successful.
- Native activation boundaries, durable open receipts, bridge defaults,
  existing-owner recovery and stream profiles: 28 checks passed.
  These are auxiliary tests, not a physical phone result.
- A separate real Host, private state and loopback port opened an unarchived
  old thread through `/v1/desktop/open`. Before: `canControl=false`; after:
  `phase=opened`, `canControl=true`, idle, with 100 visible history messages.
  Owner confirmation took 2.616 seconds. A second unarchived cold thread
  independently passed in 2.604 seconds. No model input was submitted.
- The formal installed Host then opened two additional cold conversations in
  3.7 and 4.2 seconds, preserving the original desktop PID. The previously
  reported cold conversation now returns HTTP 200 history, fresh control, and
  an `opened` receipt.
- A physical Android acceptance run used the already-installed test app and
  the formal Host. It selected a dedicated unarchived cold test conversation,
  retained its four original rows exactly once, performed one open and one
  test reply, then returned to idle and writable with six rows. This verifies
  the phone-to-Host-to-original-desktop path and the history-to-live transition;
  no business conversation was modified and no APK was installed.
- Type checking: 11 projects successful. Lint passed. The separate POSIX
  prepared-bridge smoke refuses Windows by design and was not counted as passed.
- An earlier candidate from stale catalog metadata was actually archived;
  it remained unknown. The repair does not silently unarchive such records.
  That attempt is retained as failed evidence, not counted as a pass.
- `pnpm check` passed formatting, GitHub-sync fixtures and lint, then stopped
  in the parity audit because its configured baseline commit was absent from
  this local history. No check was removed or bypassed.

Private evidence is under the worktree's ignored `.local/` directory. No real
titles, conversation exports, credentials or machine paths are published here.

## Delivery status

The final source is commit `188adc01dc02eb2aaa64017af6285587f5462b9c` and the
frozen Host runtime was verified against its 1,903-file manifest. The formal
Host was recovered once during investigation; final helper files were applied
hot without another Host or desktop restart. Host ID, access token and original
desktop PID were preserved. The existing Android acceptance APK was used over
USB reverse networking; Tailscale/WAN reachability was not re-tested. The
unpackaged-Codex fallback was not exercised against a real unpackaged desktop.
macOS behavior is unchanged and not newly verified. iOS was not tested.

## Personal main integration

The deployed Host and the physical Android results above belong to the frozen
Host source at `188adc01dc02eb2aaa64017af6285587f5462b9c`. They are not a
physical-device pass for every source file in personal main.

The corresponding narrow repair is ported to personal main after `acbf693`:
stream profiles 11/13, exact packaged-app navigation, fixed same-user navigation
helper and its boundary tests. Personal main has an older adapter/catalog
implementation, so unrelated upstream changes are not imported. Build, lint,
types and regression results for this adapted source are recorded separately
in [the integration record](../cold-open-main-integration-20261115.md).

No new APK is installed and the production Host is not replaced again by this
source integration. Existing desktop threads, phone connection, drafts and
credentials are preserved. Remote Git publication is outside this integration.
