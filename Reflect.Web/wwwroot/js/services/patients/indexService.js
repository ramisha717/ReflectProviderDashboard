
(function (window, $) {
  'use strict';

  if (!$ || typeof $.ajax !== 'function') {
    throw new Error('PatientsIndexService requires jQuery.');
  }

  function getPatients(url) {
    return $.ajax({
      url: url,
      method: 'GET',
      dataType: 'json',
      headers: {
        'Accept': 'application/json'
      }
    });
  }

  function getPatientProfile(url, patientId) {
    return $.ajax({
      url: url,
      method: 'GET',
      data: {
        patientId: patientId
      },
      dataType: 'json',
      headers: {
        'Accept': 'application/json'
      }
    });
  }

  function addDemoPatient(url, patient, verificationToken) {
    return $.ajax({
      url: url,
      method: 'POST',
      contentType: 'application/json; charset=utf-8',
      dataType: 'json',
      data: JSON.stringify({
        patientCode: patient.patientCode,
        reviewStatus: patient.reviewStatus
      }),
      headers: {
        'Accept': 'application/json',
        'RequestVerificationToken': verificationToken
      }
    });
  }

  window.PatientsIndexService = Object.freeze({
    getPatients: getPatients,
    getPatientProfile: getPatientProfile,
    addDemoPatient: addDemoPatient
  });

})(window, window.jQuery);
