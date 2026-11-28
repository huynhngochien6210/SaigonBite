
// =====================================================
// SAIGONBITE - BRAND MANAGEMENT JAVASCRIPT
// Dùng chung cho Brands.cshtml và CreateBrand.cshtml
// =====================================================

let editingRow = null;
let selectedDeleteRow = null;

let selectedHubs = ["H1", "H3"];
let temporaryHubs = [...selectedHubs];


// =====================================================
// 1. COMMON FUNCTIONS
// =====================================================

function getElement(id) {
    return document.getElementById(id);
}

function escapeHtml(value) {
    return String(value).replace(/[&<>"']/g, function (char) {
        const entities = {
            "&": "&amp;",
            "<": "&lt;",
            ">": "&gt;",
            '"': "&quot;",
            "'": "&#039;"
        };

        return entities[char];
    });
}


// =====================================================
// 2. BRAND SEARCH AND FILTER
// =====================================================

const brandSearch = getElement("brandSearch");
const statusFilter = getElement("statusFilter");
const hubFilter = getElement("hubFilter");

function filterBrands() {
    const table = getElement("brandTable");

    if (!table) return;

    const keyword = brandSearch
        ? brandSearch.value.toLowerCase().trim()
        : "";

    const selectedStatus = statusFilter
        ? statusFilter.value
        : "all";

    const selectedHub = hubFilter
        ? hubFilter.value
        : "all";

    const rows = table.querySelectorAll("tbody tr");

    let visibleCount = 0;

    rows.forEach(row => {
        const rowText = row.innerText.toLowerCase();
        const rowStatus = row.dataset.status || "active";

        const rowHubs = (row.dataset.hubs || "")
            .split(",")
            .map(hub => hub.trim())
            .filter(Boolean);

        const matchesKeyword = rowText.includes(keyword);

        const matchesStatus =
            selectedStatus === "all" ||
            rowStatus === selectedStatus;

        const matchesHub =
            selectedHub === "all" ||
            rowHubs.includes(selectedHub);

        const isVisible =
            matchesKeyword &&
            matchesStatus &&
            matchesHub;

        row.style.display = isVisible ? "" : "none";

        if (isVisible) {
            visibleCount++;
        }
    });

    const emptyMessage = getElement("emptyBrandMessage");

    if (emptyMessage) {
        emptyMessage.style.display =
            visibleCount === 0 ? "block" : "none";
    }
}

if (brandSearch) {
    brandSearch.addEventListener("input", filterBrands);
}

if (statusFilter) {
    statusFilter.addEventListener("change", filterBrands);
}

if (hubFilter) {
    hubFilter.addEventListener("change", filterBrands);
}


// =====================================================
// 3. ADD BRAND MODAL
// =====================================================

function openBrandModal() {
    window.location.href = "/Admin/CreateBrand";
}

function closeBrandModal() {
    const modal = getElement("brandModal");

    if (modal) {
        modal.classList.remove("show");
    }

    editingRow = null;
}


// =====================================================
// 4. UPDATE BRAND STATUS
// =====================================================

function updateStatus(row, status) {
    if (!row) return;

    const statusElement = row.querySelector(".brand-status");

    if (!statusElement) return;

    row.dataset.status = status;

    if (status === "active") {
        statusElement.textContent = "Đang hoạt động";
        statusElement.className = "brand-status status-active";
    } else {
        statusElement.textContent = "Tạm dừng";
        statusElement.className = "brand-status status-paused";
    }
}


// =====================================================
// 5. ADD / UPDATE BRAND
// =====================================================

const brandForm = getElement("brandForm");

