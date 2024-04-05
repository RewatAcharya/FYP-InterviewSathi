$(document).ready(function () {
    loadTotalUserChart();
});

function loadTotalUserChart() {
    $.ajax({
        url: "Dashboard/GetTotalUserRadial",
        type: "GET",
        dataType: "json",
        success: function (data) {
            document.querySelector("#spanTotalUserCount").innerHTML = data.totalCount;

            var sectionCurrentCount = document.createElement("span");
            if (data.hasIncreased) {
                sectionCurrentCount.className = "text-success me-1";
                sectionCurrentCount.innerHTML = '<i class="bi bi-arrow-up-right-circle me-1"></i> <span>' + data.totalCurrent + '</span>';
            }
            else {
                sectionCurrentCount.className = "text-danger me-1";
                sectionCurrentCount.innerHTML = '<i class="bi bi-arrow-down-right-circle me-1"></i><span>' + data.totalCurrent + '</span>';
            }
            document.querySelector("#sectionUserCount").append(sectionCurrentCount);
            document.querySelector("#sectionUserCount").append("since last month");

            loadRadialBarChart("totalUsersRadialChart", data);
        }
    });
}