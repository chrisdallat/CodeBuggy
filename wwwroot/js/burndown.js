function extractLabels(data) {
    return data.map(entry => {
        var date = new Date(entry.date);
        return `${date.getFullYear()}-${(date.getMonth() + 1).toString().padStart(2, '0')}-${date.getDate().toString().padStart(2, '0')}`;
    });
}

function getStartDate(selectedPeriod) {
    var currentDate = new Date();
    
    switch (selectedPeriod) {
        case '1week':
            currentDate.setDate(currentDate.getDate() - 7);
            break;
        case '2week':
            currentDate.setDate(currentDate.getDate() - 14);
            break;
        case '3week':
            currentDate.setDate(currentDate.getDate() - 21);
            break;
        case '4week':
            currentDate.setDate(currentDate.getDate() - 28);
            break;
        case 'monthly':
            currentDate.setMonth(currentDate.getMonth() - 12);
            break;
        case 'alltime':
            currentDate.setMonth(currentDate.getMonth() - 120);
            break;
        default:
            currentDate.setMonth(currentDate.getMonth() - 120);
            break;
    }

    return currentDate.toISOString().split('T')[0];
}

function createChartData(labels, data, selectedOption) {

    switch (selectedOption) {
        case 'Status':
            default:
            return {
                toDoCountData: {
                    x: labels,
                    y: data.map(entry => entry.toDoCount),
                    type: 'line',
                    name: 'To Do',
                    line: { color: '#a30402' }
                },
                inProgressCountData: {
                    x: labels,
                    y: data.map(entry => entry.inProgressCount),
                    type: 'line',
                    name: 'In Progress',
                    line: { color: '#f27202' }
                },
                reviewCountData: {
                    x: labels,
                    y: data.map(entry => entry.reviewCount),
                    type: 'line',
                    name: 'Review',
                    line: { color: '#f2ba02' }
                },
                doneCountData: {
                    x: labels,
                    y: data.map(entry => entry.doneCount),
                    type: 'line',
                    name: 'Done',
                    line: { color: '#00cf6b' }
                },
                totalOpenTicketsData: {
                    x: labels,
                    y: data.map(entry => entry.toDoCount + entry.inProgressCount + entry.reviewCount),
                    type: 'line',
                    name: 'Total Open Tickets',
                    line: { color: '#00bacf' }
                }
            };
        case 'Priority':
            return {
                lowPriorityCountData: {
                    x: labels,
                    y: data.map(entry => entry.lowPriorityCount),
                    type: 'line',
                    name: 'Low Priority',
                    line: { color: '#6bcf00' }
                },
                mediumPriorityCountData: {
                    x: labels,
                    y: data.map(entry => entry.mediumPriorityCount),
                    type: 'line',
                    name: 'Medium Priority',
                    line: { color: '#f2ba02' }
                },
                highPriorityCountData: {
                    x: labels,
                    y: data.map(entry => entry.highPriorityCount),
                    type: 'line',
                    name: 'High Priority',
                    line: { color: '#f27202' }
                },
                urgentPriorityCountData: {
                    x: labels,
                    y: data.map(entry => entry.urgentPriorityCount),
                    type: 'line',
                    name: 'Urgent Priority',
                    line: { color: '#a30402' }
                },
                totalOpenTicketsData: {
                    x: labels,
                    y: data.map(entry => entry.toDoCount + entry.inProgressCount + entry.reviewCount),
                    type: 'line',
                    name: 'Total Open Tickets',
                    line: { color: '#00bacf' }
                }
            };
        case 'Total Open Tickets':
            return {
                totalOpenTicketsData: {
                    x: labels,
                    y: data.map(entry => entry.toDoCount + entry.inProgressCount + entry.reviewCount),
                    type: 'line',
                    name: 'Total Open Tickets',
                    line: { color: '#00bacf' }
                }
            };
    }
}

function createChart(chartData, data) {
    var chartWidth = window.innerWidth - 100;
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
                ...data.map(entry => entry.toDoCount + entry.inProgressCount + entry.reviewCount)
            ) + 3] 
        },
        width: chartWidth,
        height: chartHeight,
        legend: {
            orientation: 'h', 
            y: 1.2, 
        }
    });
}

function fillShortRange(filteredData, startDate) {
    if (filteredData.length > 0 && filteredData[0].date !== startDate) {
        var missingEntry = { date: startDate };

        Object.keys(filteredData[0]).forEach(key => {
            if (key !== 'date') {
                missingEntry[key] = filteredData[0][key];
            }
        });
        filteredData.unshift(missingEntry);
    }
    return filteredData;
}

function fillLongRange(filteredData, startDate) {
    if (filteredData.length > 0) {
        var missingEntryDate = new Date(startDate);
        missingEntryDate.setDate(missingEntryDate.getDate() - 1);

        var missingEntry = { date: missingEntryDate.toISOString().split('T')[0] };

        Object.keys(filteredData[0]).forEach(key => {
            if (key !== 'date') {
                missingEntry[key] = 0;
            }
        });

        filteredData.unshift(missingEntry);
    }
    return filteredData;
}

function OnSuccessResult(data) {
    var firstDate = data[0].date;
    var selectedType = $("#ChartType").val();
    var selectedDateRange = $("#TimePeriod").val();
    var startDate = getStartDate(selectedDateRange);
    var filteredData = data.filter(entry => entry.date >= startDate);
    
    if(firstDate < startDate) {
        filteredData = fillShortRange(filteredData, startDate);
    }
    else {
        filteredData = fillLongRange(filteredData, firstDate);
    }
    var labels = extractLabels(filteredData);
    var chartData = createChartData(labels, filteredData, selectedType);
    createChart(chartData, filteredData);
}

function OnError(err) {
    console.log("COULD NOT FETCH DATA");
}

function fetchDataAndRenderChart(projectId) {
    $.ajax({
        type: "POST",
        url: "/Burndown/GetDailyTicketCounts?projectId=" + projectId,
        data: "",
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        success: OnSuccessResult,
        error: OnError,
    });
}