if (brandForm) {
    brandForm.addEventListener("submit", function (event) {
        event.preventDefault();

        const name = getElement("brandNameInput").value.trim();

        const description =
            getElement("brandDescriptionInput").value.trim();

        const hubInput = getElement("brandHubInput");
        const hub = hubInput ? hubInput.value : "0";

        const status = getElement("brandStatusInput").value;

        if (!name) {
            alert("Vui lòng nhập tên thương hiệu.");
            return;
        }

        // UPDATE BRAND
        if (editingRow) {
            editingRow.querySelector(".brand-name-cell").textContent = name;

            editingRow.cells[2].textContent = description;

            const hubNumber = editingRow.querySelector(".hub-number");

            if (hubNumber) {
                hubNumber.textContent = hub;
            }

            editingRow.dataset.hub = hub;

            editingRow.dataset.isDeleted = "false";

            updateStatus(editingRow, status);
        }

        // ADD NEW BRAND
        else {
            const tbody = document.querySelector("#brandTable tbody");

            if (!tbody) return;

            const row = document.createElement("tr");

            row.dataset.status = status;
            row.dataset.hub = hub;
            row.dataset.hubs = "";
            row.dataset.isDeleted = "false";
            row.dataset.previousStatus = status;
            row.dataset.dishCount = "0";
            row.dataset.orderCount = "0";

            const safeName = escapeHtml(name);
            const safeDescription = escapeHtml(description);

            row.innerHTML = `
                <td>
                    <div class="brand-thumbnail food-rice">
                        🍽️
                    </div>
                </td>

                <td class="brand-name-cell">
                    ${safeName}
                </td>

                <td>
                    ${safeDescription}
                </td>

                <td class="hub-number">
                    ${escapeHtml(hub)}
                </td>

                <td>
                    <span class="brand-status"></span>
                </td>

                <td>
                    <div class="brand-actions">

                        <button class="action-button view-button"
                                onclick="viewBrand(this)">
                            <span>◎</span> Xem
                        </button>

                        <button class="action-button edit-button"
                                onclick="editBrand(this)">
                            <span>✎</span> Sửa
                        </button>

                        <button class="action-button delete-button"
                                onclick="deleteBrand(this)">
                            <span>▤</span> Xóa
                        </button>

                    </div>
                </td>
            `;

            updateStatus(row, status);

            tbody.appendChild(row);
        }

        closeBrandModal();
        filterBrands();
    });
}


// =====================================================
// 6. VIEW BRAND
// =====================================================


function viewBrand(button) {
    const row = button.closest("tr");

    if (!row) {
        console.error("Không tìm thấy dòng thương hiệu.");
        return;
    }

    // Lấy ID thương hiệu từ nút hoặc dòng dữ liệu.
    // rowIndex chỉ dùng tạm cho dữ liệu mẫu chưa có ID thật.
    const brandId =
        button.dataset.id ||
        row.dataset.id ||
        row.dataset.brandId ||
        row.rowIndex;

    if (!brandId) {
        alert("Không xác định được ID thương hiệu.");
        return;
    }

    // Chuyển sang trang ViewBrand thay vì mở popup cũ.
    window.location.href =
        `/Admin/ViewBrand/${encodeURIComponent(brandId)}`;
}


function closeViewModal() {
    const modal = getElement("viewBrandModal");

    if (modal) {
        modal.classList.remove("show");
    }
}


// =====================================================
// 7. EDIT BRAND - NAVIGATE TO EDIT PAGE
// =====================================================

function editBrand(button) {
    const row = button.closest("tr");

    if (!row) {
        console.error("Không tìm thấy dòng thương hiệu.");
        return;
    }

    if (row.dataset.isDeleted === "true") {
        alert("Không thể sửa thương hiệu đã ngừng hoạt động. Vui lòng khôi phục trước.");
        return;
    }

    // Ưu tiên lấy ID thật từ nút hoặc dòng dữ liệu.
    // rowIndex chỉ là phương án tạm cho giao diện đang dùng dữ liệu mẫu.
    const brandId =
        button.dataset.id ||
        row.dataset.id ||
        row.dataset.brandId ||
        row.rowIndex;

    if (!brandId) {
        alert("Không xác định được ID thương hiệu.");
        return;
    }

    // Chuyển sang trang EditBrand, không mở popup brandModal cũ.
    window.location.href =
        `/Admin/EditBrand/${encodeURIComponent(brandId)}`;
}


// =====================================================
// 8. DELETE BRAND - CUSTOM CONFIRMATION POPUP
// =====================================================

