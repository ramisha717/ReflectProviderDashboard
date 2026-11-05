
(function (window, document) {
  'use strict';

  function init() {
    var root = document.getElementById('reports-page');
    if (!root || root.dataset.initialized === 'true') return;
    root.dataset.initialized = 'true';

    var service = window.ReportsIndexService;
    if (!service) throw new Error('ReportsIndexService not loaded.');

    var $ = function (id) { return document.getElementById(id); };
    var patientSelect = $('reports-patient');
    var message = $('reports-message');
    var previewPanel = $('reports-preview-panel');
    var previewSections = $('reports-preview-sections');
    var recentBody = $('reports-recent-body');
    var verifyResult = $('reports-verify-result');
    var token = root.querySelector('input[name="__RequestVerificationToken"]').value;
    var selectedTemplate = 0;
    var latestPreview = null;
    var requestVersion = 0;
    var busy = false;

    function showMessage(text, error) {
      message.hidden = !text;
      message.textContent = text || '';
      message.style.borderColor = error ? 'rgb(var(--danger-rgb))' : 'var(--default-border)';
    }

    function textNode(tag, content, className) {
      var el = document.createElement(tag);
      el.textContent = content == null ? '' : String(content);
      if (className) el.className = className;
      return el;
    }

    function dateTime(value, withTime) {
      return withTime
        ? window.ReflectDate.toUsDateTimeString(value)
        : window.ReflectDate.toUsDateString(value);
    }

    function errorText(xhr) {
      if (xhr && xhr.responseJSON) {
        return xhr.responseJSON.message || xhr.responseJSON.title || 'Request failed.';
      }
      if (xhr && typeof xhr.responseText === 'string' &&
        xhr.responseText.length < 180 && !xhr.responseText.includes('<')) {
        return xhr.responseText;
      }
      return 'The request could not be completed.';
    }

    function updateButtons() {
      $('reports-preview-button').disabled = busy;
      ['reports-pdf', 'reports-csv', 'reports-signed-pdf', 'reports-signed-csv']
        .forEach(function (id) { $(id).disabled = busy || !latestPreview; });
    }

    function resetPreview() {
      latestPreview = null;
      previewPanel.hidden = true;
      updateButtons();
    }

    function setBusy(value) {
      busy = value;
      updateButtons();
    }

    function setTemplate(value) {
      selectedTemplate = Number(value);
      root.querySelectorAll('[data-template]').forEach(function (button) {
        var active = Number(button.dataset.template) === selectedTemplate;
        button.setAttribute('aria-pressed', active ? 'true' : 'false');
        button.classList.toggle('reports-primary', active);
      });
      resetPreview();
      showMessage('Selected ' + ['Engagement trends', 'XR vs Mobile', 'Session log'][selectedTemplate] +
        '. Choose a patient and generate a preview.');
    }

    function buildRequest() {
      if (!patientSelect.value) throw new Error('Select a patient first.');
      var now = new Date();
      var start = new Date(now.getTime() - Number($('reports-period').value) * 86400000);
      return {
        patientId: patientSelect.value,
        reportType: selectedTemplate,
        periodStart: start.toISOString(),
        periodEnd: now.toISOString(),
        platformFilter: $('reports-platform').value || null,
        includeSessionCountAndDuration: $('reports-count').checked,
        includePlatformActivity: $('reports-platform-section').checked,
        includeModuleActivity: $('reports-module').checked,
        includeSessionDetails: $('reports-details').checked
      };
    }

    function renderPreview(data) {
      latestPreview = data;
      previewSections.replaceChildren();
      var report = data.report || {};
      $('reports-preview-meta').textContent = (data.patientCode || '') + ' · ' +
        (report.title || 'Report') + ' · ' + dateTime(report.periodStart) +
        ' – ' + dateTime(report.periodEnd);
      (report.sections || []).forEach(function (section) {
        var container = textNode('section', '', 'reports-preview-section');
        container.appendChild(textNode('h3', section.heading));
        var list = document.createElement('ul');
        (section.lines || []).forEach(function (line) {
          list.appendChild(textNode('li', line));
        });
        container.appendChild(list);
        previewSections.appendChild(container);
      });
      if (!previewSections.children.length) {
        previewSections.appendChild(textNode('p', 'No report sections selected.'));
      }
      previewPanel.hidden = false;
      updateButtons();
    }

    function renderRecent(items) {
      recentBody.replaceChildren();
      if (!Array.isArray(items) || !items.length) {
        var emptyRow = document.createElement('tr');
        var emptyCell = textNode('td', 'No exports for this patient yet.');
        emptyCell.colSpan = 5;
        emptyRow.appendChild(emptyCell);
        recentBody.appendChild(emptyRow);
        return;
      }
      items.slice().sort(function (a, b) {
        return new Date(b.exportedAt) - new Date(a.exportedAt);
      }).forEach(function (item) {
        var row = document.createElement('tr');
        [item.reportType, dateTime(item.exportedAt, true),
        dateTime(item.periodStart) + ' – ' + dateTime(item.periodEnd),
        item.format, item.status].forEach(function (value) {
          row.appendChild(textNode('td', value || '—'));
        });
        recentBody.appendChild(row);
      });
    }

    function refreshRecent() {
      var version = ++requestVersion;
      if (!patientSelect.value) {
        renderRecent([]);
        return;
      }
      service.recent(root.dataset.recentUrl, patientSelect.value)
        .done(function (items) {
          if (version === requestVersion) renderRecent(items);
        })
        .fail(function (xhr) {
          if (version === requestVersion) {
            showMessage('Could not load recent exports: ' + errorText(xhr), true);
          }
        });
    }

    function preview() {
      if (busy) return;
      var request;
      try {
        request = buildRequest();
      } catch (err) {
        showMessage(err.message, true);
        return;
      }
      resetPreview();
      setBusy(true);
      showMessage('Generating report preview…');
      service.preview(root.dataset.previewUrl, request, token)
        .done(function (data) {
          renderPreview(data);
          showMessage('Preview generated from recorded patient activity.');
        })
        .fail(function (xhr) {
          showMessage('Preview failed: ' + errorText(xhr), true);
        })
        .always(function () {
          setBusy(false);
        });
    }

    function downloadBlob(blob, fileName) {
      var url = URL.createObjectURL(blob);
      var link = document.createElement('a');
      link.href = url;
      link.download = fileName;
      document.body.appendChild(link);
      link.click();
      link.remove();
      window.setTimeout(function () { URL.revokeObjectURL(url); }, 30000);
    }

    function exportReport(format, signed) {
      if (busy) return;
      var request;
      try {
        request = buildRequest();
      } catch (err) {
        showMessage(err.message, true);
        return;
      }
      setBusy(true);
      showMessage('Preparing ' + (signed ? 'signed ' : '') + format.toUpperCase() + ' export…');
      service.exportFile(root.dataset.exportUrl, format, request, token, signed)
        .then(function (result) {
          var patientName = patientSelect.options[patientSelect.selectedIndex].text
            .replace(/[^a-zA-Z0-9_-]/g, '_');
          var fileName = 'Reflect_' +
            ['EngagementTrends', 'PlatformComparison', 'SessionLog'][selectedTemplate] +
            '_' + patientName + '.' + format + (signed ? '.signed.zip' : '');
          downloadBlob(result.blob, fileName);
          showMessage(signed ? 'Signed report ZIP created.' : 'Export created.');
          refreshRecent();
        })
        .catch(function (xhr) {
          showMessage('Export failed: ' + errorText(xhr), true);
        })
        .finally(function () {
          setBusy(false);
        });
    }

    function verifySignedPackage() {
      var fileInput = $('reports-signed-package');
      var file = fileInput.files && fileInput.files[0];
      if (!file) {
        verifyResult.textContent = 'Select a signed report ZIP first.';
        return;
      }
      if (file.size > 10000000) {
        verifyResult.textContent = 'Select a signed report ZIP (10 MB maximum).';
        return;
      }
      $('reports-verify').disabled = true;
      verifyResult.textContent = 'Verifying signature…';
      service.verifyPackage(root.dataset.verifyUrl, file, token)
        .done(function (result) {
          verifyResult.textContent = result.message ||
            (result.valid ? 'Signature valid. Report unchanged.' : 'Signature invalid.');
        })
        .fail(function (xhr) {
          verifyResult.textContent = 'Verification failed: ' + errorText(xhr);
        })
        .always(function () {
          $('reports-verify').disabled = false;
        });
    }

    root.querySelectorAll('[data-template]').forEach(function (button) {
      button.addEventListener('click', function () {
        setTemplate(button.dataset.template);
      });
    });
    patientSelect.addEventListener('change', function () {
      resetPreview();
      refreshRecent();
    });
    ['reports-period', 'reports-platform', 'reports-count', 'reports-module',
      'reports-platform-section', 'reports-details'].forEach(function (id) {
        $(id).addEventListener('change', resetPreview);
      });

    $('reports-preview-button').addEventListener('click', preview);
    $('reports-pdf').addEventListener('click', function () { exportReport('pdf', false); });
    $('reports-csv').addEventListener('click', function () { exportReport('csv', false); });
    $('reports-signed-pdf').addEventListener('click', function () { exportReport('pdf', true); });
    $('reports-signed-csv').addEventListener('click', function () { exportReport('csv', true); });
    $('reports-verify').addEventListener('click', verifySignedPackage);
    $('reports-signed-package').addEventListener('change', function () {
      verifyResult.textContent = '';
    });

    $('reports-new').addEventListener('click', function () {
      patientSelect.value = '';
      $('reports-period').value = '30';
      $('reports-platform').value = '';
      ['reports-count', 'reports-module', 'reports-platform-section', 'reports-details']
        .forEach(function (id) { $(id).checked = true; });
      setTemplate(0);
      refreshRecent();
      patientSelect.focus();
    });

    var initialPatient = new URLSearchParams(window.location.search).get('patientId');
    service.patients(root.dataset.patientsUrl)
      .done(function (data) {
        var rows = Array.isArray(data) ? data : (data.rows || []);
        patientSelect.replaceChildren();
        var placeholder = document.createElement('option');
        placeholder.value = '';
        placeholder.textContent = 'Select a patient';
        patientSelect.appendChild(placeholder);
        rows.forEach(function (row) {
          var option = document.createElement('option');
          option.value = row.patientId;
          option.textContent = row.patientCode;
          patientSelect.appendChild(option);
        });
        if (initialPatient && rows.some(function (row) {
          return String(row.patientId).toLowerCase() === initialPatient.toLowerCase();
        })) {
          patientSelect.value = rows.find(function (row) {
            return String(row.patientId).toLowerCase() === initialPatient.toLowerCase();
          }).patientId;
          refreshRecent();
        }
      })
      .fail(function (xhr) {
        patientSelect.replaceChildren();
        showMessage('Could not load patients: ' + errorText(xhr), true);
      });

    setTemplate(0);
  }

  window.ReportsIndexController = Object.freeze({ init: init });
  if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', init, { once: true });
  } else {
    init();
  }
})(window, document);
