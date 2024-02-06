
function initializeBurndownChart() {
    const ctx = document.getElementById('myChart').getContext('2d');
    const myChart = new Chart(ctx, {
        type: 'line', // Change chart type to 'line'
        data: {
            labels: ['January', 'February', 'March', 'April', 'May', 'June'],
            datasets: [{
                label: 'Remaining Tasks',
                data: [65, 59, 44, 35, 19, 0], // Adjust data to represent remaining tasks
                fill: false, // Set fill to false for an open line graph
                borderColor: 'rgba(75, 192, 192, 1)',
                borderWidth: 2,
                pointRadius: 5, // Set point radius for better visibility
                pointBackgroundColor: 'rgba(75, 192, 192, 1)', // Set point color
                pointBorderColor: 'rgba(75, 192, 192, 1)', // Set point border color
                pointHoverRadius: 8 // Set point hover radius
            }]
        },
        options: {
            scales: {
                y: {
                    beginAtZero: true
                }
            }
        }
    });
}

// Call the initialization function when the DOM is ready
document.addEventListener('DOMContentLoaded', function () {
    initializeBurndownChart();
});
