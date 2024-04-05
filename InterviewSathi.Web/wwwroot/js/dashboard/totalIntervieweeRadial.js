$(document).ready(function () {
    loadIntervieweeUserChart();
});

function loadIntervieweeUserChart() {
    $.ajax({
        url: "Dashboard/GetTotalIntervieweeRadial",
        type: "GET",
        dataType: "json",
        success: function (data) {
            document.querySelector("#spanTotalIntervieweeCount").innerHTML = data.totalCount;

            var sectionCurrentCount = document.createElement("span");
            if (data.hasIncreased) {
                sectionCurrentCount.className = "text-success me-1";
                sectionCurrentCount.innerHTML = '<i class="bi bi-arrow-up-right-circle me-1"></i> <span>' + data.totalCurrent + '</span>';
            }
            else {
                sectionCurrentCount.className = "text-danger me-1";
                sectionCurrentCount.innerHTML = '<i class="bi bi-arrow-down-right-circle me-1"></i><span>' + data.totalCurrent + '</span>';
            }
            document.querySelector("#sectionIntervieweeCount").append(sectionCurrentCount);
            document.querySelector("#sectionIntervieweeCount").append("since last month");

            loadRadialBarChart("totalIntervieweeRadialChart", data);
        }
    });
}
