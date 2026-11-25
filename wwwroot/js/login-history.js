document.addEventListener("DOMContentLoaded", function () {

    const filter = document.getElementById(
        "loginStatusFilter"
    );

    const tableBody = document.getElementById(
        "loginHistoryBody"
    );

    const recordCount = document.getElementById(
        "loginRecordCount"
    );

    const emptyState = document.getElementById(
        "emptyLoginHistory"
    );

    if (!filter || !tableBody) return;

    const rows = tableBody.querySelectorAll(
        "[data-login-status]"
    );

    function filterLoginHistory() {

        const selectedStatus = filter.value;

        let visibleCount = 0;

        rows.forEach(function (row) {

            const status = row.dataset.loginStatus;

            const matches =
                selectedStatus === "all" ||
                status === selectedStatus;

            row.hidden = !matches;

            if (matches) {
                visibleCount++;
            }

        });

        recordCount.textContent = visibleCount;

        emptyState.hidden = visibleCount !== 0;

    }

    filter.addEventListener(
        "change",
        filterLoginHistory
    );

    filterLoginHistory();

});