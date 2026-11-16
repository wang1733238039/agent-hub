# Android EasyWork replay recovery, 2026-11-16

## Problem and implementation

The affected phone submits messages successfully, and the desktop generates replies, but the phone does not show either record. Pairing and successful TLS do not establish that history or live events reached the UI.

EasyWork 0.1.365 was observed to stop full replay at 100 pages of 500 events and report the failure as WebSocket 1008 / invalid encrypted channel. One affected conversation exceeds that limit. The desktop installation is unchanged.

Android previously canceled LAN reception when its 64-frame queue filled, even within its 64 MiB byte budget. The bounded queue now waits on the dedicated OkHttp reader for capacity; the coroutine receiver does not block Main. Receiving, cancellation handoff and closing release capacity. The 32 MiB frame limit remains.

Within the same authenticated runtime session, reconnection now subscribes after the last event actually received, rather than restarting from zero after every socket epoch. The server close-frame callback starts recovery promptly. The cursor is session-local; cold starts and unsubscribing can still encounter the desktop cap. This change does not permanently remove that desktop defect.

No TLS checks, credentials, bindings or message history are changed. Already submitted input is not replayed. iOS is unchanged and receives no new acceptance claim.

## Personal-fork integration and package provenance

The source destination is `wang1733238039/agent-hub`, based on personal `main` at `f31d2b39ff1b51aadd4db07d963986816092fd1f`. Only the replay changes, focused checks, the JVM test dependency and parity records are ported. Personal application IDs, signing settings, voice features and update behavior are preserved. Pre-existing version/export and settings edits in the source working tree are not included.

The earlier local r2 APK came from an upstream development worktree, with application ID `com.easyai.agenthub.android.debug`, display version `0.1.1-dev`, and SHA-256 `958507cfe7fb5f097ccec2b5df8d72c53c1e5130f6a7a5f641dd25f1e1d03602`. It is not an artifact built from this personal-fork commit. The personal fork uses `com.codexmobile.personal.debug` for DEV and retains its existing release-signing configuration. These identities are not interchangeable for an in-place update.

The user confirmed that the earlier APK was not received. No delivery or installation success is claimed. A phone update must match the installed application's package and signing identity, and must preserve data. Do not uninstall or clear data to force compatibility.

## Validation boundaries

The originating candidate passed four production-queue JVM checks, Android application compilation and instrumentation-source compilation. An independent encrypted peer observed reconnect cursors 0 then 7. A read-only Windows JVM probe using the current Kotlin protocol reached event 51,858 after one reconnect in one desktop conversation and 36,720 in another; both subsequently answered runtime/context. Actual sync data also passed history merge and visible-row projection. These probes used explicit substitutes for Android Base64/Uri and the JVM OkHttp platform. They do not establish Android radio, Android TLS-provider or Compose rendering acceptance.

Those results establish the originating candidate only. The personal fork's focused checks and compilation are recorded separately below. The affected Redmi K80 has not accepted this change; its installed APK fingerprint is unknown. No testing or installation is performed on an unrelated connected Android device. Full repository CI and the permanent desktop replay correction remain separate gates.

## Personal-fork checks

On native Windows, `:android:testDebugUnitTest :android:assembleDebug :android:compileDebugAndroidTestKotlin --offline` passed. The production-queue suite reports four tests, zero failures, zero errors and zero skips. The Android instrumentation code was compiled only; no affected-phone execution is claimed. The Node workspace build also passed.

The local personal DEV build has application ID `com.codexmobile.personal.debug`, version `0.2.0-dev`, versionCode 1 and APK SHA-256 `ff6620ca580b33d6a11ef3a2d131e5260c3689d1d85cfecc396643ab2b2c0096`. APK signature verification passed with certificate SHA-256 `6fec4affeb5f10932b37f38e41742594d5ee0cb55e6ceeb256060a2b42876f25`. This is compilation evidence, not an offered in-place update to the differently identified Agent Hub phone installation. No GitHub Release has been published.

After building the Node workspace, `pnpm check` passed formatting, five Git sync checks, ESLint, mobile parity/schema/generated-table checks, the associated contract checks, typecheck and initial service/history checks. It stopped at the unchanged `scripts/runtime-smoke.mjs:899` assertion that `wire.sandboxPolicy.writableRoots` includes the real workspace path on Windows. The later chained checks did not execute, and the complete check is not passed. No unrelated runtime code was edited to conceal the failure. Remote CI and phone acceptance remain separate.

## Compatible Agent Hub DEV candidate (r3)

The user identified the earlier installation source directory. Its retained APK has already been overwritten by the r2 build, so its fingerprint does not establish the bytes installed on the phone. It identifies the intended compatibility target only.

The personal fork now has an explicit `-PagentHubLegacyUpdate=true` profile: `com.easyai.agenthub.android.debug`, display name Agent Hub, version `0.1.2-dev`, versionCode 1002. Output is isolated under `.build/native/android-agenthub-legacy`. This profile rejects acceptance mode and disables release variants. Personal defaults remain `com.codexmobile.personal.debug`, `0.2.0-dev`, code 1, with the original release signing configuration.

The compatible candidate's APK signature was verified with certificate SHA-256 `6fec4affeb5f10932b37f38e41742594d5ee0cb55e6ceeb256060a2b42876f25`, matching the retained APK from the source directory. Android installation must still confirm compatibility with the actual phone app. Do not uninstall or clear data on rejection.

Release bodies declare `codex-mobile-application-id` so the personal updater skips this legacy candidate, and the legacy updater rejects undeclared personal releases. Each such Release must contain APKs for only the declared installation identity. Existing unlabelled personal releases remain compatible with the personal updater. Android's installer remains the final package/signature check.

The compatible profile passed eight JVM tests (four production receive-queue and four update-identity boundary tests), application assembly and instrumentation-source compilation on native Windows. The personal default profile was also rebuilt and tested. No instrumentation or install was performed on the affected Redmi K80. Distribution records bind the final source commit, APK hash, version, package and certificate; publishing a preview does not establish phone delivery or acceptance.

Phone acceptance: install this candidate over the existing App, open the affected original EasyWork conversation and observe already-submitted messages and replies, then leave/reopen it and check subsequent replies. Do not resend earlier input to recreate missing history. Record installation rejection or remaining empty display accurately. The desktop's 50,000-event replay cap and cold-start/unsubscribe limitation remain open.
