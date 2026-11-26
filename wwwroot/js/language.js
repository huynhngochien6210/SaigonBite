
const translations = {

    en: {

        // LOGIN

        signIn: "Sign In",
        welcome: "Welcome back to SaigonBite!",
        identifier: "Email or Phone Number",
        identifierPlaceholder: "Enter email or phone number",
        password: "Password",
        passwordPlaceholder: "Enter your password",
        rememberMe: "Remember me",
        forgotPassword: "Forgot Password",
        or: "OR",
        noAccount: "Don't have an account?",
        createAccount: "Create Account",
        backPortal: "← Back to System Portal",
        showPassword: "Show password",
        hidePassword: "Hide password",
        requiredFields: "Please complete all required fields.",
        loginDemo: "Form validated. Backend authentication is not connected yet.",

        // ADMIN LOGIN

        adminLogin: "Admin Login",
        adminSubtitle: "Sign in to manage the SaigonBite system",
        username: "Username",
        usernamePlaceholder: "Enter your username",
        adminLoginButton: "Log In",
        backToPortal: "← Back to System Portal",

        // REGISTER

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
        passportNumber: "Passport Number",
        passportPlaceholder: "Enter your passport number",
        nationality: "Nationality",
        nationalityPlaceholder: "Enter your nationality",
        confirmPassword: "Confirm Password",
        confirmPasswordPlaceholder: "Confirm your password",
        acceptTerms: "I agree to the Terms and Conditions",
        registerButton: "Register",
        backToAccountType: "← Back to Account Type",
        alreadyHaveAccount: "Already have an account?",

        // REGISTRATION SUCCESS

        registrationSuccessful: "Registration Successful!",

        registrationSuccessDescription:
            "Your account has been created. Please verify your account to continue.",

        accountStatus: "Account Status:",
        unverified: "Unverified",
        verifyPhone: "Verify Phone",
        verifyEmail: "Verify Email",
        goHome: "Go Home",

        // OTP VERIFICATION

        verifyPhoneDescription:
            "Enter the 6-digit verification code sent to your phone.",

        verifyEmailDescription:
            "Enter the 6-digit verification code sent to your email.",

        verifyAccount: "Verify Account",
        didNotReceiveCode: "Didn't receive the code?",
        resendCodeIn: "Resend code in",
        seconds: "seconds",
        resendCode: "Resend Code",

        verificationDemoNote:
            "UI Preview - Backend is not connected.",

        otpSimulationNote:
            "Development mode - Check the server terminal for your verification code.",

        // FORGOT PASSWORD

        forgotPasswordDescription:
            "Choose your recovery method.",

        sendVerificationCode: "Send Verification Code",
        verifyResetCode: "Verify Reset Code",
        verifyCode: "Verify Code",
        resetPassword: "Reset Password",

        resetPasswordDescription:
            "Create a new password for your account.",

        newPassword: "New Password",
        newPasswordPlaceholder: "Enter your new password",
        passwordRequirements: "Password Requirements",

        ruleLength: "At least 8 characters",
        ruleUppercase: "At least one uppercase letter",
        ruleLowercase: "At least one lowercase letter",
        ruleNumber: "At least one number",
        ruleSpecial: "At least one special character",

        passwordResetSuccessful:
            "Password Reset Successful!",

        passwordResetSuccessDescription:
            "Your password has been reset successfully. You can now sign in with your new password.",

        // NAVIGATION

        home: "Home",
        menu: "Menu",
        orderStatus: "Order Status",

        // MY ACCOUNT

        myAccount: "My Account",
        editAccount: "Edit Account",
        personalInformation: "Personal Information",
        changePassword: "Change Password",
        loginHistory: "Login History",
        orderHistory: "Order History",
        dateOfBirth: "Date of Birth",
        verified: "✓ Verified",
        edit: "Edit",
        cancel: "Cancel",
        saveChanges: "Save Changes",
        signOut: "Sign Out",

        profileUpdated:
            "Profile updated successfully (Demo).",

        // CHANGE PASSWORD

        changePasswordSubtitle:
            "Update your password to keep your account secure.",

        currentPassword: "Current Password",
        confirmNewPassword: "Confirm New Password",

        passwordMinLength: "At least 8 characters",
        passwordUppercase: "One uppercase letter",
        passwordLowercase: "One lowercase letter",
        passwordNumber: "One number",
        passwordSpecial: "One special character",

        // ORDER HISTORY

        orderHistorySubtitle:
            "View your previous orders.",

        searchOrderId: "Search Order ID",
        orderId: "Order ID",
        orderDate: "Order Date",
        orderTotal: "Total",
        orderItem: "Item",
        allStatuses: "All Statuses",
        status: "Status",

        // ORDER LIFECYCLE - 8 STATUSES

        placed: "Placed",
        confirmed: "Confirmed",
        preparing: "Preparing",
        ready: "Ready",
        outfordelivery: "Out for Delivery",
        delivered: "Delivered",
        cancelled: "Cancelled",
        orderFailed: "Failed",

        // ORDER DETAILS

        action: "Action",
        viewDetails: "View Details",
        orderDetails: "Order Details",
        paymentMethod: "Payment Method",
        totalOrders: "Total orders:",
        noOrdersFound: "No orders found.",
        notAvailable: "Not available",
        noItems: "No items",
        close: "Close",

        orderHistoryDemoNote:
            "Demo order history. Backend is not connected.",

        // DEACTIVATE ACCOUNT

        deactivateAccount: "Deactivate Account",

        deactivateDescription:
            "Deactivating your account will prevent you from signing in. Your previous order records will be retained.",

        deactivateWarningTitle:
            "Account Deactivation Warning",

        deactivateWarning:
            "You will no longer be able to sign in or recover your password after deactivating your account.",

        deactivateKeepData:
            "Your previous order records will not be deleted.",

        deactivateConfirmation:
            "I understand and agree to deactivate my account.",

        confirmDeactivation:
            "Confirm Deactivation"

    },

    vi: {

        // LOGIN

        signIn: "Đăng nhập",
        welcome: "Chào mừng bạn trở lại SaigonBite!",
        identifier: "Email hoặc số điện thoại",
        identifierPlaceholder: "Nhập email hoặc số điện thoại",
        password: "Mật khẩu",
        passwordPlaceholder: "Nhập mật khẩu",
        rememberMe: "Ghi nhớ đăng nhập",
        forgotPassword: "Quên mật khẩu",
        or: "HOẶC",
        noAccount: "Bạn chưa có tài khoản?",
        createAccount: "Tạo tài khoản",
        backPortal: "← Quay lại cổng hệ thống",
        showPassword: "Hiện mật khẩu",
        hidePassword: "Ẩn mật khẩu",
        requiredFields: "Vui lòng điền đầy đủ thông tin.",
        loginDemo: "Biểu mẫu hợp lệ. Chưa kết nối hệ thống xác thực.",

        // ADMIN LOGIN

        adminLogin: "Đăng nhập quản trị",
        adminSubtitle: "Đăng nhập để quản lý hệ thống SaigonBite",
        username: "Tên đăng nhập",
        usernamePlaceholder: "Nhập tên đăng nhập",
        adminLoginButton: "Đăng nhập",
        backToPortal: "← Quay lại cổng hệ thống",

        // REGISTER

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
        passportNumber: "Số hộ chiếu",
        passportPlaceholder: "Nhập số hộ chiếu",
        nationality: "Quốc tịch",
        nationalityPlaceholder: "Nhập quốc tịch",
        confirmPassword: "Xác nhận mật khẩu",
        confirmPasswordPlaceholder: "Nhập lại mật khẩu",
        acceptTerms: "Tôi đồng ý với các điều khoản và điều kiện",
        registerButton: "Đăng ký",
        backToAccountType: "← Quay lại chọn loại tài khoản",
        alreadyHaveAccount: "Bạn đã có tài khoản?",

        // REGISTRATION SUCCESS

        registrationSuccessful: "Đăng ký thành công!",

        registrationSuccessDescription:
            "Tài khoản của bạn đã được tạo. Vui lòng xác thực tài khoản để tiếp tục.",

        accountStatus: "Trạng thái tài khoản:",
        unverified: "Chưa xác thực",
        verifyPhone: "Xác thực số điện thoại",
        verifyEmail: "Xác thực email",
        goHome: "Về trang chủ",

        // OTP VERIFICATION

        verifyPhoneDescription:
            "Nhập mã xác thực gồm 6 chữ số được gửi đến số điện thoại của bạn.",

        verifyEmailDescription:
            "Nhập mã xác thực gồm 6 chữ số được gửi đến email của bạn.",

        verifyAccount: "Xác thực tài khoản",
        didNotReceiveCode: "Bạn chưa nhận được mã?",
        resendCodeIn: "Gửi lại mã sau",
        seconds: "giây",
        resendCode: "Gửi lại mã",

        verificationDemoNote:
            "Giao diện thử nghiệm - Chưa kết nối Backend.",

        otpSimulationNote:
            "Chế độ phát triển - Kiểm tra Terminal để lấy mã xác thực.",

        // FORGOT PASSWORD

        forgotPasswordDescription:
            "Lựa chọn phương thức khôi phục mật khẩu.",

        sendVerificationCode: "Gửi mã xác thực",
        verifyResetCode: "Xác thực mã khôi phục",
        verifyCode: "Xác thực mã",
        resetPassword: "Đặt lại mật khẩu",

        resetPasswordDescription:
            "Tạo mật khẩu mới cho tài khoản của bạn.",

        newPassword: "Mật khẩu mới",
        newPasswordPlaceholder: "Nhập mật khẩu mới",
        passwordRequirements: "Yêu cầu mật khẩu",

        ruleLength: "Ít nhất 8 ký tự",
        ruleUppercase: "Ít nhất một chữ hoa",
        ruleLowercase: "Ít nhất một chữ thường",
        ruleNumber: "Ít nhất một chữ số",
        ruleSpecial: "Ít nhất một ký tự đặc biệt",

        passwordResetSuccessful:
            "Đặt lại mật khẩu thành công!",

        passwordResetSuccessDescription:
            "Mật khẩu của bạn đã được đặt lại thành công. Bạn có thể đăng nhập bằng mật khẩu mới.",

        // NAVIGATION

        home: "Trang chủ",
        menu: "Thực đơn",
        orderStatus: "Trạng thái đơn hàng",

        // MY ACCOUNT

        myAccount: "Tài khoản của tôi",
        editAccount: "Chỉnh sửa tài khoản",
        personalInformation: "Thông tin cá nhân",
        changePassword: "Đổi mật khẩu",
        loginHistory: "Lịch sử đăng nhập",
        orderHistory: "Lịch sử đơn hàng",
        dateOfBirth: "Ngày sinh",
        verified: "✓ Đã xác thực",
        edit: "Chỉnh sửa",
        cancel: "Hủy",
        saveChanges: "Lưu thay đổi",
        signOut: "Đăng xuất",

        profileUpdated:
            "Cập nhật thông tin thành công (Mô phỏng).",

        // CHANGE PASSWORD

        changePasswordSubtitle:
            "Cập nhật mật khẩu để bảo vệ tài khoản của bạn.",

        currentPassword: "Mật khẩu hiện tại",
        confirmNewPassword: "Xác nhận mật khẩu mới",

        passwordMinLength: "Ít nhất 8 ký tự",
        passwordUppercase: "Một chữ cái viết hoa",
        passwordLowercase: "Một chữ cái viết thường",
        passwordNumber: "Một chữ số",
        passwordSpecial: "Một ký tự đặc biệt",

        // ORDER HISTORY

        orderHistorySubtitle:
            "Xem các đơn hàng trước đây.",

        searchOrderId: "Tìm mã đơn hàng",
        orderId: "Mã đơn hàng",
        orderDate: "Ngày đặt hàng",
        orderTotal: "Tổng tiền",
        orderItem: "Món ăn",
        allStatuses: "Tất cả trạng thái",
        status: "Trạng thái",

        // ORDER LIFECYCLE - 8 STATUSES

        placed: "Đã đặt hàng",
        confirmed: "Đã xác nhận",
        preparing: "Đang chuẩn bị",
        ready: "Đã chuẩn bị xong",
        outfordelivery: "Đang giao hàng",
        delivered: "Đã giao hàng",
        cancelled: "Đã hủy",
        orderFailed: "Giao hàng thất bại",

        // ORDER DETAILS

        action: "Thao tác",
        viewDetails: "Xem chi tiết",
        orderDetails: "Chi tiết đơn hàng",
        paymentMethod: "Phương thức thanh toán",
        totalOrders: "Tổng số đơn hàng:",
        noOrdersFound: "Không tìm thấy đơn hàng.",
        notAvailable: "Chưa có dữ liệu",
        noItems: "Không có món ăn",
        close: "Đóng",

        orderHistoryDemoNote:
            "Lịch sử đơn hàng mô phỏng. Backend chưa được kết nối.",

        // DEACTIVATE ACCOUNT

        deactivateAccount: "Vô hiệu hóa tài khoản",

        deactivateDescription:
            "Sau khi vô hiệu hóa tài khoản, bạn sẽ không thể đăng nhập. Lịch sử đơn hàng trước đây vẫn được lưu giữ.",

        deactivateWarningTitle:
            "Cảnh báo vô hiệu hóa tài khoản",

        deactivateWarning:
            "Bạn sẽ không thể đăng nhập hoặc khôi phục mật khẩu sau khi vô hiệu hóa tài khoản.",

        deactivateKeepData:
            "Lịch sử đơn hàng trước đây sẽ không bị xóa.",

        deactivateConfirmation:
            "Tôi hiểu và đồng ý vô hiệu hóa tài khoản của mình.",

        confirmDeactivation:
            "Xác nhận vô hiệu hóa"

    }

};

// ==========================================
// TRANSLATE TEXT
// ==========================================

function translate(key) {

    const language =
        localStorage.getItem("saigonbite-language") || "en";

    return translations[language]?.[key]
        || translations.en[key]
        || key;

}

// ==========================================
// SET LANGUAGE
// ==========================================

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

// ==========================================
// INITIALIZE LANGUAGE
// ==========================================

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
