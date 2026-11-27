
document.addEventListener("DOMContentLoaded", function () {

    "use strict";

    const form =
        document.getElementById("verificationForm");

    if (!form) return;

    const inputs = Array.from(
        form.querySelectorAll(".otp-input")
    );

    const message =
        document.getElementById("otpMessage");

    const resendButton =
        document.getElementById("resendButton");

    const countdown =
        document.getElementById("countdown");

    const countdownWrapper =
        document.getElementById("resendCountdown");

    const submitButton =
        form.querySelector('button[type="submit"]');

    const antiForgeryToken = form.querySelector(
        'input[name="__RequestVerificationToken"]'
    )?.value;

    const isReset =
        form.dataset.purpose === "reset";


    // ==========================================
    // 1. SELECT API ENDPOINT
    // ==========================================

    const sendEndpoint = isReset
        ? "/Account/SendResetCode"
        : "/Account/SendVerificationCode";

    const verifyEndpoint = isReset
        ? "/Account/VerifyResetCode"
        : "/Account/VerifyRegistrationCode";

    const expiredRedirect = isReset
        ? "/Account/ForgotPassword"
        : "/Account/Login";

    let timer = null;

    let busy = false;


    // ==========================================
    // 2. DISPLAY MESSAGE
    // ==========================================

    function showMessage(text, success = false) {

        if (!message) return;

        message.textContent = text;

        message.classList.toggle(
            "text-success",
            success
        );

        message.classList.toggle(
            "text-danger",
            !success
        );
    }


    // ==========================================
    // 3. OTP INPUT
    // ==========================================

    inputs.forEach((input, index) => {

        input.addEventListener("input", function () {

            input.value = input.value
                .replace(/[^0-9]/g, "")
                .slice(0, 1);

            if (input.value && index < inputs.length - 1) {

                inputs[index + 1].focus();
            }
        });

        input.addEventListener("keydown", function (event) {

            if (
                event.key === "Backspace" &&
                !input.value &&
                index > 0
            ) {

                inputs[index - 1].focus();
            }

            if (
                event.key === "ArrowLeft" &&
                index > 0
            ) {

                inputs[index - 1].focus();
            }

            if (
                event.key === "ArrowRight" &&
                index < inputs.length - 1
            ) {

                inputs[index + 1].focus();
            }
        });

        input.addEventListener("paste", function (event) {

            event.preventDefault();

            const pasted = event.clipboardData
                .getData("text")
                .replace(/[^0-9]/g, "")
                .slice(0, 6);

            if (!pasted) return;

            inputs.forEach((field, i) => {

                field.value = pasted[i] || "";
            });

            inputs[
                Math.min(pasted.length, inputs.length - 1)
            ].focus();
        });
    });


    // ==========================================
    // 4. COUNTDOWN
    // ==========================================

    function startCountdown(seconds = 60) {

        if (timer) {
            clearInterval(timer);
        }

        let remaining = seconds;

        resendButton.disabled = true;

        countdownWrapper.hidden = false;

        countdown.textContent = remaining;

        timer = setInterval(function () {

            remaining--;

            countdown.textContent =
                Math.max(remaining, 0);

            if (remaining <= 0) {

                clearInterval(timer);

                timer = null;

                resendButton.disabled = false;

                countdownWrapper.hidden = true;
            }

        }, 1000);
    }


    // ==========================================
    // 5. HTTP POST REQUEST
    // ==========================================

    async function postRequest(url, data = {}) {

        if (!antiForgeryToken) {

            throw new Error(
                "Security token is missing. Refresh the page."
            );
        }

        const body = new URLSearchParams();

        body.append(
            "__RequestVerificationToken",
            antiForgeryToken
        );

        Object.entries(data).forEach(([key, value]) => {

            body.append(key, value);
        });

        const response = await fetch(url, {

            method: "POST",

            headers: {
                "Content-Type":
                    "application/x-www-form-urlencoded"
            },

            credentials: "same-origin",

            body: body.toString()
        });

        if (response.status === 401) {

            window.location.href = expiredRedirect;

            return null;
        }

        if (!response.ok) {

            throw new Error(
                "Request failed. Please try again."
            );
        }

        return await response.json();
    }


    // ==========================================
    // 6. SEND OTP
    // ==========================================

    async function sendCode() {

        if (busy) return;

        busy = true;

        resendButton.disabled = true;

        submitButton.disabled = true;

        try {

            const result = await postRequest(sendEndpoint);

            if (!result) return;

            showMessage(
                result.message,
                result.success
            );

            if (result.success) {

                inputs.forEach(input => {

                    input.value = "";
                });

                inputs[0].focus();
            }

            startCountdown(60);
        }
        catch (error) {

            showMessage(error.message);

            startCountdown(60);
        }
        finally {

            busy = false;

            submitButton.disabled = false;
        }
    }


    // ==========================================
    // 7. VERIFY OTP
    // ==========================================

    form.addEventListener("submit", async function (event) {

        event.preventDefault();

        if (busy) return;

        const code = inputs
            .map(input => input.value)
            .join("");

        if (!/^[0-9]{6}$/.test(code)) {

            showMessage(
                "Please enter a valid 6-digit code."
            );

            return;
        }

        busy = true;

        submitButton.disabled = true;

        let redirecting = false;

        try {

            const result = await postRequest(
                verifyEndpoint,
                { code: code }
            );

            if (!result) return;

            showMessage(
                result.message,
                result.success
            );

            if (result.success) {

                if (timer) {
                    clearInterval(timer);
                }

                redirecting = true;

                resendButton.disabled = true;

                window.location.href =
                    result.redirectUrl ||
                    (isReset
                        ? "/Account/ResetPassword"
                        : "/Account/Login");

                return;
            }

            inputs.forEach(input => {

                input.value = "";
            });

            inputs[0].focus();
        }
        catch (error) {

            showMessage(error.message);
        }
        finally {

            if (!redirecting) {

                busy = false;

                submitButton.disabled = false;
            }
        }
    });


    // ==========================================
    // 8. RESEND OTP
    // ==========================================

    resendButton.addEventListener("click", sendCode);


    // ==========================================
    // 9. INITIAL OTP REQUEST
    // ==========================================

    sendCode();

});
