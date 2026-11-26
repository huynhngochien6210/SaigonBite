
document.addEventListener("DOMContentLoaded", () => {

    // Show / Hide Password
    document.querySelectorAll(".password-toggle")
        .forEach(button => {

            button.addEventListener("click", () => {

                const input = document.getElementById(
                    button.dataset.target
                );

                if (!input) return;

                const isHidden =
                    input.type === "password";

                input.type =
                    isHidden ? "text" : "password";

                const key = isHidden
                    ? "hidePassword"
                    : "showPassword";

                button.dataset.i18nAria = key;

                button.setAttribute(
                    "aria-label",
                    translate(key)
                );
            });
        });

    // Customer Login
    const loginForm =
        document.getElementById("loginForm");

    if (!loginForm) return;

    loginForm.addEventListener("submit", event => {

        const identifier =
            document.getElementById("identifier")
                .value.trim();

        const password =
            document.getElementById("password").value;

        const message =
            document.getElementById("loginMessage");

        const submitButton =
            loginForm.querySelector(
                'button[type="submit"]'
            );

        // Validate required fields
        if (!identifier || !password) {

            event.preventDefault();

            message.dataset.i18n = "requiredFields";

            message.textContent =
                translate("requiredFields");

            message.style.color = "#b90000";

            return;
        }

        // Prevent duplicate submissions
        submitButton.disabled = true;

        message.removeAttribute("data-i18n");

        message.style.color = "#16803c";

        message.textContent = "Signing in...";

        // Allow form submission to AccountController
    });
});
