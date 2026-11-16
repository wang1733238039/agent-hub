package com.agenthub.android

import org.junit.Assert.*
import org.junit.Test

class AppUpdateIdentityTest {
    private val legacy = "com.easyai.agenthub.android.debug"
    private val personal = "com.codexmobile.personal.debug"
    private val legacyBody = "Candidate\n\ncodex-mobile-application-id: $legacy\n"

    @Test fun legacyCandidateIsNotOfferedToPersonalApp() {
        assertFalse(updateReleaseMatchesApplication(legacyBody, personal, false))
        assertTrue(updateReleaseMatchesApplication(legacyBody, legacy, true))
    }

    @Test fun legacyAppRejectsUndeclaredPersonalReleases() {
        assertFalse(updateReleaseMatchesApplication("Personal release", legacy, true))
        assertTrue(updateReleaseMatchesApplication("Personal release", personal, false))
    }

    @Test fun declaredPersonalReleaseCannotUpdateLegacyApp() {
        val body = "codex-mobile-application-id: $personal"
        assertFalse(updateReleaseMatchesApplication(body, legacy, true))
        assertTrue(updateReleaseMatchesApplication(body, personal, false))
    }

    @Test fun ambiguousDeclarationsAreRejected() {
        assertFalse(updateReleaseMatchesApplication(legacyBody + legacyBody, legacy, true))
        assertFalse(updateReleaseMatchesApplication(
            legacyBody + "codex-mobile-application-id: $personal", personal, false,
        ))
    }
}
