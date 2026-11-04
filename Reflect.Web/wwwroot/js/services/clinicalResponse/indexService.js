(function (window, $) {
  'use strict';
  window.ClinicalResponseIndexService = Object.freeze({
    get: function (url, patientId) {
      return $.ajax({ url: url, method: 'GET', dataType: 'json', data: { patientId: patientId } });
    },
    create: function (url, values, token) {
      return $.ajax({
        url: url, method: 'POST', dataType: 'json',
        data: { PatientCode: values.patientCode, ObservationType: values.observationType, Notes: values.notes, __RequestVerificationToken: token }
      });
    }
  });
})(window, window.jQuery);
