
document.addEventListener("DOMContentLoaded", function () {

    "use strict";

    const form =
        document.getElementById("changePasswordForm");

    if (!form) return;

    const currentPassword =
        document.getElementById("currentPassword");

    const newPassword =
        document.getElementById("newPassword");

    const confirmPassword =
        document.getElementById("confirmNewPassword");

    const message =
        document.getElementById("changePasswordMessage");

    const submitButton =
        form.querySelector('button[type="submit"]');


    // ==========================================
    // 1. SHOW / HIDE PASSWORD
    // ==========================================

    form.querySelectorAll("[data-password-target]")
        .forEach(function (button) {

            button.addEventListener("click", function () {

                const target = document.getElementById(
                    button.dataset.passwordTarget
                );

                if (!target) return;

                const hidden =
                    target.type === "password";

                target.type =
                    hidden ? "text" : "password";

                button.setAttribute(
                    "aria-label",
                    hidden
                        ? "Hide password"
                        : "Show password"
                );
            });
        });


    // ==========================================
    // 2. PASSWORD REQUIREMENTS
    // ==========================================

    const rules = {

        ruleLength: value =>
            value.length >= 8 &&
            value.length <= 128,

        ruleUppercase: value =>
            /[A-Z]/.test(value),

        ruleLowercase: value =>
            /[a-z]/.test(value),

        ruleNumber: value =>
            /[0-9]/.test(value),

        ruleSpecial: value =>
            /[^A-Za-z0-9\s]/.test(value)
    };


    function validatePassword() {

        let isValid = true;

        Object.entries(rules)
            .forEach(function ([id, rule]) {

                const passed =
                    rule(newPassword.value);

                const element =
                    document.getElementById(id);

                if (element) {
                    element.classList.toggle(
                        "valid",
                        passed
                    );
                }

                if (!passed) {
                    isValid = false;
                }
            });

        return isValid;
    }


    newPassword.addEventListener(
        "input",
        validatePassword
    );


    // ==========================================
    // 3. BILINGUAL MESSAGE
    // ==========================================

    function showMessage(
        en,
        vi,
        validated = false
    ) {

        const isVietnamese =
            document.documentElement.lang
                .toLowerCase()
                .startsWith("vi");

        message.textContent =
            isVietnamese ? vi : en;

        message.classList.toggle(
            "validated",
            validated
        );
    }


    // ==========================================
    // 4. SUBMIT FORM
    // ==========================================

    form.addEventListener("submit", function (event) {

        message.textContent = "";

        message.classList.remove("validated");


        // Check current password
        if (!currentPassword.value) {

            event.preventDefault();

            showMessage(
                "Please enter your current password.",
                "Vui lòng nhập mật khẩu hiện tại."
            );

            return;
        }


        // Check password requirements
        if (!validatePassword()) {

            event.preventDefault();

            showMessage(
                "New password does not meet the requirements.",
                "Mật khẩu mới chưa đáp ứng yêu cầu."
            );

            return;
        }


        // Prevent password reuse
        if (
            currentPassword.value ===
            newPassword.value
        ) {

            event.preventDefault();

            showMessage(
                "New password must differ from current password.",
                "Mật khẩu mới phải khác mật khẩu hiện tại."
            );

            return;
        }


        // Check password confirmation
        if (
            newPassword.value !==
            confirmPassword.value
        ) {

            event.preventDefault();

            showMessage(
                "Passwords do not match.",
                "Mật khẩu xác nhận không trùng khớp."
            );

            return;
        }


        // Prevent duplicate submissions
        if (submitButton) {
            submitButton.disabled = true;
        }


        // Allow form submission to backend
        showMessage(
            "Changing password...",
            "Đang thay đổi mật khẩu...",
            true
        );

    });


    // Restore button when returning via browser history
    window.addEventListener("pageshow", function () {

        if (submitButton) {
            submitButton.disabled = false;
        }

        validatePassword();

    });

});
