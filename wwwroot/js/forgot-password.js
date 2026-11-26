
document.addEventListener("DOMContentLoaded", function () {

    "use strict";

    function translateMessage(en, vi) {
        return document.documentElement.lang
            .toLowerCase()
            .startsWith("vi") ? vi : en;
    }


    // ==========================================
    // 1. FORGOT PASSWORD FORM
    // ==========================================

    const forgotForm =
        document.getElementById("forgotPasswordForm");

    if (forgotForm) {

        const methods = forgotForm.querySelectorAll(
            'input[name="RecoveryMethod"]'
        );

        const phone = forgotForm.querySelector(
            'input[name="Phone"]'
        );

        const email = forgotForm.querySelector(
            'input[name="Email"]'
        );

        const message =
            document.getElementById("forgotPasswordMessage");

        const submitButton =
            forgotForm.querySelector('button[type="submit"]');

        function showMessage(en, vi) {
            if (message) {
                message.textContent =
                    translateMessage(en, vi);
            }
        }

        function updateRecoveryMethod() {

            const selected = forgotForm.querySelector(
                'input[name="RecoveryMethod"]:checked'
            );

            const isLocal =
                selected?.value !== "foreign";

            if (phone) {

                phone.disabled = !isLocal;
                phone.required = isLocal;

                const wrapper =
                    phone.closest(".register-field")
                    || phone.parentElement;

                wrapper.hidden = !isLocal;
            }

            if (email) {

                email.disabled = isLocal;
                email.required = !isLocal;

                const wrapper =
                    email.closest(".register-field")
                    || email.parentElement;

                wrapper.hidden = isLocal;
            }

            if (message) {
                message.textContent = "";
            }
        }

        methods.forEach(method => {

            method.addEventListener(
                "change",
                updateRecoveryMethod
            );

        });

        updateRecoveryMethod();

        forgotForm.addEventListener("submit", function (event) {

            const selected = forgotForm.querySelector(
                'input[name="RecoveryMethod"]:checked'
            );

            const method = selected?.value;

            if (method === "local") {

                const value = phone.value.trim();

                if (!/^0[35789][0-9]{8}$/.test(value)) {

                    event.preventDefault();

                    showMessage(
                        "Invalid Vietnamese phone number.",
                        "Số điện thoại Việt Nam không hợp lệ."
                    );

                    return;
                }

                phone.value = value;
            }
            else if (method === "foreign") {

                const value = email.value.trim();

                if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(value)) {

                    event.preventDefault();

                    showMessage(
                        "Invalid email address.",
                        "Địa chỉ email không hợp lệ."
                    );

                    return;
                }

                email.value = value;
            }
            else {

                event.preventDefault();

                showMessage(
                    "Please select a recovery method.",
                    "Vui lòng chọn phương thức khôi phục."
                );

                return;
            }

            if (submitButton) {
                submitButton.disabled = true;
            }

            showMessage(
                "Processing your request...",
                "Đang xử lý yêu cầu..."
            );

            // Allow HTTP POST to AccountController
        });

        window.addEventListener("pageshow", function () {

            if (submitButton) {
                submitButton.disabled = false;
            }

            updateRecoveryMethod();
        });
    }


    // ==========================================
    // 2. RESET PASSWORD FORM
    // ==========================================

    const resetForm =
        document.getElementById("resetPasswordForm");

    if (resetForm) {

        const password =
            document.getElementById("newPassword");

        const confirmPassword =
            document.getElementById("confirmNewPassword");

        const message =
            document.getElementById("resetPasswordMessage");

        const submitButton =
            resetForm.querySelector('button[type="submit"]');


        // Show / Hide Password

        resetForm.querySelectorAll("[data-password-target]")
            .forEach(button => {

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
                        hidden ? "Hide password" : "Show password"
                    );
                });
            });


        // Password requirements

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

            let valid = true;

            Object.entries(rules)
                .forEach(([id, rule]) => {

                    const passed = rule(password.value);

                    const element =
                        document.getElementById(id);

                    if (element) {

                        element.classList.toggle(
                            "valid",
                            passed
                        );
                    }

                    if (!passed) {
                        valid = false;
                    }
                });

            return valid;
        }

        function showResetMessage(en, vi) {

            if (message) {

                message.textContent =
                    translateMessage(en, vi);
            }
        }

        password.addEventListener(
            "input",
            validatePassword
        );


        // Submit new password

        resetForm.addEventListener("submit", function (event) {

            if (!validatePassword()) {

                event.preventDefault();

                showResetMessage(
                    "Password does not meet the requirements.",
                    "Mật khẩu chưa đáp ứng yêu cầu."
                );

                return;
            }

            if (password.value !== confirmPassword.value) {

                event.preventDefault();

                showResetMessage(
                    "Passwords do not match.",
                    "Mật khẩu xác nhận không trùng khớp."
                );

                return;
            }

            if (submitButton) {
                submitButton.disabled = true;
            }

            showResetMessage(
                "Resetting password...",
                "Đang đặt lại mật khẩu..."
            );

            // Allow HTTP POST to AccountController
        });

        window.addEventListener("pageshow", function () {

            if (submitButton) {
                submitButton.disabled = false;
            }

            validatePassword();
        });
    }

});
