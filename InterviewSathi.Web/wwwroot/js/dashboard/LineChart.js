﻿$(document).ready(function () {
    loadCustomerAndBookingLineChart();
});

function loadCustomerAndBookingLineChart() {


    $.ajax({
        url: "/Dashboard/GetMemberAndMeetingLineChartData",
        type: 'GET',
        dataType: 'json',
        success: function (data) {

            loadLineChart("freeAndPaidLineChart", data);

        }
    });
}

function loadLineChart(id, data) {
    var chartColors = getChartColorsArray(id);
    var options = {
        colors: chartColors,
        chart: {
            height: 350,
            type: 'line',
            zoom: {
                type: 'x',
                enabled: true,
                autoScaleYaxis: true
            },
        },
        stroke: {
            curve: 'smooth',
            width: 2
        },
        series: data.series,
        dataLabels: {
            enabled: false,
        },
        markers: {
            size: 6,
            strokeWidth: 0,
            hover: {
                size: 9
            }
        },
        xaxis: {
            categories: data.categories,
            labels: {
                style: {
                    colors: "#000000",
                },
            }
        },
        yaxis: {
            labels: {
                style: {
                    colors: "#000000",
                },
            }
        },
        legend: {
            labels: {
                colors: "#000000",
            },
        },
        tooltip: {
            theme: 'dark'
        }
    };
    var chart = new ApexCharts(document.querySelector("#" + id), options);
    chart.render();
}