package com.agenthub.android

/** Release metadata separates the legacy update from personal installation identities. */
internal fun updateReleaseMatchesApplication(
    body: String,
    applicationId: String,
    requireDeclaredApplicationId: Boolean,
): Boolean {
    val declarations = Regex("""(?im)^\s*codex-mobile-application-id\s*:\s*(\S+)\s*$""")
        .findAll(body).map { it.groupValues[1] }.toList()
    return if (declarations.isEmpty()) !requireDeclaredApplicationId
    else declarations.size == 1 && declarations.single() == applicationId
}
