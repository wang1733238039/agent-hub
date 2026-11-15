# Personal main: PR3 integration

Date: 2026-11-15. This record describes source integration into the personal fork.
It is not a production deployment or a completed physical-device acceptance.

## Scope and provenance

The integration starts at personal `main` commit
`9aee5097a9ac9a423a0f58e56205e5d86fdc4c83` on branch
`codex/integrate-pr3-main`.

| Original PR3 commit | Personal integration commit | Repair                                                                                                             |
| ------------------- | --------------------------- | ------------------------------------------------------------------------------------------------------------------ |
| `c0ff003`           | `45ee050`                   | Keep controls ready on thread-filtered empty event batches; keep earlier paging usable during background reads.    |
| `527b800`           | `a812b1d`                   | Prevent an older history/event response from replacing a newer snapshot or live runtime.                           |
| `d4e247e`           | `ae4ec1a`                   | Reconcile ordinal message IDs across history-reader and original-owner transitions without comparing message text. |

These are selected changes adapted to the older personal main, rather than a
merge of all intervening upstream changes. The original PR3 branch is therefore
not itself an ancestor of the integrated main. The integration branch contains
the adapted commits and the final personal-package test adjustments.

The personal continuous-voice implementation, in-app update implementation,
package identity, private signing configuration and initial 15-message reading
window are retained. Android tests target only the independent personal
acceptance package `com.codexmobile.personal.acceptance`.

The later stream13 desktop adaptation and the uncommitted Android version-number
changes in the original development checkout are outside this integration.
Existing development changes and installed production services are preserved.

## Verification performed on this integration

| Check                                                                           | Result                                                                                        |
| ------------------------------------------------------------------------------- | --------------------------------------------------------------------------------------------- |
| `pnpm build`                                                                    | Passed: six workspace build tasks.                                                            |
| Android debug app and instrumentation compilation                               | Passed with `-PagentHubAcceptance=true`.                                                      |
| `:mobile-core:jvmTest`                                                          | Passed: 101 tests, zero failures/errors/skips.                                                |
| Known-owner recovery, desktop live window and stream generation tests           | Passed: 28 tests, zero failures/skips.                                                        |
| Desktop history identity smoke                                                  | Passed.                                                                                       |
| Large conversation synchronization smoke                                        | Passed: synthetic 70 MiB owner snapshot and 241 historical messages paged without duplicates. |
| Runtime persistence smoke                                                       | Passed: durable receipts, cold-history isolation, coalescing and shutdown.                    |
| Mobile parity definitions and six parity-tool tests                             | Passed for the integrated sources.                                                            |
| Formatting, lint, TypeScript checks, general service smoke and history boundary | Passed before the full check reached the runtime failure below.                               |
| `git diff --check`                                                              | Passed.                                                                                       |

## Full-check limitations and baseline comparison

`pnpm check` was executed and did not pass in full. Its normal fail-fast behavior
means later checks in that command must not be reported as having run.

1. With the default Windows temporary directory, `runtime-smoke.mjs:845` fails
   because the sandbox root uses the Windows short username path while
   `realpath(root)` returns the long username path. A diagnostic copy on a clean
   pre-integration main reproduces the same assertion and prints both forms.
2. With `TEMP` and `TMP` set to a full-path temporary directory in each isolated
   worktree, both the integration and clean pre-integration main pass that
   assertion and fail at `runtime-smoke.mjs:1084`, an assistant attachment
   projection assertion. This comparison establishes that the observed failure
   is present before these PR3 changes; it does not establish that the attachment
   behavior is correct. No production assertions were disabled.
3. The extra voice transcription suite passes five of six tests. The remaining
   test fails when Windows denies creation of a file symbolic link (`EPERM`).
   The clean pre-integration main reproduces the same result. This test setup
   limitation is recorded separately from voice product behavior.
4. The extra desktop IPC smoke progresses through the oversized-payload and
   asynchronous-question fixtures, then Windows denies its file-symbolic-link
   fixture at `desktop-ipc-smoke.mjs:2643` (`EPERM`). The complete IPC smoke is
   therefore not a pass.

Local evidence is stored in each worktree's ignored `.local/` directory:
`pr3-check-final.log`, `pr3-check-canonical-temp.log`, `pr3-host-tests.log`,
`pr3-large-conversation.log`, `pr3-persistence.log`, `pr3-voice.log`, and
`pr3-desktop-ipc.log`.
The clean baseline worktree retains `baseline-runtime.log`,
`baseline-runtime-canonical-temp.log`, and `baseline-voice.log`.
Raw machine-local paths and private runtime files are not published.

## Device and publication status

- The Android acceptance app was built, but installation returned
  `INSTALL_FAILED_USER_RESTRICTED`. The user explicitly deferred installation
  and physical-device testing. No new integrated-candidate phone test is claimed.
- Earlier PR3 device records are historical evidence for their own fingerprints;
  they are not a pass for this adapted integration.
- iOS build and physical-device validation were not performed on this Windows
  host; Xcode is unavailable.
- The configured personal GitHub remote could not be read: attempts returned
  repository-not-found or connection failure. No successful push or public
  main update is claimed.
- Source integration does not install a new app or restart the production Host.

Physical Android acceptance and resolution of the baseline check failures remain
necessary before calling this a fully validated release.
