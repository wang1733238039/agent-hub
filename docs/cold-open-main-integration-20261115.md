# Personal main: Windows cold-thread opening repair

Date: 2026-11-15. Base: personal main `acbf693`.

## Behavior and scope

Persisted old history was readable while the original desktop had not acquired
the thread. Two separate compatibility gaps are addressed: accepting observed
stream profiles 11 and 13, and routing the exact thread URI to the authenticated
running MSIX application. Elevated Host activation uses only the fixed helper
under the same user's interactive shell; the helper verifies the desktop pipe.
Unknown profiles still revoke control. Navigation or process-launch success is
not sufficient: an exact fresh original owner must confirm control.

These changes are ported onto personal main without unrelated upstream updates.
Personal Android package identity, signing/update configuration, continuous
voice and the initial 15-message reading window are unchanged. This task does
not update or install the user's APK or publish remote Git changes.

## Evidence separation

The formal Host is already repaired using source `188adc0`. Two actual cold
opens completed in 3.661 and 4.218 seconds with the same original desktop PID.
The reported conversation's persisted open receipt is `opened`; a fresh
read confirmed HTTP 200, `canControl=true`, and idle. This checks the actual
reported target without sending model input to a business conversation.

The existing Android acceptance app completed one dedicated cold-thread test:
four original rows each once, one explicit open, one test input, one reply,
then idle and writable with six rows. The device result was backed up and
restored. No new APK was installed. This evidence belongs to the deployed
Host source and existing acceptance app, not a newly built personal APK.

The separate source/fixture checks below are for this adapted integration.
The full PR3 validation limitations remain documented in
[the earlier integration record](pr3-main-integration-20261115.md).

## Adapted-source checks

| Check                                                                                                     | Actual result                                                                                                                                                                                                                                            |
| --------------------------------------------------------------------------------------------------------- | -------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| Workspace build                                                                                           | Six tasks successful.                                                                                                                                                                                                                                    |
| Type checking                                                                                             | Ten tasks successful.                                                                                                                                                                                                                                    |
| Stream profiles, native activation boundaries, known-owner recovery, live windows and generation recovery | 33 tests passed, no failures or skips.                                                                                                                                                                                                                   |
| Desktop platform smoke                                                                                    | Passed, including profile 13, unknown profiles and other method-version rejection. Synthetic fixtures only.                                                                                                                                              |
| Windows native helper path                                                                                | Passed the synthetic MSIX mapping and launcher/permission transport checks. This is not real unpackaged-desktop activation.                                                                                                                              |
| Mobile parity                                                                                             | 73 feature definitions and six parity-tool tests passed; generation, HTTP/schema, private connection, design and failure-copy checks passed.                                                                                                             |
| Full `pnpm check`                                                                                         | Failed at the existing Windows path-canonicalization assertion in `runtime-smoke.mjs:845`. Formatting, sync fixtures, lint, parity, type checks, basic service and history checks completed first. Later steps in the fail-fast chain were not executed. |

The full-check assertion and the clean pre-PR3 baseline comparison are retained
in [the earlier integration record](pr3-main-integration-20261115.md). No assertion
was removed, no result was rewritten, and no full-product pass is claimed.
Private logs use the ignored `.local/cold-main-*.log` files. Source validation
does not replace the deployed-Host or physical-phone evidence above.

## Unverified paths

No new iOS or macOS run, no Tailscale/WAN test, and no new APK installation.
No real unpackaged-Codex activation was exercised. The private Host installation
and identity are retained; this integration does not replace production again.