// Mở popup thao tác thương hiệu
function deleteBrand(button) {
    const row = button.closest("tr");

    if (!row) {
        console.error("Không tìm thấy dòng thương hiệu.");
        return;
    }

    selectedDeleteRow = row;

    const brandNameElement =
        row.querySelector(".brand-name-cell");

    const brandName = brandNameElement
        ? brandNameElement.textContent.trim()
        : "thương hiệu";

    const dishCount = row.dataset.dishCount || "0";
    const orderCount = row.dataset.orderCount || "0";

    // Trạng thái được lấy trực tiếp từ dữ liệu database
    const isPaused = row.dataset.status === "paused";
    const isDeleted = row.dataset.isDeleted === "true";

    const modal = getElement("deleteBrandModal");

    if (!modal) {
        alert("Không tìm thấy popup xác nhận thương hiệu.");
        return;
    }

    // Không cho thao tác lại với thương hiệu đã xóa mềm
    if (isDeleted) {
        alert("Thương hiệu này đã được xóa.");
        return;
    }

    const heading =
        getElement("deleteModalHeading");

    const title =
        getElement("deleteWarningTitle");

    const details =
        getElement("deleteWarningDetails");

    const restoreButton =
        document.querySelector(".delete-restore-button");

    const confirmButton =
        document.querySelector(".delete-confirm-button");

    const description =
        document.querySelector(".delete-description");

    // ---------------------------------------------
    // TRƯỜNG HỢP 1: THƯƠNG HIỆU ĐANG TẠM DỪNG
    // ---------------------------------------------
    if (isPaused) {

        if (heading) {
            heading.textContent =
                "Quản lý thương hiệu đang tạm dừng";
        }

        if (title) {
            title.textContent =
                `Thương hiệu '${brandName}' đang tạm dừng.`;
        }

        if (details) {
            details.textContent =
                `(${dishCount} món ăn, ${orderCount} đơn hàng liên quan).`;
        }

        if (description) {
            description.textContent =
                "Bạn có thể khôi phục thương hiệu để tiếp tục kinh doanh hoặc xác nhận xóa để đưa thương hiệu ra khỏi danh sách quản lý. Dữ liệu liên quan vẫn được giữ trong hệ thống.";
        }

        if (restoreButton) {
            restoreButton.disabled = false;
            restoreButton.style.display = "flex";
            restoreButton.title = "Khôi phục thương hiệu";
        }

        if (confirmButton) {
            confirmButton.disabled = false;
            confirmButton.style.display = "flex";
            confirmButton.innerHTML =
                'Xác nhận xóa <span>→</span>';
        }
    }

    // ---------------------------------------------
    // TRƯỜNG HỢP 2: THƯƠNG HIỆU ĐANG HOẠT ĐỘNG
    // ---------------------------------------------
    else {

        if (heading) {
            heading.textContent =
                "Xác nhận tạm dừng thương hiệu";
        }

        if (title) {
            title.textContent =
                `Bạn muốn tạm dừng '${brandName}'?`;
        }

        if (details) {
            details.textContent =
                `(${dishCount} món ăn, ${orderCount} đơn hàng liên quan).`;
        }

        if (description) {
            description.textContent =
                "Thương hiệu sẽ chuyển sang trạng thái Tạm dừng và không tiếp tục kinh doanh. Dữ liệu món ăn, đơn hàng và lịch sử liên quan vẫn được giữ lại. Bạn có thể khôi phục thương hiệu sau đó.";
        }

        if (restoreButton) {
            restoreButton.disabled = true;
            restoreButton.style.display = "none";
        }

        if (confirmButton) {
            confirmButton.disabled = false;
            confirmButton.style.display = "flex";
            confirmButton.innerHTML =
                'Xác nhận tạm dừng <span>→</span>';
        }
    }

    modal.classList.add("show");
}


// =====================================================
// 9. CLOSE DELETE MODAL
// =====================================================

function closeDeleteModal() {
    const modal = getElement("deleteBrandModal");

    if (modal) {
        modal.classList.remove("show");
    }

    selectedDeleteRow = null;
}


// =====================================================
// 10. CONFIRM PAUSE OR DELETE BRAND
// =====================================================

