(function (window) {
  'use strict';

  const timeZone = 'America/New_York';

  const dateFormatter = new Intl.DateTimeFormat('en-US', {
    timeZone: timeZone,
    month: 'numeric',
    day: 'numeric',
    year: 'numeric'
  });


  const timeFormatter = new Intl.DateTimeFormat('en-US', {
    timeZone: timeZone,
    hour: 'numeric',
    minute: '2-digit',
    hour12: true
  });

  const chartDateFormatter = new Intl.DateTimeFormat('en-US', {
    timeZone: timeZone,
    month: 'short',
    day: 'numeric'
  });

  function parseDate(value) {
    if (value === null || value === undefined || value === '') {
      return null;
    }

    const date = value instanceof Date ? value : new Date(value);

    return Number.isNaN(date.getTime()) ? null : date;
  }

  function format(value, formatter, fallback) {
    const date = parseDate(value);
    return date ? formatter.format(date) : fallback;
  }

  window.ReflectDate = Object.freeze({
    toUsDateString: function (value, fallback = '—') {
      return format(value, dateFormatter, fallback);
    },

    toUsDateTimeString: function (value, fallback = '—') {
      const date = parseDate(value);
      if (!date) return fallback;

      return dateFormatter.format(date) + ' ' +
        timeFormatter.format(date);
    },

    toUsTimeString: function (value, fallback = '—') {
      return format(value, timeFormatter, fallback);
    },

    toChartDateString: function (value, fallback = '—') {
      return format(value, chartDateFormatter, fallback);
    }
  });

})(window);
