
document.addEventListener("DOMContentLoaded", () => {

    // Show / Hide Password

    document.querySelectorAll(".password-toggle")
        .forEach(button => {

            button.addEventListener("click", () => {

                const input = document.getElementById(
                    button.dataset.target
                );

                if (!input) return;

                const isHidden = input.type === "password";

                input.type = isHidden ? "text" : "password";

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

    // Login Form Testing

    const loginForm =
        document.getElementById("loginForm");

    if (loginForm) {

        loginForm.addEventListener("submit", event => {

            event.preventDefault();

            const identifier =
                document.getElementById("identifier")
                    .value.trim();

            const password =
                document.getElementById("password").value;

            const message =
                document.getElementById("loginMessage");

            const key = (!identifier || !password)
                ? "requiredFields"
                : "loginDemo";

            message.dataset.i18n = key;
            message.textContent = translate(key);
        });
    }
});