function confirmDeleteBrand() {

    if (!selectedDeleteRow) {
        alert("Vui lòng chọn thương hiệu cần thao tác.");
        return;
    }

    const row = selectedDeleteRow;

    const brandId = row.dataset.brandId;

    if (!brandId) {
        alert("Không tìm thấy mã thương hiệu.");
        return;
    }

    const isPaused = row.dataset.status === "paused";
    const isDeleted = row.dataset.isDeleted === "true";

    if (isDeleted) {
        alert("Thương hiệu này đã được xóa.");
        return;
    }

    // ---------------------------------------------
    // THƯƠNG HIỆU ĐANG HOẠT ĐỘNG
    // => GỬI YÊU CẦU TẠM DỪNG
    // ---------------------------------------------
    if (!isPaused) {

        const form =
            getElement("pauseBrandForm");

        const idInput =
            getElement("pauseBrandId");

        if (!form || !idInput) {
            alert("Không tìm thấy form tạm dừng thương hiệu.");
            return;
        }

        idInput.value = brandId;

        form.submit();

        return;
    }

    // ---------------------------------------------
    // THƯƠNG HIỆU ĐANG TẠM DỪNG
    // => GỬI YÊU CẦU XÓA MỀM
    // ---------------------------------------------

    const form =
        getElement("deleteBrandForm");

    const idInput =
        getElement("deleteBrandId");

    if (!form || !idInput) {
        alert("Không tìm thấy form xóa thương hiệu.");
        return;
    }

    idInput.value = brandId;

    form.submit();
}


// =====================================================
// 11. RESTORE BRAND
// =====================================================

function restoreBrand() {

    if (!selectedDeleteRow) {
        alert("Vui lòng chọn thương hiệu cần khôi phục.");
        return;
    }

    const row = selectedDeleteRow;

    const brandId = row.dataset.brandId;

    if (!brandId) {
        alert("Không tìm thấy mã thương hiệu.");
        return;
    }

    const isPaused = row.dataset.status === "paused";
    const isDeleted = row.dataset.isDeleted === "true";

    if (!isPaused || isDeleted) {
        alert("Chỉ có thể khôi phục thương hiệu đang tạm dừng.");
        return;
    }

    const form =
        getElement("restoreBrandForm");

    const idInput =
        getElement("restoreBrandId");

    if (!form || !idInput) {
        alert("Không tìm thấy form khôi phục thương hiệu.");
        return;
    }

    idInput.value = brandId;

    form.submit();
}


// =====================================================
// 11. CLOSE BRAND MODALS WHEN CLICKING OUTSIDE
// =====================================================

document.querySelectorAll(".brand-modal-overlay").forEach(modal => {
    modal.addEventListener("click", function (event) {
        if (event.target === modal) {
            modal.classList.remove("show");
            editingRow = null;
        }
    });
});

const deleteBrandModal = getElement("deleteBrandModal");

if (deleteBrandModal) {
    deleteBrandModal.addEventListener("click", function (event) {
        if (event.target === this) {
            closeDeleteModal();
        }
    });
}


// =====================================================
// 12. CREATE BRAND - HUB SELECTION
// =====================================================

function openHubModal() {
    const modal = getElement("hubModal");

    if (!modal) return;

    temporaryHubs = [...selectedHubs];

    document.querySelectorAll(".hub-option input").forEach(input => {
        input.checked = temporaryHubs.includes(input.value);
    });

    modal.classList.add("show");
}

function closeHubModal() {
    const modal = getElement("hubModal");

    if (modal) {
        modal.classList.remove("show");
    }
}


// Lưu Hub đã chọn
function saveSelectedHubs() {
    const checkedHubs = document.querySelectorAll(
        ".hub-option input:checked"
    );

    selectedHubs = Array.from(checkedHubs).map(input => input.value);

    renderSelectedHubs();

    closeHubModal();
}


// =====================================================
// 13. DISPLAY SELECTED HUBS
// =====================================================

function renderSelectedHubs() {
    const container = getElement("selectedHubs");

    if (!container) return;

    container.innerHTML = "";

    if (selectedHubs.length === 0) {
        container.innerHTML = `
            <span class="no-hub-text">
                Chưa chọn Hub nào
            </span>
        `;

        return;
    }

    selectedHubs.forEach(hubId => {
        const input = document.querySelector(
            `.hub-option input[value="${hubId}"]`
        );

        if (!input) return;

        const hubName = input.dataset.name;

        const chip = document.createElement("span");
        chip.className = "hub-chip";

        const name = document.createElement("span");
        name.textContent = hubName;

        const removeButton = document.createElement("button");

        removeButton.type = "button";
        removeButton.textContent = "×";
        removeButton.setAttribute("aria-label", `Bỏ ${hubName}`);

        removeButton.addEventListener("click", () => {
            selectedHubs = selectedHubs.filter(id => id !== hubId);

            input.checked = false;

            renderSelectedHubs();
        });

        chip.appendChild(name);
        chip.appendChild(removeButton);

        container.appendChild(chip);
    });
}


