document.addEventListener("DOMContentLoaded", function () {

    const form = document.getElementById("changePasswordForm");

    if (!form) return;

    const currentPassword =
        document.getElementById("currentPassword");

    const newPassword =
        document.getElementById("newPassword");

    const confirmPassword =
        document.getElementById("confirmNewPassword");

    const message =
        document.getElementById("changePasswordMessage");

    // Show / Hide Password

    document.querySelectorAll("[data-password-target]")
        .forEach(function (button) {

            button.addEventListener("click", function () {

                const target = document.getElementById(
                    button.dataset.passwordTarget
                );

                const hidden = target.type === "password";

                target.type = hidden ? "text" : "password";

                button.setAttribute(
                    "aria-label",
                    hidden ? "Hide password" : "Show password"
                );

            });

        });

    // Password Requirements

    const rules = {

        ruleLength: value => value.length >= 8,

        ruleUppercase: value => /[A-Z]/.test(value),

        ruleLowercase: value => /[a-z]/.test(value),

        ruleNumber: value => /[0-9]/.test(value),

        ruleSpecial: value =>
            /[^A-Za-z0-9\s]/.test(value)

    };

    function validatePassword() {

        let isValid = true;

        Object.entries(rules).forEach(function ([id, rule]) {

            const passed = rule(newPassword.value);

            document.getElementById(id)
                .classList.toggle("valid", passed);

            if (!passed) {
                isValid = false;
            }

        });

        return isValid;

    }

    newPassword.addEventListener("input", validatePassword);

    // Bilingual Message

    function showMessage(en, vi, validated = false) {

        const isVietnamese =
            document.documentElement.lang
                .toLowerCase().startsWith("vi");

        message.textContent = isVietnamese ? vi : en;

        message.classList.toggle("validated", validated);

    }

    // Submit Form

    form.addEventListener("submit", function (event) {

        event.preventDefault();

        message.textContent = "";

        if (!currentPassword.value) {

            showMessage(
                "Please enter your current password.",
                "Vui lòng nhập mật khẩu hiện tại."
            );

            return;
        }

        if (!validatePassword()) {

            showMessage(
                "New password does not meet the requirements.",
                "Mật khẩu mới chưa đáp ứng yêu cầu."
            );

            return;
        }

        if (currentPassword.value === newPassword.value) {

            showMessage(
                "New password must differ from current password.",
                "Mật khẩu mới phải khác mật khẩu hiện tại."
            );

            return;
        }

        if (newPassword.value !== confirmPassword.value) {

            showMessage(
                "Passwords do not match.",
                "Mật khẩu xác nhận không trùng khớp."
            );

            return;
        }

        form.reset();

        form.querySelectorAll("[data-password-target]")
            .forEach(function (button) {

                document.getElementById(
                    button.dataset.passwordTarget
                ).type = "password";

                button.setAttribute(
                    "aria-label",
                    "Show password"
                );

            });

        validatePassword();

        showMessage(
            "Validation successful. Backend is not connected yet.",
            "Dữ liệu hợp lệ. Backend chưa được kết nối.",
            true
        );

    });

});