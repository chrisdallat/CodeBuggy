function extractLabels(data) {
    return data.map(entry => {
        var date = new Date(entry.date);
        return `${date.getFullYear()}-${(date.getMonth() + 1).toString().padStart(2, '0')}-${date.getDate().toString().padStart(2, '0')}`;
    });
}

function createChartData(labels, data, selectedOption) {
    switch (selectedOption) {
        case 'Status':
            return {
                toDoCountData: {
                    x: labels,
                    y: data.map(entry => entry.toDoCount),
                    type: 'line',
                    name: 'To Do',
                    line: { color: 'red' }
                },
                inProgressCountData: {
                    x: labels,
                    y: data.map(entry => entry.inProgressCount),
                    type: 'line',
                    name: 'In Progress',
                    line: { color: 'blue' }
                },
                reviewCountData: {
                    x: labels,
                    y: data.map(entry => entry.reviewCount),
                    type: 'line',
                    name: 'Review',
                    line: { color: 'yellow' }
                },
                doneCountData: {
                    x: labels,
                    y: data.map(entry => entry.doneCount),
                    type: 'line',
                    name: 'Done',
                    line: { color: 'green' }
                },
                totalOpenTicketsData: {
                    x: labels,
                    y: data.map(entry => entry.toDoCount + entry.inProgressCount + entry.reviewCount - entry.doneCount),
                    type: 'line',
                    name: 'Total Open Tickets',
                    line: { color: 'purple' }
                }
            };
        case 'Priority':
            return {
                nonePriorityCountData: {
                    x: labels,
                    y: data.map(entry => entry.nonePriorityCount),
                    type: 'line',
                    name: 'None Priority',
                    line: { color: 'gray' }
                },
                lowPriorityCountData: {
                    x: labels,
                    y: data.map(entry => entry.lowPriorityCount),
                    type: 'line',
                    name: 'Low Priority',
                    line: { color: 'purple' }
                },
                mediumPriorityCountData: {
                    x: labels,
                    y: data.map(entry => entry.mediumPriorityCount),
                    type: 'line',
                    name: 'Medium Priority',
                    line: { color: 'brown' }
                },
                highPriorityCountData: {
                    x: labels,
                    y: data.map(entry => entry.highPriorityCount),
                    type: 'line',
                    name: 'High Priority',
                    line: { color: 'orange' }
                },
                urgentPriorityCountData: {
                    x: labels,
                    y: data.map(entry => entry.urgentPriorityCount),
                    type: 'line',
                    name: 'Urgent Priority',
                    line: { color: 'purple' }
                },
                totalOpenTicketsData: {
                    x: labels,
                    y: data.map(entry => entry.toDoCount + entry.inProgressCount + entry.reviewCount - entry.doneCount),
                    type: 'line',
                    name: 'Total Open Tickets',
                    line: { color: 'purple' }
                }
            };
        default:
            // Default
            return {
                totalOpenTicketsData: {
                    x: labels,
                    y: data.map(entry => entry.toDoCount + entry.inProgressCount + entry.reviewCount - entry.doneCount),
                    type: 'line',
                    name: 'Total Open Tickets',
                    line: { color: 'purple' }
                }
            };
    }
}

function createChart(chartData, data) {
    var chartWidth = window.innerWidth - 40;
    var chartHeight = window.innerHeight - 100; 

    var datasets = Object.values(chartData);

    Plotly.newPlot('burndownChart', datasets, {
        xaxis: {
            title: 'Date'
        },
        yaxis: {
            title: 'Count',
            range: [0, Math.max(
                ...datasets.flatMap(dataset => dataset.y),
                ...data.map(entry => entry.toDoCount + entry.inProgressCount + entry.reviewCount - entry.doneCount)
            ) + 3] 
        },
        width: chartWidth,
        height: chartHeight,
        legend: {
            orientation: 'h', // 'h' for horizontal, 'v' for vertical
            y: 1.2, // Adjust this value to set the legend position
        }
    });
}

function OnSuccessResult(data) {
    // console.log('Received data from server:', JSON.stringify(data, null, 2));

    var labels = extractLabels(data);
    var selectedOption = $("#ChartType").val();
    var chartData = createChartData(labels, data, selectedOption);

    createChart(chartData, data);
    adjustFooterPosition();
}

function OnError(err) {
    console.log("COULD NOT FETCH DATA");
}

function fetchDataAndRenderChart() {
    $.ajax({
        type: "POST",
        url: "/Burndown/GetDailyTicketCounts",
        data: "",
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        success: OnSuccessResult,
        error: OnError,
    });
}

function adjustFooterPosition() {
    var chartHeight = document.getElementById('burndownChart').clientHeight;

    document.querySelector('.footer').style.marginTop = 1 + chartHeight + 'px';
}

$(function () {
    $("#bd-button").click(fetchDataAndRenderChart);
});