// =====================================================
// 14. CREATE BRAND - STATUS TOGGLE
// =====================================================

const statusCheckbox = getElement("brandStatus");

if (statusCheckbox) {
    statusCheckbox.addEventListener("change", function () {
        const statusText = getElement("statusText");

        if (!statusText) return;

        statusText.textContent = this.checked
            ? "Đang hoạt động"
            : "Tạm dừng";
    });
}


// =====================================================
// 15. CREATE BRAND - DESCRIPTION CHARACTER COUNT
// =====================================================

const descriptionInput = getElement("brandDescription");

if (descriptionInput) {
    descriptionInput.addEventListener("input", function () {
        const counter = getElement("descriptionCount");

        if (counter) {
            counter.textContent = this.value.length;
        }
    });
}


// =====================================================
// 16. CREATE BRAND - LOGO PREVIEW
// =====================================================

const logoInput = getElement("brandLogo");

if (logoInput) {
    logoInput.addEventListener("change", function () {
        const file = this.files[0];

        const error = getElement("fileError");
        const preview = getElement("logoPreview");

        if (!error || !preview) return;

        error.textContent = "";

        preview.classList.remove("show");
        preview.removeAttribute("src");

        if (!file) return;

        const allowedTypes = ["image/jpeg", "image/png"];
        const maxSize = 5 * 1024 * 1024;

        if (!allowedTypes.includes(file.type)) {
            error.textContent = "Chỉ chấp nhận file JPG hoặc PNG.";
            this.value = "";
            return;
        }

        if (file.size > maxSize) {
            error.textContent = "Dung lượng ảnh không được vượt quá 5MB.";
            this.value = "";
            return;
        }

        const reader = new FileReader();

        reader.onload = function (event) {
            preview.src = event.target.result;
            preview.classList.add("show");
        };

        reader.readAsDataURL(file);
    });
}


// =====================================================
// 17. CREATE BRAND - FORM SUBMIT
// =====================================================

const createBrandForm = getElement("createBrandForm");

if (createBrandForm) {
    createBrandForm.addEventListener("submit", function (event) {
        event.preventDefault();

        const brandNameElement = getElement("brandName");
        const logoFile = logoInput ? logoInput.files[0] : null;

        const brandName = brandNameElement
            ? brandNameElement.value.trim()
            : "";

        if (!brandName) {
            alert("Vui lòng nhập tên thương hiệu.");
            return;
        }

        if (!logoFile) {
            alert("Vui lòng tải lên logo thương hiệu.");
            return;
        }

        if (selectedHubs.length === 0) {
            alert("Vui lòng chọn ít nhất một Hub liên kết.");
            openHubModal();
            return;
        }

        const status = statusCheckbox && statusCheckbox.checked
            ? "Đang hoạt động"
            : "Tạm dừng";

        alert(
            "Thông tin thương hiệu đã được kiểm tra!\n\n" +
            "Tên: " + brandName + "\n" +
            "Số Hub: " + selectedHubs.length + "\n" +
            "Trạng thái: " + status +
            "\n\nĐây là bản giao diện mẫu, dữ liệu chưa được lưu vào SQL Server."
        );
    });
}


// =====================================================
// 18. CREATE BRAND - CLOSE HUB MODAL
// =====================================================

const hubModal = getElement("hubModal");

if (hubModal) {
    hubModal.addEventListener("click", function (event) {
        if (event.target === this) {
            closeHubModal();
        }
    });
}


// =====================================================
// 19. INITIALIZE CREATE BRAND PAGE
// =====================================================

renderSelectedHubs();

const initialDescription = getElement("brandDescription");
const initialCounter = getElement("descriptionCount");

if (initialDescription && initialCounter) {
    initialCounter.textContent = initialDescription.value.length;
}


// =====================================================
// 20. EDIT BRAND - INITIAL DATA
// =====================================================

