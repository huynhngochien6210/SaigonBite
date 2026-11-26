
document.addEventListener("DOMContentLoaded", function () {

    const inputs = Array.from(
        document.querySelectorAll(".otp-input")
    );

    const form =
        document.getElementById("verificationForm");

    if (!form || inputs.length === 0) return;

    const countdown =
        document.getElementById("countdown");

    const countdownContainer =
        document.getElementById("resendCountdown");

    const resendButton =
        document.getElementById("resendButton");

    const message =
        document.getElementById("otpMessage");

    const submitButton =
        form.querySelector('button[type="submit"]');

    const isPasswordReset =
        form.dataset.purpose === "reset";

    const token = form.querySelector(
        'input[name="__RequestVerificationToken"]'
    ).value;

    let timer;
    let busy = false;

    // 1. Bilingual message

    function showMessage(en, vi, success = false) {

        const isVietnamese =
            document.documentElement.lang
                .toLowerCase().startsWith("vi");

        message.textContent =
            isVietnamese ? vi : en;

        message.style.color =
            success ? "#198754" : "#b90000";
    }

    // 2. OTP input handling

    inputs.forEach(function (input, index) {

        input.addEventListener("input", function () {

            input.value =
                input.value.replace(/\D/g, "").slice(0, 1);

            message.textContent = "";

            if (input.value && index < inputs.length - 1) {
                inputs[index + 1].focus();
            }
        });

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

        input.addEventListener("focus", function () {
            input.select();
        });

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

    // 3. Countdown

    function startCountdown(seconds = 60) {

        clearInterval(timer);

        const endTime =
            Date.now() + seconds * 1000;

        countdown.textContent = seconds;

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

    // 4. HTTP POST helper

    async function postRequest(url, data = {}) {

        const body = new URLSearchParams();

        body.append(
            "__RequestVerificationToken",
            token
        );

        Object.entries(data).forEach(([key, value]) => {
            body.append(key, value);
        });

        const response = await fetch(url, {

            method: "POST",

            credentials: "same-origin",

            headers: {
                "Content-Type":
                    "application/x-www-form-urlencoded"
            },

            body: body.toString()
        });

        if (response.status === 401) {

            window.location.href = "/Account/Login";

            throw new Error(
                "Verification session has expired."
            );
        }

        if (!response.ok) {

            throw new Error(
                "Server request failed."
            );
        }

        return await response.json();
    }

    // 5. Send OTP

    async function sendCode() {

        if (busy) return;

        busy = true;

        resendButton.disabled = true;
        submitButton.disabled = true;

        try {

            const result = await postRequest(
                "/Account/SendVerificationCode"
            );

            if (result.success) {

                inputs.forEach(input => {
                    input.value = "";
                });

                showMessage(
                    "Verification code generated. Check the server terminal.",
                    "Mã xác thực đã được tạo. Kiểm tra Terminal.",
                    true
                );

                inputs[0].focus();

                startCountdown();

            } else {

                showMessage(
                    result.message,
                    result.message
                );

                startCountdown();
            }

        } catch (error) {

            showMessage(
                "Unable to request verification code.",
                "Không thể yêu cầu mã xác thực."
            );

            resendButton.disabled = false;

        } finally {

            busy = false;

            submitButton.disabled = false;
        }
    }

    // 6. Resend OTP

    resendButton.addEventListener("click", function () {

        if (isPasswordReset) {

            inputs.forEach(input => {
                input.value = "";
            });

            showMessage(
                "Resend simulation.",
                "Mô phỏng gửi lại mã."
            );

            inputs[0].focus();

            startCountdown();

            return;
        }

        sendCode();
    });

    // 7. Verify OTP

    form.addEventListener("submit", async function (event) {

        event.preventDefault();

        if (busy) return;

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

        // Keep password recovery UI simulation

        if (isPasswordReset) {

            window.location.href =
                "/Account/ResetPassword";

            return;
        }

        busy = true;
        submitButton.disabled = true;

        try {

            const result = await postRequest(
                "/Account/VerifyRegistrationCode",
                { code: otp }
            );

            if (result.success) {

                clearInterval(timer);

                resendButton.disabled = true;

                showMessage(
                    "Account verified successfully.",
                    "Xác thực tài khoản thành công.",
                    true
                );

                window.location.href =
                    result.redirectUrl || "/Account/Login";

            } else {

                showMessage(
                    result.message,
                    result.message
                );

                inputs.forEach(input => {
                    input.value = "";
                });

                inputs[0].focus();
            }

        } catch (error) {

            showMessage(
                "Unable to verify the code.",
                "Không thể xác thực mã OTP."
            );

        } finally {

            busy = false;
            submitButton.disabled = false;
        }
    });

    // 8. Initialize verification page

    if (isPasswordReset) {

        startCountdown();

    } else {

        sendCode();
    }
});
