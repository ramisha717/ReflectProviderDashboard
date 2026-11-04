(function (window, $) {
  'use strict';
  if (!$ || typeof $.ajax !== 'function') {
    throw new Error('MonitoringDocumentationService requires jQuery.');
  }

  function patients(url) {
    return $.ajax({ url: url, method: 'GET', dataType: 'json' });
  }

  function summary(url, patientCode, days) {
    return $.ajax({
      url: url, method: 'GET', dataType: 'json',
      data: { patientCode: patientCode, days: days }
    });
  }

  function regenerate(url, values, token) {
    return $.ajax({
      url: url, method: 'POST', dataType: 'json',
      headers: { RequestVerificationToken: token },
      data: values
    });
  }

  function save(url, patientCode, notes, token) {
    return $.ajax({
      url: url, method: 'POST', dataType: 'json',
      headers: { RequestVerificationToken: token },
      data: { patientCode: patientCode, notes: notes }
    });
  }

  function markReviewed(url, patientCode, notes, token) {
    return $.ajax({
      url: url, method: 'POST', dataType: 'json',
      headers: { RequestVerificationToken: token },
      data: { patientCode: patientCode, notes: notes }
    });
  }

  window.MonitoringDocumentationService = Object.freeze({
    patients: patients,
    summary: summary,
    regenerate: regenerate,
    save: save,
    markReviewed: markReviewed
  });
})(window, window.jQuery);
