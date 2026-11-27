
document.addEventListener("DOMContentLoaded", function () {

    // ==========================================
    // 1. GET DOM ELEMENTS
    // ==========================================

    const search = document.getElementById("orderSearch");

    const filter = document.getElementById(
        "orderStatusFilter"
    );

    const tableBody = document.getElementById(
        "orderHistoryBody"
    );

    const recordCount = document.getElementById(
        "orderRecordCount"
    );

    const emptyState = document.getElementById(
        "emptyOrderHistory"
    );

    if (!search || !filter || !tableBody) return;

    const rows = tableBody.querySelectorAll(
        "[data-order-id]"
    );

    // ==========================================
    // 2. SEARCH AND FILTER
    // ==========================================

    function filterOrders() {

        const keyword = search.value.trim()
            .toLowerCase()
            .replace(/^#/, "");

        const selectedStatus = filter.value;

        let visibleCount = 0;

        rows.forEach(function (row) {

            const orderId =
                (row.dataset.orderId || "").toLowerCase();

            const status =
                row.dataset.orderStatus || "";

            const matchesSearch =
                orderId.includes(keyword);

            const matchesStatus =
                selectedStatus === "all" ||
                status === selectedStatus;

            const visible =
                matchesSearch && matchesStatus;

            row.hidden = !visible;

            if (visible) {
                visibleCount++;
            }

        });

        // Update record count

        if (recordCount) {
            recordCount.textContent = visibleCount;
        }

        // Display empty state

        if (emptyState) {
            emptyState.hidden = visibleCount !== 0;
        }

    }

    search.addEventListener("input", filterOrders);

    filter.addEventListener("change", filterOrders);

    filterOrders();

    // ==========================================
    // 3. LANGUAGE DETECTION
    // ==========================================

    function isVietnamese() {

        return document.documentElement.lang
            .toLowerCase()
            .startsWith("vi");

    }

    // ==========================================
    // 4. TRANSLATE ORDER STATUS
    // ==========================================

    function translateStatus(status) {

        const vietnameseStatuses = {

            Placed: "Đã đặt hàng",

            Confirmed: "Đã xác nhận",

            Preparing: "Đang chuẩn bị",

            Ready: "Đã chuẩn bị xong",

            OutForDelivery: "Đang giao hàng",

            Delivered: "Đã giao hàng",

            Failed: "Giao hàng thất bại",

            Cancelled: "Đã hủy"

        };

        const englishStatuses = {

            Placed: "Placed",

            Confirmed: "Confirmed",

            Preparing: "Preparing",

            Ready: "Ready",

            OutForDelivery: "Out for Delivery",

            Delivered: "Delivered",

            Failed: "Failed",

            Cancelled: "Cancelled"

        };

        return isVietnamese()
            ? vietnameseStatuses[status] || status
            : englishStatuses[status] || status;

    }

    // ==========================================
    // 5. FORMAT PAYMENT METHOD
    // ==========================================

    function translatePayment(payment) {

        if (!payment || payment === "Not available") {

            return isVietnamese()
                ? "Chưa có dữ liệu"
                : "Not available";

        }

        return payment;

    }

    // ==========================================
    // 6. FORMAT CURRENCY
    // ==========================================

    function formatCurrency(value) {

        const amount = Number(value);

        if (
            value == null ||
            String(value).trim() === "" ||
            !Number.isFinite(amount)
        ) {

            return isVietnamese()
                ? "Chưa có dữ liệu"
                : "Not available";

        }

        return new Intl.NumberFormat("vi-VN", {

            style: "currency",

            currency: "VND",

            maximumFractionDigits: 0

        }).format(amount);

    }

    // ==========================================
    // 7. SET ELEMENT TEXT
    // ==========================================

    function setText(id, value) {

        const element = document.getElementById(id);

        if (element) {

            element.textContent = value ?? "";

        }

    }

    // ==========================================
    // 8. INITIALIZE ORDER DETAIL MODAL
    // ==========================================

    const modalElement = document.getElementById(
        "orderDetailModal"
    );

    if (!modalElement || !window.bootstrap) return;

    const modal = bootstrap.Modal.getOrCreateInstance(
        modalElement
    );

    // ==========================================
    // 9. DISPLAY ORDER DETAILS
    // ==========================================

    document.querySelectorAll("[data-order-detail]")
        .forEach(function (button) {

            button.addEventListener("click", function () {

                const data = button.dataset;

                // Order ID

                setText(
                    "detailOrderId",
                    "#" + data.id
                );

                // Order Date

                setText(
                    "detailOrderDate",
                    data.date
                );

                // Order Items

                setText(
                    "detailOrderItem",
                    data.item || (
                        isVietnamese()
                            ? "Không có món ăn"
                            : "No items"
                    )
                );

                // Payment Method

                setText(
                    "detailOrderPayment",
                    translatePayment(data.payment)
                );

                // Order Status

                setText(
                    "detailOrderStatus",
                    translateStatus(data.status)
                );

                // Final Total

                setText(
                    "detailOrderTotal",
                    formatCurrency(data.total)
                );

                // Show modal

                modal.show();

            });

        });

});
