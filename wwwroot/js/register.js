document.addEventListener("DOMContentLoaded", function () {

    const continueButton =
        document.getElementById("continueRegister");

    if (!continueButton) return;

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

});


document.addEventListener("DOMContentLoaded", function () {

    // Show / Hide Password

    document.querySelectorAll("[data-toggle-password]")
        .forEach(function (button) {

            button.addEventListener("click", function () {

                const inputId =
                    button.dataset.togglePassword;

                const input =
                    document.getElementById(inputId);

                if (!input) return;

                const isHidden =
                    input.type === "password";

                input.type =
                    isHidden ? "text" : "password";

                button.setAttribute(
                    "aria-label",
                    isHidden ? "Hide password" : "Show password"
                );

            });

        });

    // Registration Validation

    const form =
        document.getElementById("registrationForm");

    if (!form) return;

    const message =
        document.getElementById("registerMessage");

    function showMessage(en, vi) {

        const isVietnamese =
            document.documentElement.lang
                .toLowerCase().startsWith("vi");

        message.textContent =
            isVietnamese ? vi : en;

    }

    form.addEventListener("submit", function (event) {

        event.preventDefault();

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

        // Full Name

        if (fullName.length === 0) {

            showMessage(
                "Please enter your full name.",
                "Vui lòng nhập họ và tên."
            );

            return;
        }

        // Phone Number

        if (phone && !/^0[0-9]{9}$/.test(phone.value.trim())) {

            showMessage(
                "Please enter a valid 10-digit phone number.",
                "Vui lòng nhập số điện thoại hợp lệ gồm 10 chữ số."
            );

            return;
        }

        // Email

        if (email && !email.validity.valid) {

            showMessage(
                "Please enter a valid email address.",
                "Vui lòng nhập địa chỉ email hợp lệ."
            );

            return;
        }

        // Password

        const passwordPattern =
            /^(?=.*[a-z])(?=.*[A-Z])(?=.*[0-9])(?=.*[^A-Za-z0-9\s]).{8,}$/;

        if (!passwordPattern.test(password)) {

            showMessage(
                "Password must contain at least 8 characters, including uppercase, lowercase, number and special character.",
                "Mật khẩu phải có ít nhất 8 ký tự, gồm chữ hoa, chữ thường, chữ số và ký tự đặc biệt."
            );

            return;
        }

        // Confirm Password

        if (password !== confirmPassword) {

            showMessage(
                "Passwords do not match.",
                "Mật khẩu xác nhận không trùng khớp."
            );

            return;
        }

        // Terms

        if (!document.getElementById("acceptTerms").checked) {

            showMessage(
                "Please accept the Terms and Conditions.",
                "Vui lòng đồng ý với các điều khoản."
            );

            return;
        }

        // Frontend Validation Completed

        showMessage(
            "Validation successful. Backend registration is not connected yet.",
            "Dữ liệu hợp lệ. Chức năng đăng ký chưa được kết nối Backend."
        );

    });

});
