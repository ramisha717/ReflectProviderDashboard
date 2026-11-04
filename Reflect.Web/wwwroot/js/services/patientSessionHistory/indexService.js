(function (window, $) {
  'use strict';
  if (!$ || typeof $.ajax !== 'function') {
    throw new Error('PatientSessionHistoryService requires jQuery.');
  }

  function getSessions(url, filters) {
    return $.ajax({
      url: url,
      method: 'GET',
      dataType: 'json',
      data: filters || {},
      headers: { Accept: 'application/json' }
    });
  }

  window.PatientSessionHistoryService = Object.freeze({ getSessions: getSessions });
})(window, window.jQuery);
