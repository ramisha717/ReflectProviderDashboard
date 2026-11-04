
// Overview screen controller: uses data already rendered by the Razor ViewModel.
const OverviewIndexController = {
  init() {
    const canvas = document.getElementById('weekly-engagement-chart');
    if (!canvas || typeof Chart === 'undefined') return;

    const labels = JSON.parse(canvas.dataset.labels || '[]');
    const xr = JSON.parse(canvas.dataset.xr || '[]');
    const mobile = JSON.parse(canvas.dataset.mobile || '[]');

    // Destroy the previous chart before recreating it.
    // This prevents the "Canvas is already in use" error.
    const existingChart = Chart.getChart(canvas);
    if (existingChart) {
      existingChart.destroy();
    }

    if (!labels.length) return;

    const css = getComputedStyle(document.documentElement);
    const themeColor = (name, fallback) =>
      css.getPropertyValue(name).trim() || fallback;

    const textColor = themeColor('--text-muted', '#8B90A0');
    const gridColor = themeColor('--border-color', '#F0F1F5');

    new Chart(canvas.getContext('2d'), {
      type: 'bar',
      data: {
        labels,
        datasets: [
          {
            label: 'Reflect XR',
            data: xr,
            backgroundColor: themeColor('--primary-navy', '#1F305E'),
            borderRadius: 4,
            barPercentage: 0.6,
            categoryPercentage: 0.7
          },
          {
            label: 'Reflect Mobile',
            data: mobile,
            backgroundColor: themeColor('--accent-coral', '#E87B7B'),
            borderRadius: 4,
            barPercentage: 0.6,
            categoryPercentage: 0.7
          }
        ]
      },
      options: {
        responsive: true,
        maintainAspectRatio: false,
        plugins: { legend: { display: false } },
        scales: {
          x: {
            grid: { display: false },
            ticks: { color: textColor }
          },
          y: {
            beginAtZero: true,
            grid: { color: gridColor },
            ticks: { color: textColor }
          }
        }
      }
    });
  }
};

// Initialize the chart when the page loads.
document.addEventListener('DOMContentLoaded', () => {
  OverviewIndexController.init();
});

// Reinitialize the chart when the user changes the theme.
document.addEventListener('reflect:themechange', () => {
  OverviewIndexController.init();
});
