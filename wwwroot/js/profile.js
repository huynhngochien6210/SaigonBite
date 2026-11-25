document.addEventListener("DOMContentLoaded", function () {

    const storageKey = "saigonbite_demo_profile";

    const defaultProfile = {
        fullName: "Nguyen Van An",
        email: "customer@example.com",
        phone: "0912345678",
        dateOfBirth: "2004-05-15"
    };

    // Read profile

    function getProfile() {

        try {

            const stored = sessionStorage.getItem(storageKey);

            return stored
                ? { ...defaultProfile, ...JSON.parse(stored) }
                : { ...defaultProfile };

        } catch {

            return { ...defaultProfile };

        }

    }

    const profile = getProfile();

    // Display information

    function setText(id, value) {

        const element = document.getElementById(id);

        if (element) {
            element.textContent = value || "—";
        }

    }

    setText("sidebarFullName", profile.fullName);
    setText("displayFullName", profile.fullName);
    setText("displayEmail", profile.email);
    setText("displayPhone", profile.phone);
    setText("displayDateOfBirth", profile.dateOfBirth);

    // Show success message after saving

    const successMessage =
        document.getElementById("profileSuccessMessage");

    if (successMessage) {

        const updated = sessionStorage.getItem(
            "saigonbite_profile_updated"
        );

        if (updated === "true") {

            successMessage.hidden = false;

            sessionStorage.removeItem(
                "saigonbite_profile_updated"
            );

        }

    }

    // Demo sign out

    const signOut = document.getElementById("demoSignOut");

    if (signOut) {

        signOut.addEventListener("click", function () {

            sessionStorage.removeItem(storageKey);

            sessionStorage.removeItem(
                "saigonbite_profile_updated"
            );

        });

    }

    // Edit Account

    const form = document.getElementById("editProfileForm");

    if (!form) return;

    const fullName = document.getElementById("editFullName");
    const email = document.getElementById("editEmail");
    const phone = document.getElementById("editPhone");
    const dateOfBirth = document.getElementById("editDateOfBirth");
    const message = document.getElementById("profileMessage");

    // Fill existing profile

    fullName.value = profile.fullName;
    email.value = profile.email;
    phone.value = profile.phone;
    dateOfBirth.value = profile.dateOfBirth;

    // Prevent future date of birth

    const today = new Date();

    const year = today.getFullYear();

    const month = String(
        today.getMonth() + 1
    ).padStart(2, "0");

    const day = String(
        today.getDate()
    ).padStart(2, "0");

    dateOfBirth.max = `${year}-${month}-${day}`;

    // Bilingual validation message

    function showMessage(en, vi) {

        const isVietnamese =
            document.documentElement.lang
                .toLowerCase().startsWith("vi");

        message.textContent = isVietnamese ? vi : en;

    }

    // Save changes

    form.addEventListener("submit", function (event) {

        event.preventDefault();

        message.textContent = "";

        // Full name validation

        if (!fullName.value.trim()) {

            showMessage(
                "Please enter your full name.",
                "Vui lòng nhập họ và tên."
            );

            return;

        }

        // Email validation

        if (!email.validity.valid) {

            showMessage(
                "Invalid email address.",
                "Địa chỉ email không hợp lệ."
            );

            return;

        }

        // Phone validation - BR18

        if (!/^0[35789][0-9]{8}$/.test(phone.value.trim())) {

            showMessage(
                "Invalid Vietnamese mobile number.",
                "Số điện thoại Việt Nam không hợp lệ."
            );

            return;

        }

        // Date of birth validation

        if (
            dateOfBirth.value &&
            dateOfBirth.value > dateOfBirth.max
        ) {

            showMessage(
                "Date of birth cannot be in the future.",
                "Ngày sinh không được nằm trong tương lai."
            );

            return;

        }

        // Updated profile

        const updatedProfile = {

            fullName: fullName.value.trim(),

            email: email.value.trim(),

            phone: phone.value.trim(),

            dateOfBirth: dateOfBirth.value

        };

        // Temporary frontend storage

        try {

            sessionStorage.setItem(
                storageKey,
                JSON.stringify(updatedProfile)
            );

            sessionStorage.setItem(
                "saigonbite_profile_updated",
                "true"
            );

        } catch {

            showMessage(
                "Unable to save demo profile.",
                "Không thể lưu thông tin thử nghiệm."
            );

            return;

        }

        window.location.href = "/Account/MyAccount";

    });

});