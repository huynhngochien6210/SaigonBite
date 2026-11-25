
document.addEventListener("DOMContentLoaded", function () {

    // Bilingual messages

    function showMessage(element, en, vi) {

        const isVietnamese =
            document.documentElement.lang
                .toLowerCase().startsWith("vi");

        element.textContent = isVietnamese ? vi : en;

    }

    // ========================================
    // FORGOT PASSWORD
    // ========================================

    const forgotForm =
        document.getElementById("forgotPasswordForm");

    if (forgotForm) {

        const methods = document.querySelectorAll(
            'input[name="recoveryMethod"]'
        );

        const phoneField =
            document.getElementById("recoveryPhoneField");

        const emailField =
            document.getElementById("recoveryEmailField");

        const phoneInput =
            document.getElementById("recoveryPhone");

        const emailInput =
            document.getElementById("recoveryEmail");

        const message =
            document.getElementById("forgotPasswordMessage");

        function updateRecoveryMethod() {

            const selected = document.querySelector(
                'input[name="recoveryMethod"]:checked'
            );

            const usePhone = selected.value === "local";

            phoneField.hidden = !usePhone;
            emailField.hidden = usePhone;

            phoneInput.disabled = !usePhone;
            emailInput.disabled = usePhone;

            phoneInput.required = usePhone;
            emailInput.required = !usePhone;

            message.textContent = "";

        }

        methods.forEach(function (method) {

            method.addEventListener(
                "change",
                updateRecoveryMethod
            );

        });

        forgotForm.addEventListener("submit", function (event) {

            event.preventDefault();

            const selected = document.querySelector(
                'input[name="recoveryMethod"]:checked'
            );

            const type = selected.value;

            message.textContent = "";

            if (type === "local") {

                const phone = phoneInput.value.trim();

                if (!/^0[0-9]{9}$/.test(phone)) {

                    showMessage(
                        message,
                        "Please enter a valid 10-digit phone number.",
                        "Vui lòng nhập số điện thoại hợp lệ gồm 10 chữ số."
                    );

                    return;

                }

            } else {

                if (!emailInput.validity.valid) {

                    showMessage(
                        message,
                        "Please enter a valid email address.",
                        "Vui lòng nhập địa chỉ email hợp lệ."
                    );

                    return;

                }

            }

            // Temporary frontend navigation

            window.location.href =
                "/Account/VerifyResetCode?type=" +
                encodeURIComponent(type);

        });

        updateRecoveryMethod();

    }

    // ========================================
    // RESET PASSWORD
    // ========================================

    const resetForm =
        document.getElementById("resetPasswordForm");

    if (resetForm) {

        const passwordInput =
            document.getElementById("newPassword");

        const confirmInput =
            document.getElementById("confirmNewPassword");

        const message =
            document.getElementById("resetPasswordMessage");

        // Show / Hide Password

        resetForm.querySelectorAll("[data-toggle-password]")
            .forEach(function (button) {

                button.addEventListener("click", function () {

                    const input = document.getElementById(
                        button.dataset.togglePassword
                    );

                    if (!input) return;

                    const isHidden = input.type === "password";

                    input.type = isHidden
                        ? "text"
                        : "password";

                    button.setAttribute(
                        "aria-label",
                        isHidden ? "Hide password" : "Show password"
                    );

                });

            });

        // Password validation rules

        function getPasswordRules(password) {

            return {

                ruleLength: password.length >= 8,

                ruleUppercase: /[A-Z]/.test(password),

                ruleLowercase: /[a-z]/.test(password),

                ruleNumber: /[0-9]/.test(password),

                ruleSpecial: /[^A-Za-z0-9\s]/.test(password)

            };

        }

        // Realtime password validation

        function updatePasswordRules() {

            const rules = getPasswordRules(
                passwordInput.value
            );

            Object.entries(rules).forEach(function ([id, valid]) {

                const element = document.getElementById(id);

                element.classList.toggle("valid", valid);

            });

            return Object.values(rules).every(Boolean);

        }

        passwordInput.addEventListener("input", function () {

            updatePasswordRules();

            message.textContent = "";

        });

        confirmInput.addEventListener("input", function () {

            message.textContent = "";

        });

        // Submit reset password

        resetForm.addEventListener("submit", function (event) {

            event.preventDefault();

            if (!updatePasswordRules()) {

                showMessage(
                    message,
                    "Password does not meet all requirements.",
                    "Mật khẩu chưa đáp ứng đầy đủ các yêu cầu."
                );

                return;

            }

            if (passwordInput.value !== confirmInput.value) {

                showMessage(
                    message,
                    "Passwords do not match.",
                    "Mật khẩu xác nhận không trùng khớp."
                );

                return;

            }

            // Temporary frontend navigation

            window.location.href =
                "/Account/ResetPasswordSuccess";

        });

        updatePasswordRules();

    }

});
