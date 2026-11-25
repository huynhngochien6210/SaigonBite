
document.addEventListener("DOMContentLoaded", function () {

    const inputs = Array.from(
        document.querySelectorAll(".otp-input")
    );

    const form =
        document.getElementById("verificationForm");

    const countdown =
        document.getElementById("countdown");

    const countdownContainer =
        document.getElementById("resendCountdown");

    const resendButton =
        document.getElementById("resendButton");

    const message =
        document.getElementById("otpMessage");

    if (!form || inputs.length === 0) return;

    let timer;

    // Bilingual message

    function showMessage(en, vi) {

        const isVietnamese =
            document.documentElement.lang
                .toLowerCase().startsWith("vi");

        message.textContent =
            isVietnamese ? vi : en;
    }

    // OTP input handling

    inputs.forEach(function (input, index) {

        input.addEventListener("input", function () {

            input.value =
                input.value.replace(/\D/g, "").slice(0, 1);

            message.textContent = "";

            if (input.value && index < inputs.length - 1) {

                inputs[index + 1].focus();

            }

        });

        // Backspace

        input.addEventListener("keydown", function (event) {

            if (
                event.key === "Backspace" &&
                input.value === "" &&
                index > 0
            ) {

                event.preventDefault();

                inputs[index - 1].value = "";
                inputs[index - 1].focus();

            }

        });

        // Select existing digit

        input.addEventListener("focus", function () {

            input.select();

        });

        // Paste OTP

        input.addEventListener("paste", function (event) {

            event.preventDefault();

            const pastedCode = event.clipboardData
                .getData("text")
                .replace(/\D/g, "")
                .slice(0, 6);

            if (!pastedCode) return;

            inputs.forEach(function (item, i) {

                item.value = pastedCode[i] || "";

            });

            const nextIndex = Math.min(
                pastedCode.length,
                inputs.length - 1
            );

            inputs[nextIndex].focus();

        });

    });

    // Countdown

    function startCountdown() {

        clearInterval(timer);

        const endTime = Date.now() + 60000;

        countdown.textContent = "60";

        countdownContainer.hidden = false;

        resendButton.disabled = true;

        timer = setInterval(function () {

            const remaining = Math.max(
                0,
                Math.ceil((endTime - Date.now()) / 1000)
            );

            countdown.textContent = remaining;

            if (remaining === 0) {

                clearInterval(timer);

                resendButton.disabled = false;

                countdownContainer.hidden = true;

            }

        }, 1000);

    }

    // Resend OTP - UI simulation

    resendButton.addEventListener("click", function () {

        inputs.forEach(function (input) {

            input.value = "";

        });

        showMessage(
            "Resend simulation. No real OTP has been sent.",
            "Mô phỏng gửi lại. Chưa có mã OTP thật được gửi."
        );

        inputs[0].focus();

        startCountdown();

    });

    // Verify OTP

    form.addEventListener("submit", function (event) {

        event.preventDefault();

        const otp = inputs
            .map(input => input.value)
            .join("");

        if (!/^[0-9]{6}$/.test(otp)) {

            showMessage(
                "Please enter a valid 6-digit code.",
                "Vui lòng nhập mã xác thực gồm 6 chữ số."
            );

            return;
        }

        showMessage(
            "Valid OTP format. Backend verification is not connected.",
            "Định dạng OTP hợp lệ. Chưa kết nối xác thực Backend."
        );

    });

    startCountdown();

});
