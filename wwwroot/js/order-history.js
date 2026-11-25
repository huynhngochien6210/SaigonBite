document.addEventListener("DOMContentLoaded", function () {

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

    // Search and Filter

    function filterOrders() {

        const keyword = search.value.trim()
            .toLowerCase()
            .replace(/^#/, "");

        const selectedStatus = filter.value;

        let visibleCount = 0;

        rows.forEach(function (row) {

            const orderId =
                row.dataset.orderId.toLowerCase();

            const status =
                row.dataset.orderStatus;

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

        recordCount.textContent = visibleCount;

        emptyState.hidden = visibleCount !== 0;

    }

    search.addEventListener("input", filterOrders);

    filter.addEventListener("change", filterOrders);

    filterOrders();

    // Order Details

    const modalElement =
        document.getElementById("orderDetailModal");

    if (!modalElement || !window.bootstrap) return;

    const modal = bootstrap.Modal.getOrCreateInstance(
        modalElement
    );

    function setText(id, value) {

        const element = document.getElementById(id);

        if (element) {
            element.textContent = value;
        }

    }

    function formatCurrency(value) {

        return new Intl.NumberFormat("vi-VN", {
            style: "currency",
            currency: "VND"
        }).format(Number(value));

    }

    function translateStatus(status) {

        const isVietnamese =
            document.documentElement.lang
                .toLowerCase().startsWith("vi");

        const statuses = {
            Delivered: "Đã giao",
            Cancelled: "Đã hủy",
            Failed: "Thất bại"
        };

        return isVietnamese
            ? statuses[status] || status
            : status;

    }

    document.querySelectorAll("[data-order-detail]")
        .forEach(function (button) {

            button.addEventListener("click", function () {

                const data = button.dataset;

                setText("detailOrderId", "#" + data.id);

                setText("detailOrderDate", data.date);

                setText("detailOrderItem", data.item);

                setText("detailOrderPayment", data.payment);

                setText(
                    "detailOrderStatus",
                    translateStatus(data.status)
                );

                setText(
                    "detailOrderTotal",
                    formatCurrency(data.total)
                );

                modal.show();

            });

        });

});