const originalEditBrand = {
    name: "Cơm tấm Sài Gòn",

    description:
        "Thương hiệu cơm tấm truyền thống Sài Gòn, nổi tiếng với sườn nướng, bì, chả và hương vị đậm đà đặc trưng.",

    status: "active",

    hubs: ["HBT", "H1", "H3"]
};

let editSelectedHubs = [...originalEditBrand.hubs];


// =====================================================
// 21. EDIT BRAND - RENDER SELECTED HUBS
// =====================================================

function renderEditSelectedHubs() {
    const container = getElement("editSelectedHubs");

    if (!container) return;

    container.innerHTML = "";

    if (editSelectedHubs.length === 0) {
        container.innerHTML = `
            <span class="no-hub-text">
                Chưa có Hub nào được liên kết
            </span>
        `;
        return;
    }

    editSelectedHubs.forEach(hubId => {

        const checkbox = document.querySelector(
            `#editHubModal input[value="${hubId}"]`
        );

        if (!checkbox) return;

        const hubName = checkbox.dataset.name;

        const chip = document.createElement("span");
        chip.className = "edit-hub-chip";

        const name = document.createElement("span");
        name.textContent = hubName;

        const removeButton = document.createElement("button");
        removeButton.type = "button";
        removeButton.textContent = "×";
        removeButton.setAttribute("aria-label", `Bỏ ${hubName}`);

        removeButton.addEventListener("click", () => {

            editSelectedHubs = editSelectedHubs.filter(
                id => id !== hubId
            );

            checkbox.checked = false;

            renderEditSelectedHubs();
        });

        chip.appendChild(name);
        chip.appendChild(removeButton);

        container.appendChild(chip);
    });
}


// =====================================================
// 22. EDIT BRAND - OPEN HUB MODAL
// =====================================================

function openEditHubModal() {
    const modal = getElement("editHubModal");

    if (!modal) {
        console.error("Không tìm thấy editHubModal.");
        return;
    }

    document.querySelectorAll(
        "#editHubModal .hub-option input"
    ).forEach(checkbox => {

        checkbox.checked =
            editSelectedHubs.includes(checkbox.value);

    });

    modal.classList.add("show");
}


// =====================================================
// 23. EDIT BRAND - CLOSE HUB MODAL
// =====================================================

function closeEditHubModal() {
    const modal = getElement("editHubModal");

    if (!modal) return;

    document.querySelectorAll(
        "#editHubModal .hub-option input"
    ).forEach(checkbox => {

        checkbox.checked =
            editSelectedHubs.includes(checkbox.value);

    });

    modal.classList.remove("show");
}


// =====================================================
// 24. EDIT BRAND - SAVE SELECTED HUBS
// =====================================================

function saveEditSelectedHubs() {

    const checkedHubs = document.querySelectorAll(
        "#editHubModal .hub-option input:checked"
    );

    editSelectedHubs = Array.from(checkedHubs).map(
        checkbox => checkbox.value
    );

    renderEditSelectedHubs();

    closeEditHubModal();
}


// =====================================================
// 25. EDIT BRAND - REMOVE HUB
// =====================================================

function removeEditHub(hubId) {

    editSelectedHubs = editSelectedHubs.filter(
        id => id !== hubId
    );

    const checkbox = document.querySelector(
        `#editHubModal input[value="${hubId}"]`
    );

    if (checkbox) {
        checkbox.checked = false;
    }

    renderEditSelectedHubs();
}


// =====================================================
// 26. EDIT BRAND - LOGO PREVIEW
// =====================================================

const editLogoInput = getElement("editBrandLogo");

if (editLogoInput) {

    editLogoInput.addEventListener("change", function () {

        const file = this.files[0];

        const error = getElement("editFileError");
        const preview = getElement("editCurrentImage");

        if (!error || !preview) return;

        error.textContent = "";

        if (!file) return;

        const allowedTypes = [
            "image/jpeg",
            "image/png"
        ];

        const maxSize = 2 * 1024 * 1024;

        if (!allowedTypes.includes(file.type)) {
            error.textContent =
                "Chỉ chấp nhận ảnh JPG hoặc PNG.";

            this.value = "";
            return;
        }

        if (file.size > maxSize) {
            error.textContent =
                "Dung lượng ảnh không được vượt quá 2MB.";

            this.value = "";
            return;
        }

        const reader = new FileReader();

        reader.onload = function (event) {

            preview.innerHTML = "";

            const image = document.createElement("img");

            image.src = event.target.result;
            image.alt = "Logo thương hiệu mới";

            preview.appendChild(image);
        };

        reader.readAsDataURL(file);
    });
}


