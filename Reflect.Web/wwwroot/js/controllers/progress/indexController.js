
(function () {
  'use strict';

  const ProgressIndexController = {
    weeklyChart: null,
    durationChart: null,

    init() {
      const root = document.getElementById('progress-dashboard');

      if (!root || typeof Chart === 'undefined') {
        return;
      }

      let data;

      try {
        data = JSON.parse(root.dataset.chart);
      } catch (error) {
        console.error('Invalid Progress chart data.', error);
        return;
      }

      this.data = data;
      this.renderCharts();

      document.addEventListener(
        'reflect:themechange',
        () => this.renderCharts()
      );
    },

    renderCharts() {
      if (this.weeklyChart) {
        this.weeklyChart.destroy();
        this.weeklyChart = null;
      }

      if (this.durationChart) {
        this.durationChart.destroy();
        this.durationChart = null;
      }

      const styles = getComputedStyle(document.documentElement);

      const navy = `rgb(${styles.getPropertyValue('--success-rgb').trim()})`;

      const coral = `rgb(${styles.getPropertyValue('--primary-rgb').trim()})`;

      const text = styles.getPropertyValue('--default-text-color').trim();

      const grid = styles.getPropertyValue('--default-border').trim();

      const weeklyCanvas =
        document.getElementById('progress-weekly-chart');

      const durationCanvas =
        document.getElementById('progress-duration-chart');

      if (weeklyCanvas) {
        this.weeklyChart = new Chart(weeklyCanvas, {
          type: 'bar',

          data: {
            labels: this.data.labels,
            datasets: [
              {
                label: 'Reflect XR',
                data: this.data.xr,
                backgroundColor: navy,
                borderRadius: 4
              },
              {
                label: 'Reflect Mobile',
                data: this.data.mobile,
                backgroundColor: coral,
                borderRadius: 4
              }
            ]
          },

          options: {
            responsive: true,
            maintainAspectRatio: false,

            plugins: {
              legend: {
                position: 'bottom',
                labels: { color: text }
              }
            },

            scales: {
              x: {
                stacked: false,
                ticks: { color: text },
                grid: { display: false }
              },

              y: {
                beginAtZero: true,
                ticks: {
                  color: text,
                  precision: 0
                },
                grid: { color: grid }
              }
            }
          }
        });
      }

      if (durationCanvas) {
        this.durationChart = new Chart(durationCanvas, {
          type: 'bar',

          data: {
            labels: this.data.durationLabels,
            datasets: [
              {
                label: 'Sessions',
                data: this.data.durationCounts,
                backgroundColor: navy,
                borderRadius: 4
              }
            ]
          },

          options: {
            responsive: true,
            maintainAspectRatio: false,

            plugins: {
              legend: { display: false }
            },

            scales: {
              x: {
                ticks: { color: text },
                grid: { display: false }
              },

              y: {
                beginAtZero: true,
                ticks: {
                  color: text,
                  precision: 0
                },
                grid: { color: grid }
              }
            }
          }
        });
      }
    }
  };

  ProgressIndexController.init();
})();
