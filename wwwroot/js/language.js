
const translations = {

    en: {
        signIn: "Sign In",
        welcome: "Welcome back to SaigonBite!",
        identifier: "Email or Phone Number",
        identifierPlaceholder: "Enter email or phone number",
        password: "Password",
        passwordPlaceholder: "Enter your password",
        rememberMe: "Remember me",
        forgotPassword: "Forgot password?",
        or: "OR",
        noAccount: "Don't have an account?",
        createAccount: "Create New Account",
        backPortal: "← Back to System Portal",
        showPassword: "Show password",
        hidePassword: "Hide password",
        requiredFields: "Please complete all required fields.",
        loginDemo: "Form validated. Backend authentication is not connected yet."
    },

    vi: {
        signIn: "Đăng nhập",
        welcome: "Chào mừng bạn trở lại SaigonBite!",
        identifier: "Email hoặc số điện thoại",
        identifierPlaceholder: "Nhập email hoặc số điện thoại",
        password: "Mật khẩu",
        passwordPlaceholder: "Nhập mật khẩu",
        rememberMe: "Ghi nhớ đăng nhập",
        forgotPassword: "Quên mật khẩu?",
        or: "HOẶC",
        noAccount: "Bạn chưa có tài khoản?",
        createAccount: "Tạo tài khoản mới",
        backPortal: "← Quay lại cổng hệ thống",
        showPassword: "Hiện mật khẩu",
        hidePassword: "Ẩn mật khẩu",
        requiredFields: "Vui lòng điền đầy đủ thông tin.",
        loginDemo: "Biểu mẫu hợp lệ. Chưa kết nối hệ thống xác thực."
    }
};

function translate(key) {
    const language =
        localStorage.getItem("saigonbite-language") || "en";

    return translations[language]?.[key]
        || translations.en[key]
        || key;
}

function setLanguage(language) {

    if (!translations[language]) {
        language = "en";
    }

    localStorage.setItem(
        "saigonbite-language",
        language
    );

    document.documentElement.lang = language;

    // Translate text
    document.querySelectorAll("[data-i18n]")
        .forEach(element => {

            const key = element.dataset.i18n;

            element.textContent = translate(key);
        });

    // Translate placeholders
    document.querySelectorAll("[data-i18n-placeholder]")
        .forEach(element => {

            const key = element.dataset.i18nPlaceholder;

            element.placeholder = translate(key);
        });

    // Translate accessibility labels
    document.querySelectorAll("[data-i18n-aria]")
        .forEach(element => {

            element.setAttribute(
                "aria-label",
                translate(element.dataset.i18nAria)
            );
        });

    // Update active language button
    document.querySelectorAll("[data-language]")
        .forEach(button => {

            const active =
                button.dataset.language === language;

            button.classList.toggle("active", active);

            button.setAttribute("aria-pressed", active);
        });

    // Translate page title
    const titleKey = document.body.dataset.titleKey;

    if (titleKey) {
        document.title =
            translate(titleKey) + " - SaigonBite";
    }
}

document.addEventListener("DOMContentLoaded", () => {

    const savedLanguage =
        localStorage.getItem("saigonbite-language") || "en";

    setLanguage(savedLanguage);

    document.querySelectorAll("[data-language]")
        .forEach(button => {

            button.addEventListener("click", () => {

                setLanguage(button.dataset.language);
            });
        });
});
