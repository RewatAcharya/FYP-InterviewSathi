$(document).ready(function () {
    loadPieChart();
});

function loadPieChart() {
    $.ajax({
        url: "Dashboard/PieChart",
        type: "GET",
        dataType: "json",
        success: function (data) {
          
            loadPieChartFunc("meetingsPieChart", data);
        }
    });
}

function loadPieChartFunc(id, data) {
    var chartColours = getChartColorsArray(id);
    options = {
        series: data.series,
        labels: data.labels,
        colors: chartColours,
        chart: {
            type: 'pie',
            width: 380
        },
        stroke: {
            show: false
        },
        legend: {
            position: 'bottom',
            horizontalAlign: 'center',
            labels: {
                colors: "#fff",
                useSeriesColors: true
            },
        },
    };

    var chart = new ApexCharts(document.querySelector("#" + id), options);

    chart.render();
}