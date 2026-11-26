
document.addEventListener("DOMContentLoaded", function () {

    // Remove obsolete demo data
    sessionStorage.removeItem("saigonbite_demo_profile");

    sessionStorage.removeItem("saigonbite_profile_updated");

    const form = document.getElementById("editProfileForm");

    if (!form) return;

    const fullName =
        document.getElementById("editFullName");

    const email =
        document.getElementById("editEmail");

    const phone =
        document.getElementById("editPhone");

    const dateOfBirth =
        document.getElementById("editDateOfBirth");

    const nationality =
        document.getElementById("editNationality");

    const message =
        document.getElementById("profileMessage");

    const submitButton =
        form.querySelector('button[type="submit"]');


    // ==========================================
    // 1. DATE OF BIRTH LIMIT
    // ==========================================

    const today = new Date();

    const year = today.getFullYear();

    const month = String(
        today.getMonth() + 1
    ).padStart(2, "0");

    const day = String(
        today.getDate()
    ).padStart(2, "0");

    if (dateOfBirth) {

        dateOfBirth.max = `${year}-${month}-${day}`;

    }


    // ==========================================
    // 2. BILINGUAL MESSAGE
    // ==========================================

    function showMessage(en, vi, isError = true) {

        if (!message) return;

        const isVietnamese =
            document.documentElement.lang
                .toLowerCase()
                .startsWith("vi");

        message.textContent =
            isVietnamese ? vi : en;

        message.classList.toggle(
            "text-danger",
            isError
        );

        message.classList.toggle(
            "text-success",
            !isError
        );
    }


    // ==========================================
    // 3. VALIDATE AND SUBMIT FORM
    // ==========================================

    form.addEventListener("submit", function (event) {

        if (message) {

            message.textContent = "";

        }

        // Full Name
        if (!fullName.value.trim()) {

            event.preventDefault();

            showMessage(
                "Please enter your full name.",
                "Vui lòng nhập họ và tên."
            );

            return;
        }

        // Editable Email
        if (
            email &&
            !email.readOnly &&
            !email.validity.valid
        ) {

            event.preventDefault();

            showMessage(
                "Invalid email address.",
                "Địa chỉ email không hợp lệ."
            );

            return;
        }

        // Foreign Tourist Phone Number
        if (
            phone &&
            !phone.readOnly &&
            phone.value.trim() &&
            !/^\+?[0-9]{7,15}$/.test(phone.value.trim())
        ) {

            event.preventDefault();

            showMessage(
                "Invalid phone number.",
                "Số điện thoại không hợp lệ."
            );

            return;
        }

        // Date of Birth
        if (
            dateOfBirth &&
            dateOfBirth.value &&
            dateOfBirth.value > dateOfBirth.max
        ) {

            event.preventDefault();

            showMessage(
                "Date of birth cannot be in the future.",
                "Ngày sinh không được nằm trong tương lai."
            );

            return;
        }

        // Nationality
        if (
            nationality &&
            !nationality.value.trim()
        ) {

            event.preventDefault();

            showMessage(
                "Please enter your nationality.",
                "Vui lòng nhập quốc tịch."
            );

            return;
        }

        // Normalize editable input
        fullName.value = fullName.value.trim();

        if (email && !email.readOnly) {

            email.value = email.value.trim();

        }

        if (phone && !phone.readOnly) {

            phone.value = phone.value.trim();

        }

        if (nationality) {

            nationality.value =
                nationality.value.trim();

        }

        // Disable duplicate submission
        if (submitButton) {

            submitButton.disabled = true;

        }

        showMessage(
            "Saving changes...",
            "Đang lưu thay đổi...",
            false
        );

        // Allow normal POST to AccountController
        // Do not call event.preventDefault() here

    });

});
