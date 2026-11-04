(function (window, document) {
  'use strict';

  function init() {
    var page = document.getElementById('session-history-page');
    if (!page || page.dataset.initialized === 'true') return;
    page.dataset.initialized = 'true';

    var service = window.PatientSessionHistoryService;
    if (!service) throw new Error('PatientSessionHistoryService is missing.');

    var form = document.getElementById('session-history-filters');
    var patientSelect = document.getElementById('history-patient');
    var platformSelect = document.getElementById('history-platform');
    var moduleSelect = document.getElementById('history-module');
    var startInput = document.getElementById('history-start');
    var endInput = document.getElementById('history-end');
    var loading = document.getElementById('session-history-loading');
    var error = document.getElementById('session-history-error');
    var empty = document.getElementById('session-history-empty');
    var table = document.getElementById('session-history-table');
    var tbody = document.getElementById('session-history-tbody');
    var preview = document.getElementById('session-history-preview');
    var previewPrimary = document.getElementById('session-preview-primary');
    var previewSecondary = document.getElementById('session-preview-secondary');
    var previewLink = document.getElementById('session-preview-link');
    var allSessions = [];
    var selectedId = null;
    var requestNumber = 0;

    function dateText(value) {
      return window.ReflectDate.toUsDateTimeString(value);
    }

    function timeText(value) {
      return window.ReflectDate.toUsTimeString(value);
    }
    function node(tag, className, value) {
      var el = document.createElement(tag);
      if (className) el.className = className;
      if (value !== undefined && value !== null) el.textContent = String(value);
      return el;
    }
    function sessionLabel(session) {
      // The backend stores GUID session IDs, not the Figma's illustrative S-00814 IDs.
      return String(session.sessionId || '').slice(0, 8).toUpperCase();
    }
    function platformLabel(value) {
      var text = String(value || '');
      if (text.toLowerCase().includes('mobile')) return 'Reflect Mobile';
      if (text.toLowerCase().includes('xr')) return 'Reflect XR';
      return text || '—';
    }
    function populateOptions(select, values, previous) {
      var seen = new Set();
      var options = values.filter(function (v) {
        var value = String(v || '').trim();
        if (!value || seen.has(value)) return false;
        seen.add(value);
        return true;
      }).sort(function (a, b) { return String(a).localeCompare(String(b)); });
      while (select.options.length > 1) select.remove(1);
      options.forEach(function (value) {
        var option = document.createElement('option');
        option.value = value;
        option.textContent = value;
        select.appendChild(option);
      });
      select.value = previous;
      if (select.value !== previous) select.value = '';
    }
    function setOptions(sessions) {
      var patient = patientSelect.value;
      var platform = platformSelect.value;
      var module = moduleSelect.value;
      populateOptions(patientSelect, sessions.map(function (s) { return s.patientCode; }), patient);
      populateOptions(platformSelect, sessions.map(function (s) { return s.platform; }), platform);
      populateOptions(moduleSelect, sessions.map(function (s) { return s.moduleName; }), module);
    }
    function detailHref(session) {
      var params = new URLSearchParams({
        sessionId: String(session.sessionId),
        patientId: String(session.patientId)
      });
      return page.dataset.detailUrl + '?' + params.toString();
    }
    function selectSession(session) {
      selectedId = String(session.sessionId);
      Array.prototype.forEach.call(tbody.querySelectorAll('tr'), function (row) {
        var selected = row.dataset.sessionId === selectedId;
        row.classList.toggle('is-selected', selected);
        row.setAttribute('aria-selected', String(selected));
      });
      previewPrimary.textContent = sessionLabel(session) + ' · ' + session.patientCode + ' · ' + platformLabel(session.platform);
      previewSecondary.textContent = 'Started ' + timeText(session.startedAt) + ' · Duration ' +
        session.durationMinutes + ' min · Module: ' + session.moduleName;
      previewLink.href = detailHref(session);
      preview.hidden = false;
    }
    function render(sessions) {
      tbody.replaceChildren();
      table.hidden = sessions.length === 0;
      empty.hidden = sessions.length !== 0;
      if (!sessions.length) {
        selectedId = null;
        preview.hidden = true;
        return;
      }
      sessions.forEach(function (session) {
        var row = node('tr');
        row.dataset.sessionId = String(session.sessionId);
        row.tabIndex = 0;
        row.setAttribute('aria-label', 'Preview session ' + sessionLabel(session) + ' for ' + session.patientCode);
        row.setAttribute('aria-selected', 'false');
        [sessionLabel(session), session.patientCode, dateText(session.startedAt),
        platformLabel(session.platform), session.moduleName,
        String(session.durationMinutes) + ' min', session.recordingStatus].forEach(function (value, index) {
          row.appendChild(node('td', index === 0 ? 'session-history-id' : '', value || '—'));
        });
        row.addEventListener('click', function () { selectSession(session); });
        row.addEventListener('keydown', function (event) {
          if (event.key === 'Enter' || event.key === ' ') {
            event.preventDefault();
            selectSession(session);
          }
        });
        var firstCell = row.cells[0];
        if (firstCell) {
          var link = node('a', 'session-history-detail-link', sessionLabel(session));
          link.href = detailHref(session);
          link.setAttribute('aria-label', 'Open session details for ' + sessionLabel(session));
          firstCell.replaceChildren(link);
        }
        tbody.appendChild(row);
      });
      var retained = sessions.find(function (s) { return String(s.sessionId) === selectedId; });
      selectSession(retained || sessions[0]);
    }
    function readFilters() {
      return {
        patientCode: patientSelect.value,
        platform: platformSelect.value,
        startDate: startInput.value,
        endDate: endInput.value,
        module: moduleSelect.value
      };
    }
    function load(filters, refreshOptions) {
      var requestId = ++requestNumber;
      loading.hidden = false;
      error.hidden = true;
      empty.hidden = true;
      table.hidden = true;
      preview.hidden = true;
      return service.getSessions(page.dataset.indexUrl, filters).then(function (data) {
        if (requestId !== requestNumber) return;
        var sessions = Array.isArray(data.sessions) ? data.sessions : [];
        if (refreshOptions) {
          allSessions = sessions;
          setOptions(allSessions);
        }
        render(sessions);
      }).catch(function (xhr) {
        if (requestId !== requestNumber) return;
        tbody.replaceChildren();
        preview.hidden = true;
        error.textContent = xhr && xhr.responseJSON && xhr.responseJSON.message
          ? xhr.responseJSON.message
          : 'Unable to load session history. Please try again.';
        error.hidden = false;
      }).always(function () {
        if (requestId === requestNumber) loading.hidden = true;
      });
    }

    form.addEventListener('submit', function (event) {
      event.preventDefault();
      if (startInput.value && endInput.value && startInput.value > endInput.value) {
        error.textContent = 'Start date cannot be after end date.';
        error.hidden = false;
        return;
      }
      load(readFilters(), false);
    });

    var params = new URLSearchParams(window.location.search);
    var initialPatientCode = params.get('patientCode') || '';
    load({}, true).then(function () {
      if (!initialPatientCode) return;
      patientSelect.value = initialPatientCode;
      if (patientSelect.value === initialPatientCode) load(readFilters(), false);
    });
  }

  window.PatientSessionHistoryController = Object.freeze({ init: init });
  if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', init, { once: true });
  } else {
    init();
  }
})(window, document);
