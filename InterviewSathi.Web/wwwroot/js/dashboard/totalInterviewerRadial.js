$(document).ready(function () {
    loadTotalInterviewerChart();
});

function loadTotalInterviewerChart() {
    $.ajax({
        url: "Dashboard/GetTotalInterviewerRadial",
        type: "GET",
        dataType: "json",
        success: function (data) {
            document.querySelector("#spanTotalInterviewerUserCount").innerHTML = data.totalCount;

            var sectionCurrentCount = document.createElement("span");
            if (data.hasIncreased) {
                sectionCurrentCount.className = "text-success me-1";
                sectionCurrentCount.innerHTML = '<i class="bi bi-arrow-up-right-circle me-1"></i> <span>' + data.totalCurrent + '</span>';
            }
            else {
                sectionCurrentCount.className = "text-danger me-1";
                sectionCurrentCount.innerHTML = '<i class="bi bi-arrow-down-right-circle me-1"></i><span>' + data.totalCurrent + '</span>';
            }
            document.querySelector("#sectionInterviewerUserCount").append(sectionCurrentCount);
            document.querySelector("#sectionInterviewerUserCount").append("since last month");

            loadRadialBarChart("totalInterviewerUserRadialChart", data);
        }
    });
}