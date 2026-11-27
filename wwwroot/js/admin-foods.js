
document.addEventListener("DOMContentLoaded", function () {
    const searchInput = document.getElementById("foodSearch");
    const brandFilter = document.getElementById("brandFilter");
    const categoryFilter = document.getElementById("categoryFilter");
    const statusFilter = document.getElementById("statusFilter");

    const table = document.getElementById("foodTable");
    const emptyMessage = document.getElementById("emptyFoodMessage");
    const foodCount = document.getElementById("foodCount");

    if (!table) return;

    function applyFilters() {
        const keyword = searchInput.value.trim().toLocaleLowerCase("vi");
        const selectedBrand = brandFilter.value;
        const selectedCategory = categoryFilter.value;
        const selectedStatus = statusFilter.value;

        const rows = table.querySelectorAll("tbody tr");

        let visibleCount = 0;

        rows.forEach(function (row) {
            const rowText = row.innerText.toLocaleLowerCase("vi");

            const matchesKeyword = rowText.includes(keyword);
            const matchesBrand =
                selectedBrand === "all" ||
                row.dataset.brand === selectedBrand;

            const matchesCategory =
                selectedCategory === "all" ||
                row.dataset.category === selectedCategory;

            const matchesStatus =
                selectedStatus === "all" ||
                row.dataset.status === selectedStatus;

            const isVisible =
                matchesKeyword &&
                matchesBrand &&
                matchesCategory &&
                matchesStatus;

            row.hidden = !isVisible;

            if (isVisible) {
                visibleCount++;
            }
        });

        emptyMessage.style.display =
            visibleCount === 0 ? "block" : "none";

        if (foodCount) {
            foodCount.textContent =
                `Hiển thị ${visibleCount} trong tổng số ${rows.length} món ăn`;
        }
    }

    searchInput.addEventListener("input", applyFilters);
    brandFilter.addEventListener("change", applyFilters);
    categoryFilter.addEventListener("change", applyFilters);
    statusFilter.addEventListener("change", applyFilters);

    applyFilters();
});

// Xóa món ăn khỏi bảng dữ liệu mẫu
function deleteFood(button) {
    const row = button.closest("tr");

    if (!row) return;

    const foodName = row.querySelector(".food-name")?.textContent.trim();

    const confirmed = confirm(
        `Bạn có chắc chắn muốn xóa món "${foodName}" khỏi danh sách mẫu không?`
    );

    if (confirmed) {
        row.remove();

        const event = new Event("input");

        document.getElementById("foodSearch").dispatchEvent(event);
    }
}

// =====================================================
// EDIT FOOD - NAVIGATE TO EDIT PAGE
// =====================================================

function editFood(button) {
    const row = button.closest("tr");
    if (!row) return;

    if (row.dataset.isDeleted === "true") {
        alert("Món ăn đã ngừng kinh doanh. Vui lòng khôi phục trước khi sửa.");
        return;
    }

    const foodId =
        button.dataset.id ||
        row.dataset.id ||
        row.dataset.foodId;

    if (!foodId) {
        alert("Không xác định được mã món ăn.");
        return;
    }

    window.location.href =
        `/Admin/EditFood/${encodeURIComponent(foodId)}`;
}


// =====================================================
// DELETE FOOD - CONFIRMATION POPUP
// =====================================================

let selectedDeleteFoodRow = null;

function getFoodElement(id) {
    return document.getElementById(id);
}

function deleteFood(button) {
    const row = button.closest("tr");
    if (!row) return;

    selectedDeleteFoodRow = row;

    const nameElement = row.querySelector(".food-name");
    const foodName = nameElement
        ? nameElement.textContent.trim()
        : "Món ăn đang chọn";

    const orderCount = row.dataset.orderCount || "0";
    const isDeleted = row.dataset.isDeleted === "true";

    const modal = getFoodElement("deleteFoodModal");
    if (!modal) {
        alert("Không tìm thấy popup xác nhận xóa món ăn.");
        return;
    }

    const title = getFoodElement("foodDeleteWarningTitle");
    const details = getFoodElement("foodDeleteWarningDetails");
    const description = getFoodElement("foodDeleteDescription");

    const restoreButton = getFoodElement("restoreFoodButton");
    const confirmButton = getFoodElement("confirmFoodDeleteButton");

    if (isDeleted) {
        title.textContent =
            `Món ăn '${foodName}' đã ngừng kinh doanh.`;

        details.textContent =
            `(${orderCount} đơn hàng đang được lưu trữ).`;

        description.textContent =
            "Bạn có thể khôi phục món ăn này để tiếp tục kinh doanh. Dữ liệu đơn hàng và lịch sử liên quan vẫn được giữ nguyên.";

        restoreButton.disabled = false;
        confirmButton.style.display = "none";
    } else {
        title.textContent =
            `Bạn có chắc chắn muốn xóa món '${foodName}'?`;

        details.textContent =
            `Món ăn hiện có ${orderCount} đơn hàng liên quan.`;

        description.textContent =
            "Hệ thống sẽ ngừng kinh doanh món ăn này thay vì xóa vĩnh viễn, nhằm giữ lại dữ liệu đơn hàng và lịch sử giao dịch có liên quan.";

        restoreButton.disabled = true;
        confirmButton.style.display = "flex";
    }

    modal.classList.add("show");
}

