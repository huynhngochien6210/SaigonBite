
document.addEventListener("DOMContentLoaded", function () {

    // 1. Select Account Type

    const continueButton =
        document.getElementById("continueRegister");

    if (continueButton) {

        continueButton.addEventListener("click", function () {

            const selected = document.querySelector(
                'input[name="accountType"]:checked'
            );

            if (!selected) return;

            if (selected.value === "local") {
                window.location.href = "/Account/RegisterLocal";
            } else if (selected.value === "foreign") {
                window.location.href = "/Account/RegisterForeign";
            }
        });
    }

    // 2. Show / Hide Password

    document.querySelectorAll("[data-toggle-password]")
        .forEach(function (button) {

            button.addEventListener("click", function () {

                const input = document.getElementById(
                    button.dataset.togglePassword
                );

                if (!input) return;

                const isHidden = input.type === "password";

                input.type = isHidden ? "text" : "password";

                button.setAttribute(
                    "aria-label",
                    isHidden ? "Hide password" : "Show password"
                );
            });
        });

    // 3. Registration Validation

    const form =
        document.getElementById("registrationForm");

    if (!form) return;

    const message =
        document.getElementById("registerMessage");

    function showMessage(en, vi) {

        const isVietnamese =
            document.documentElement.lang
                .toLowerCase().startsWith("vi");

        message.textContent = isVietnamese ? vi : en;
        message.style.color = "#b90000";
    }

    form.addEventListener("submit", function (event) {

        message.textContent = "";

        const fullName =
            document.getElementById("fullName").value.trim();

        const password =
            document.getElementById("registerPassword").value;

        const confirmPassword =
            document.getElementById("confirmPassword").value;

        const phone =
            document.getElementById("phone");

        const email =
            document.getElementById("email");

        const passport =
            document.getElementById("passportNumber");

        const nationality =
            document.getElementById("nationality");

        // Full Name

        if (fullName.length === 0 || fullName.length > 100) {

            event.preventDefault();

            showMessage(
                "Please enter a valid full name.",
                "Vui lòng nhập họ và tên hợp lệ."
            );

            return;
        }

        // Phone Number - BR18

        if (phone && !/^0[35789][0-9]{8}$/.test(phone.value)) {

            event.preventDefault();

            showMessage(
                "Phone number must contain 10 digits and start with 03, 05, 07, 08 or 09.",
                "Số điện thoại phải gồm 10 chữ số và bắt đầu bằng 03, 05, 07, 08 hoặc 09."
            );

            return;
        }

        // Email

        if (email && !email.validity.valid) {

            event.preventDefault();

            showMessage(
                "Please enter a valid email address.",
                "Vui lòng nhập địa chỉ email hợp lệ."
            );

            return;
        }

        // Passport Number - BR18

        if (passport &&
            !/^[A-Za-z0-9]{6,9}$/.test(passport.value)) {

            event.preventDefault();

            showMessage(
                "Passport must contain 6 to 9 letters or digits.",
                "Số hộ chiếu phải gồm 6 đến 9 chữ cái hoặc chữ số."
            );

            return;
        }

        // Nationality

        if (nationality &&
            nationality.value.trim().length === 0) {

            event.preventDefault();

            showMessage(
                "Please enter your nationality.",
                "Vui lòng nhập quốc tịch."
            );

            return;
        }

        // Password

        const passwordPattern =
            /^(?=.*[a-z])(?=.*[A-Z])(?=.*[0-9])(?=.*[^A-Za-z0-9\s]).{8,128}$/;

        if (!passwordPattern.test(password)) {

            event.preventDefault();

            showMessage(
                "Password must contain 8 to 128 characters, including uppercase, lowercase, number and special character.",
                "Mật khẩu phải có từ 8 đến 128 ký tự, gồm chữ hoa, chữ thường, chữ số và ký tự đặc biệt."
            );

            return;
        }

        // Confirm Password

        if (password !== confirmPassword) {

            event.preventDefault();

            showMessage(
                "Passwords do not match.",
                "Mật khẩu xác nhận không trùng khớp."
            );

            return;
        }

        // Terms and Conditions

        if (!document.getElementById("acceptTerms").checked) {

            event.preventDefault();

            showMessage(
                "Please accept the Terms and Conditions.",
                "Vui lòng đồng ý với các điều khoản."
            );

            return;
        }

        // All validations passed.
        // Allow the form to submit to AccountController.

        const submitButton =
            form.querySelector('button[type="submit"]');

        if (submitButton) {
            submitButton.disabled = true;
        }

        showMessage(
            "Creating your account...",
            "Đang tạo tài khoản..."
        );

        message.style.color = "#198754";
    });
});
