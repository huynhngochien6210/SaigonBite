const themePreference = window.matchMedia(
    "(prefers-color-scheme: dark)"
);

function applyTheme() {
    const theme = themePreference.matches
        ? "dark"
        : "light";

    document.documentElement.setAttribute(
        "data-bs-theme",
        "light"
    );
}

// Apply theme automatically
applyTheme();

// Detect system theme changes
themePreference.addEventListener("change", applyTheme);