// =====================================================
// 27. EDIT BRAND - RESTORE ORIGINAL DATA
// =====================================================

function restoreEditBrand() {

    const confirmed = confirm(
        "Bạn có chắc muốn khôi phục thông tin ban đầu?"
    );

    if (!confirmed) return;

    const name = getElement("editBrandName");
    const description = getElement("editBrandDescription");
    const status = getElement("editBrandStatus");
    const preview = getElement("editCurrentImage");
    const error = getElement("editFileError");

    if (name) {
        name.value = originalEditBrand.name;
    }

    if (description) {
        description.value = originalEditBrand.description;
    }

    if (status) {
        status.value = originalEditBrand.status;
    }

    editSelectedHubs = [...originalEditBrand.hubs];

    document.querySelectorAll(
        "#editHubModal .hub-option input"
    ).forEach(checkbox => {

        checkbox.checked =
            editSelectedHubs.includes(checkbox.value);

    });

    if (preview) {
        preview.innerHTML = "<span>🍛</span>";
    }

    if (editLogoInput) {
        editLogoInput.value = "";
    }

    if (error) {
        error.textContent = "";
    }

    renderEditSelectedHubs();
}


// =====================================================
// 28. EDIT BRAND - SAVE CHANGES
// =====================================================

function saveEditBrand() {

    const nameElement = getElement("editBrandName");
    const descriptionElement = getElement("editBrandDescription");
    const statusElement = getElement("editBrandStatus");

    if (!nameElement || !descriptionElement || !statusElement) {
        console.error("Không tìm thấy các trường thông tin thương hiệu.");
        return;
    }

    const name = nameElement.value.trim();
    const description = descriptionElement.value.trim();
    const status = statusElement.value;

    if (!name) {
        alert("Vui lòng nhập tên thương hiệu.");
        nameElement.focus();
        return;
    }

    if (!description) {
        alert("Vui lòng nhập mô tả thương hiệu.");
        descriptionElement.focus();
        return;
    }

    if (editSelectedHubs.length === 0) {
        alert("Vui lòng liên kết ít nhất một Hub.");
        openEditHubModal();
        return;
    }

    const hubNames = editSelectedHubs.map(hubId => {

        const checkbox = document.querySelector(
            `#editHubModal input[value="${hubId}"]`
        );

        return checkbox
            ? checkbox.dataset.name
            : hubId;

    });

    const statusText = status === "active"
        ? "Đang hoạt động"
        : "Tạm dừng";

    alert(
        "Thông tin thương hiệu đã được kiểm tra!\n\n" +
        "Tên thương hiệu: " + name + "\n" +
        "Mô tả: " + description + "\n" +
        "Trạng thái: " + statusText + "\n" +
        "Hub liên kết: " + hubNames.join(", ") +
        "\n\nĐây là bản giao diện mẫu. Dữ liệu chưa được lưu vào SQL Server."
    );
}


// =====================================================
// 29. EDIT BRAND - CLOSE HUB MODAL WHEN CLICKING OUTSIDE
// =====================================================

const editHubModalElement = getElement("editHubModal");

if (editHubModalElement) {

    editHubModalElement.addEventListener("click", function (event) {

        if (event.target === this) {
            closeEditHubModal();
        }

    });
}


// =====================================================
// 30. EDIT BRAND - INITIALIZE PAGE
// =====================================================

function initializeEditBrandPage() {

    if (!getElement("editBrandForm")) {
        return;
    }

    editSelectedHubs = [...originalEditBrand.hubs];

    document.querySelectorAll(
        "#editHubModal .hub-option input"
    ).forEach(checkbox => {

        checkbox.checked =
            editSelectedHubs.includes(checkbox.value);

    });

    renderEditSelectedHubs();
}

if (document.readyState === "loading") {
    document.addEventListener(
        "DOMContentLoaded",
        initializeEditBrandPage
    );
} else {
    initializeEditBrandPage();
}