function closeDeleteFoodModal() {
    const modal = getFoodElement("deleteFoodModal");

    if (modal) {
        modal.classList.remove("show");
    }

    selectedDeleteFoodRow = null;
}


// =====================================================
// CONFIRM SOFT DELETE FOOD
// =====================================================

function confirmDeleteFood() {
    if (!selectedDeleteFoodRow) return;

    const row = selectedDeleteFoodRow;

    if (row.dataset.isDeleted === "true") return;

    row.dataset.previousStatus =
        row.dataset.status || "available";

    row.dataset.isDeleted = "true";
    row.dataset.status = "unavailable";

    const statusElement = row.querySelector(".food-status");

    if (statusElement) {
        statusElement.innerHTML = "<span>×</span> Ngừng kinh doanh";
        statusElement.className = "food-status status-unavailable";
    }

    closeDeleteFoodModal();

    // Cập nhật bộ lọc nếu hàm này có sẵn trong trang.
    if (typeof filterFoods === "function") {
        filterFoods();
    }

    alert(
        "Đã ngừng kinh doanh món ăn.\n" +
        "Dữ liệu đơn hàng và lịch sử liên quan vẫn được giữ lại."
    );
}


// =====================================================
// RESTORE FOOD
// =====================================================

function restoreFood() {
    if (!selectedDeleteFoodRow) return;

    const row = selectedDeleteFoodRow;

    if (row.dataset.isDeleted !== "true") return;

    const previousStatus =
        row.dataset.previousStatus || "available";

    row.dataset.isDeleted = "false";
    row.dataset.status = previousStatus;

    const statusElement = row.querySelector(".food-status");

    if (statusElement) {
        if (previousStatus === "available") {
            statusElement.innerHTML = "<span>✓</span> Đang kinh doanh";
            statusElement.className = "food-status status-available";
        } else {
            statusElement.innerHTML = "<span>×</span> Tạm ngừng";
            statusElement.className = "food-status status-unavailable";
        }
    }

    closeDeleteFoodModal();

    if (typeof filterFoods === "function") {
        filterFoods();
    }

    alert("Đã khôi phục món ăn thành công.");
}


// Đóng popup khi click ra ngoài
document.addEventListener("DOMContentLoaded", function () {
    const modal = getFoodElement("deleteFoodModal");

    if (modal) {
        modal.addEventListener("click", function (event) {
            if (event.target === modal) {
                closeDeleteFoodModal();
            }
        });
    }

    // Xử lý ảnh xem trước ở trang EditFood
    const imageInput = getFoodElement("editFoodImage");
    const imagePreview = getFoodElement("editFoodImagePreview");
    const imageEmoji = getFoodElement("editFoodImageEmoji");
    const imageError = getFoodElement("editFoodImageError");

    if (imageInput) {
        imageInput.addEventListener("change", function () {
            const file = this.files[0];

            imageError.textContent = "";
            imagePreview.style.display = "none";

            if (!file) return;

            if (!["image/jpeg", "image/png"].includes(file.type)) {
                imageError.textContent = "Chỉ chấp nhận ảnh JPG hoặc PNG.";
                this.value = "";
                return;
            }

            if (file.size > 5 * 1024 * 1024) {
                imageError.textContent = "Dung lượng ảnh không được vượt quá 5MB.";
                this.value = "";
                return;
            }

            const reader = new FileReader();

            reader.onload = function (event) {
                imagePreview.src = event.target.result;
                imagePreview.style.display = "block";
                imageEmoji.style.display = "none";
            };

            reader.readAsDataURL(file);
        });
    }

    // Form chỉnh sửa hiện đang chạy dữ liệu mẫu
    const editForm = getFoodElement("editFoodForm");

    if (editForm) {
        editForm.addEventListener("submit", function (event) {
            event.preventDefault();

            const name = getFoodElement("editFoodName").value.trim();
            const price = Number(getFoodElement("editFoodPrice").value);

            if (!name || price < 0 || !Number.isFinite(price)) {
                alert("Vui lòng kiểm tra lại tên món ăn và giá bán.");
                return;
            }

            alert(
                "Thông tin món ăn đã được kiểm tra!\n\n" +
                "Tên món: " + name +
                "\nGiá bán: " + price.toLocaleString("vi-VN") + " VNĐ" +
                "\n\nĐây là giao diện mẫu, dữ liệu chưa được lưu vào SQL Server."
            );
        });
    }
});