
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
        loginDemo: "Form validated. Backend authentication is not connected yet.",
        adminLogin: "Admin Login",
        adminSubtitle: "Sign in to manage the SaigonBite system",
        username: "Username",
        usernamePlaceholder: "Enter your username",
        adminLoginButton: "Log In",
        backToPortal: "← Back to System Portal",
        createAccount: "Create Account",
        chooseAccountType: "Choose your account type",
        localCustomer: "Local Customer",
        localDescription: "Register with your phone number",
        foreignTourist: "Foreign Tourist",
        foreignDescription: "Register with your email address",
        continueButton: "Continue →",
        backToSignIn: "← Back to Sign In",
        fullName: "Full Name",
        fullNamePlaceholder: "Enter your full name",
        phoneNumber: "Phone Number",
        phonePlaceholder: "Enter your phone number",
        emailAddress: "Email Address",
        emailPlaceholder: "Enter your email address",
        confirmPassword: "Confirm Password",
        confirmPasswordPlaceholder: "Confirm your password",
        acceptTerms: "I agree to the Terms and Conditions",
        registerButton: "Register",
        backToAccountType: "← Back to Account Type",
        alreadyHaveAccount: "Already have an account?",
        signIn: "Sign In",
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
        loginDemo: "Biểu mẫu hợp lệ. Chưa kết nối hệ thống xác thực.",
        adminLogin: "Đăng nhập quản trị",
        adminSubtitle: "Đăng nhập để quản lý hệ thống SaigonBite",
        username: "Tên đăng nhập",
        usernamePlaceholder: "Nhập tên đăng nhập",
        adminLoginButton: "Đăng nhập",
        backToPortal: "← Quay lại cổng hệ thống",
        createAccount: "Tạo tài khoản",
        chooseAccountType: "Lựa chọn loại tài khoản",
        localCustomer: "Khách hàng nội địa",
        localDescription: "Đăng ký bằng số điện thoại",
        foreignTourist: "Khách du lịch nước ngoài",
        foreignDescription: "Đăng ký bằng địa chỉ email",
        continueButton: "Tiếp tục →",
        backToSignIn: "← Quay lại đăng nhập",
        fullName: "Họ và tên",
        fullNamePlaceholder: "Nhập họ và tên",
        phoneNumber: "Số điện thoại",
        phonePlaceholder: "Nhập số điện thoại",
        emailAddress: "Địa chỉ email",
        emailPlaceholder: "Nhập địa chỉ email",
        confirmPassword: "Xác nhận mật khẩu",
        confirmPasswordPlaceholder: "Nhập lại mật khẩu",
        acceptTerms: "Tôi đồng ý với các điều khoản và điều kiện",
        registerButton: "Đăng ký",
        backToAccountType: "← Quay lại chọn loại tài khoản",
        alreadyHaveAccount: "Bạn đã có tài khoản?",
        signIn: "Đăng nhập",
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
