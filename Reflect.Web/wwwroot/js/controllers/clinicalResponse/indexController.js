(function (window, document, $) {
  'use strict';
  function init() {
    var config = document.getElementById('clinical-response-config');
    if (!config || !$ || !window.ClinicalResponseIndexService) return;
    var service = window.ClinicalResponseIndexService;
    var input = document.getElementById('clinical-patient-id');
    var loadButton = document.getElementById('clinical-load');
    var createButton = document.getElementById('clinical-create-open');
    var patientLabel = document.getElementById('clinical-patient-label');
    var tbody = document.getElementById('clinical-observations-body');
    var count = document.getElementById('clinical-count');
    var empty = document.getElementById('clinical-empty');
    var alertBox = document.getElementById('clinical-alert');
    var loading = document.getElementById('clinical-loading');
    var form = document.getElementById('clinical-create-form');
    var formError = document.getElementById('clinical-form-error');
    var save = document.getElementById('clinical-save');
    var modalElement = document.getElementById('clinical-create-modal');
    var modal = window.bootstrap && window.bootstrap.Modal ? window.bootstrap.Modal.getOrCreateInstance(modalElement) : null;
    var selected = null;
    var requestSequence = 0;
    function showError(element, message) { element.textContent = message; element.classList.remove('d-none'); }
    function clearError(element) { element.textContent = ''; element.classList.add('d-none'); }
    function errorText(xhr) {
      if (xhr && xhr.status === 403) return 'You are not authorized to access this patient.';
      if (xhr && xhr.status === 401) return 'Your session has expired. Please sign in again.';
      if (xhr && xhr.responseJSON && xhr.responseJSON.message) return String(xhr.responseJSON.message);
      if (xhr && typeof xhr.responseText === 'string' && xhr.responseText.length < 250 && xhr.status === 400) return xhr.responseText;
      return 'The request could not be completed.';
    }
    function cell(row, value) { var td = document.createElement('td'); td.textContent = value == null ? '' : String(value); row.appendChild(td); }
    function easternDate(value) {
      return window.ReflectDate.toUsDateTimeString(value);
    }
    function render(data) {
      selected = { id: data.patientId, code: data.patientCode };
      input.value = selected.id;
      patientLabel.textContent = selected.code + ' — Clinical observations';
      createButton.disabled = false;
      tbody.replaceChildren();
      var rows = Array.isArray(data.recentObservations) ? data.recentObservations : [];
      rows.forEach(function (observation) {
        var tr = document.createElement('tr');
        cell(tr, easternDate(observation.observedAt));
        cell(tr, observation.observationType);
        cell(tr, observation.notes);
        cell(tr, observation.clinicianId);
        tbody.appendChild(tr);
      });
      count.textContent = rows.length + (rows.length === 1 ? ' observation' : ' observations');
      empty.textContent = 'No observations recorded for this patient.';
      empty.classList.toggle('d-none', rows.length > 0);
    }
    function load(id) {
      var requestId = ++requestSequence;
      selected = null; createButton.disabled = true; tbody.replaceChildren();
      count.textContent = 'Loading…';
      empty.classList.add('d-none'); clearError(alertBox);
      loading.classList.remove('d-none');
      return service.get(config.dataset.indexUrl, id).then(function (data) {
        if (requestId === requestSequence) render(data);
      }).catch(function (xhr) {
        if (requestId === requestSequence) {
          count.textContent = 'Unable to load';
          showError(alertBox, errorText(xhr));
        }
      }).always(function () {
        if (requestId === requestSequence) loading.classList.add('d-none');
      });
    }
    loadButton.addEventListener('click', function () {
      var id = input.value.trim();
      if (!/^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$/i.test(id)) {
        showError(alertBox, 'Enter a valid patient ID or open this page from the patient profile.');
        return;
      }
      load(id);
    });
    createButton.addEventListener('click', function () {
      if (!selected || !modal) return;
      form.reset(); clearError(formError);
      document.getElementById('clinical-form-patient').textContent = selected.code;
      modal.show();
    });
    form.addEventListener('submit', function (event) {
      event.preventDefault();
      if (!selected || !form.reportValidity()) return;
      clearError(formError); save.disabled = true;
      var patientId = selected.id;
      service.create(config.dataset.createUrl, {
        patientCode: selected.code,
        observationType: document.getElementById('clinical-type').value.trim(),
        notes: document.getElementById('clinical-notes').value.trim()
      }, form.querySelector('input[name="__RequestVerificationToken"]').value).then(function () {
        modal.hide();
        return load(patientId);
      }).catch(function (xhr) {
        showError(formError, errorText(xhr));
      }).always(function () { save.disabled = false; });
    });
    var initialId = new URLSearchParams(window.location.search).get('patientId');
    if (initialId) { input.value = initialId; load(initialId); }
  }
  window.ClinicalResponseIndexController = Object.freeze({ init: init });
  if (document.readyState === 'loading') document.addEventListener('DOMContentLoaded', init, { once: true });
  else init();
})(window, document, window.jQuery);
