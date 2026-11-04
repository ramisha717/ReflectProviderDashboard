(function (window, document) {
  'use strict';

  function init() {
    var root = document.getElementById('monitoring-root');
    if (!root || root.dataset.initialized === 'true') return;
    root.dataset.initialized = 'true';

    var service = window.MonitoringDocumentationService;
    if (!service) throw new Error('MonitoringDocumentationService is missing.');

    function byId(id) { return document.getElementById(id); }
    var patientSelect = byId('monitoring-patient');
    var daysSelect = byId('monitoring-days');
    var notes = byId('monitoring-notes');
    var regenerateButton = byId('monitoring-regenerate');
    var saveButton = byId('monitoring-save');
    var reviewButton = byId('monitoring-review');
    var feedback = byId('monitoring-feedback');
    var loading = byId('monitoring-summary-loading');
    var sections = byId('monitoring-summary-sections');
    var reviewBadge = byId('monitoring-review-badge');
    var tokenElement = root.querySelector('input[name="__RequestVerificationToken"]');
    var token = tokenElement ? tokenElement.value : '';
    var rows = [];
    var currentRequest = 0;
    var busy = false;

    var include = [
      { checkbox: 'monitoring-include-sessions', section: 'monitoring-section-sessions', text: 'monitoring-summary-sessions', key: 'IncludeSessionCountAndDuration' },
      { checkbox: 'monitoring-include-platform', section: 'monitoring-section-platform', text: 'monitoring-summary-platform', key: 'IncludePlatformActivity' },
      { checkbox: 'monitoring-include-modules', section: 'monitoring-section-modules', text: 'monitoring-summary-modules', key: 'IncludeModuleActivity' },
      { checkbox: 'monitoring-include-observations', section: 'monitoring-section-observations', text: 'monitoring-summary-observations', key: 'IncludeClinicianObservations' }
    ];

    function errorText(xhr) {
      if (xhr && xhr.responseJSON && xhr.responseJSON.message) return xhr.responseJSON.message;
      if (xhr && typeof xhr.responseText === 'string' && xhr.responseText.length < 200 && xhr.responseText.indexOf('<') < 0) return xhr.responseText;
      return 'The request could not be completed. Please try again.';
    }

    function showMessage(message, isError) {
      feedback.textContent = message || '';
      feedback.classList.toggle('is-error', !!isError);
    }

    function selectedPatient() {
      return rows.find(function (row) { return row.patientCode === patientSelect.value; });
    }

    function setBusy(value) {
      busy = value;
      var hasPatient = !!patientSelect.value;
      patientSelect.disabled = value || rows.length === 0;
      daysSelect.disabled = value;
      regenerateButton.disabled = value || !hasPatient;
      saveButton.disabled = value || !hasPatient;
      reviewButton.disabled = value || !hasPatient;
      notes.disabled = value || !hasPatient;
      include.forEach(function (entry) { byId(entry.checkbox).disabled = value; });
    }

    function periodText(days) {
      var now = new Date();
      var start = new Date(now.getTime() - Number(days) * 86400000);

      return 'Period: ' +
        window.ReflectDate.toUsDateString(start) +
        ' – ' +
        window.ReflectDate.toUsDateString(now) +
        ' (US Eastern)';
    }

    function renderSummary(data, preserveNotes) {
      byId('monitoring-profile-name').textContent = 'Profile: ' + (data.patientCode || patientSelect.value);
      byId('monitoring-period-text').textContent = periodText(data.days || daysSelect.value);
      var parts = String(data.generatedSummary || '').split(/\n\s*\n/);
      var position = 0;
      include.forEach(function (entry) {
        var enabled = !!data[entry.key.charAt(0).toLowerCase() + entry.key.slice(1)];
        // The server returns sections in the same order as these four settings.
        var section = byId(entry.section);
        section.hidden = !enabled;
        byId(entry.text).textContent = enabled ? (parts[position++] || 'No activity recorded.') : '';
      });
      if (!preserveNotes) notes.value = data.notes || '';
      sections.hidden = false;
      loading.hidden = true;
    }

    function updateBadge() {
      var patient = selectedPatient();
      var reviewed = patient && String(patient.reviewStatus).toLowerCase() === 'reviewed';
      reviewBadge.textContent = reviewed ? 'PATIENT REVIEWED' : 'DRAFT · REVIEW REQUIRED';
    }

    function clearPreview() {
      sections.hidden = true;
      byId('monitoring-profile-name').textContent = patientSelect.value ? 'Profile: ' + patientSelect.value : 'Select a patient';
      byId('monitoring-period-text').textContent = patientSelect.value ? periodText(daysSelect.value) : 'Select a patient and reporting period to generate a summary.';
      notes.value = '';
    }

    function loadSummary() {
      var patientCode = patientSelect.value;
      var requestId = ++currentRequest;
      clearPreview();
      updateBadge();
      if (!patientCode) { setBusy(false); return; }
      showMessage('');
      loading.hidden = false;
      setBusy(true);
      service.summary(root.dataset.summaryUrl, patientCode, Number(daysSelect.value))
        .done(function (data) {
          if (requestId !== currentRequest) return;
          renderSummary(data, false);
        })
        .fail(function (xhr) {
          if (requestId !== currentRequest) return;
          loading.hidden = true;
          showMessage(errorText(xhr), true);
        })
        .always(function () {
          if (requestId === currentRequest) setBusy(false);
        });
    }

    function selectedValues() {
      var values = {
        patientCode: patientSelect.value,
        Days: Number(daysSelect.value),
        Notes: notes.value
      };
      include.forEach(function (entry) {
        values[entry.key] = byId(entry.checkbox).checked ? 'true' : 'false';
      });
      return values;
    }

    regenerateButton.addEventListener('click', function () {
      if (busy || !patientSelect.value) return;
      var values = selectedValues();
      var requestId = ++currentRequest;
      showMessage('');
      setBusy(true);
      loading.hidden = false;
      service.regenerate(root.dataset.regenerateUrl, values, token)
        .done(function (data) {
          if (requestId !== currentRequest) return;
          renderSummary(data, true);
          showMessage('Draft regenerated. Save it to retain your clinician notes.');
        })
        .fail(function (xhr) {
          if (requestId !== currentRequest) return;
          loading.hidden = true;
          showMessage(errorText(xhr), true);
        })
        .always(function () {
          if (requestId === currentRequest) setBusy(false);
        });
    });

    saveButton.addEventListener('click', function () {
      if (busy || !patientSelect.value) return;
      setBusy(true);
      showMessage('');
      service.save(root.dataset.saveUrl, patientSelect.value, notes.value, token)
        .done(function () { showMessage('Clinician notes saved as a draft.'); })
        .fail(function (xhr) { showMessage(errorText(xhr), true); })
        .always(function () { setBusy(false); });
    });


    reviewButton.addEventListener('click', function () {
      if (busy || !patientSelect.value) return;

      window.MessageService.showConfirmationDialogSuccess(
        'Mark Patient Reviewed',
        'Mark this patient reviewed? Any clinician notes entered here will be saved as a clinical observation.',
        'Mark Reviewed',
        'Cancel',
        function () {
          if (busy || !patientSelect.value) return;

          setBusy(true);
          showMessage('');

          service.markReviewed(
            root.dataset.reviewUrl,
            patientSelect.value,
            notes.value,
            token
          )
            .done(function () {
              var patient = selectedPatient();
              if (patient) patient.reviewStatus = 'Reviewed';
              updateBadge();
              showMessage('Patient marked reviewed. Clinician notes, if supplied, were recorded as an observation.');
            })
            .fail(function (xhr) {
              showMessage(errorText(xhr), true);
            })
            .always(function () {
              setBusy(false);
            });
        },
        window.MessageService.cancelAction
      );
    });


    patientSelect.addEventListener('change', loadSummary);
    daysSelect.addEventListener('change', loadSummary);

    service.patients(root.dataset.patientsUrl)
      .done(function (data) {
        rows = Array.isArray(data.rows) ? data.rows : [];
        patientSelect.replaceChildren();
        if (!rows.length) {
          var empty = document.createElement('option');
          empty.value = '';
          empty.textContent = 'No accessible patients';
          patientSelect.appendChild(empty);
          showMessage('No patient profiles are available to this clinician.');
          setBusy(false);
          return;
        }
        rows.forEach(function (row) {
          var option = document.createElement('option');
          option.value = row.patientCode;
          option.textContent = row.patientCode;
          patientSelect.appendChild(option);
        });
        var params = new URLSearchParams(window.location.search);
        var requestedCode = params.get('patientCode') || root.dataset.initialPatientCode;
        var requestedId = params.get('patientId');
        var match = rows.find(function (row) {
          return (requestedCode && row.patientCode.toLowerCase() === requestedCode.toLowerCase()) ||
            (requestedId && String(row.patientId).toLowerCase() === requestedId.toLowerCase());
        });
        if (requestedCode || requestedId) {
          if (match) patientSelect.value = match.patientCode;
          else showMessage('Requested patient is unavailable; showing the first accessible profile.', true);
        }
        loadSummary();
      })
      .fail(function (xhr) {
        patientSelect.replaceChildren();
        showMessage('Could not load patient profiles: ' + errorText(xhr), true);
      });
  }

  window.MonitoringDocumentationController = Object.freeze({ init: init });
  function start() { if (document.getElementById('monitoring-root')) init(); }
  if (document.readyState === 'loading') document.addEventListener('DOMContentLoaded', start, { once: true });
  else start();
})(window, document);
