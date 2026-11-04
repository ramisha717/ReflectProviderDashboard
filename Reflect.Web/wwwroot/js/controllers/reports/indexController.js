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
    var token = root.querySelector('input[name="__RequestVerificationToken"]').value;
    var selectedTemplate = 0;
    var latestPreview = null;
    var requestVersion = 0;

    function showMessage(text, error) {
      message.hidden = !text;
      message.textContent = text || '';
      message.style.borderColor = error ? '#b91c1c' : '#b8c9de';
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
      if (xhr && xhr.responseJSON) return xhr.responseJSON.message || xhr.responseJSON.title || 'Request failed.';
      if (xhr && typeof xhr.responseText === 'string' && xhr.responseText.length < 180 && !xhr.responseText.includes('<')) return xhr.responseText;
      return 'The request could not be completed.';
    }
    function resetPreview() {
      latestPreview = null;
      previewPanel.hidden = true;
      $('reports-pdf').disabled = true;
      $('reports-csv').disabled = true;
    }
    function setTemplate(value) {
      selectedTemplate = Number(value);
      root.querySelectorAll('[data-template]').forEach(function (button) {
        var active = Number(button.dataset.template) === selectedTemplate;
        button.setAttribute('aria-pressed', active ? 'true' : 'false');
        button.classList.toggle('reports-primary', active);
      });
      resetPreview();
      showMessage('Selected ' + ['Engagement trends', 'XR vs Mobile', 'Session log'][selectedTemplate] + '. Choose a patient and generate a preview.');
    }
    function buildRequest() {
      if (!patientSelect.value) throw new Error('Select a patient first.');
      var now = new Date();
      var start = new Date(now.getTime() - Number($('reports-period').value) * 86400000);
      return {
        patientId: patientSelect.value, reportType: selectedTemplate,
        periodStart: start.toISOString(), periodEnd: now.toISOString(),
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
        (report.title || 'Report') + ' · ' + dateTime(report.periodStart) + ' – ' + dateTime(report.periodEnd);
      (report.sections || []).forEach(function (section) {
        var container = textNode('section', '', 'reports-preview-section');
        container.appendChild(textNode('h3', section.heading));
        var list = document.createElement('ul');
        (section.lines || []).forEach(function (line) { list.appendChild(textNode('li', line)); });
        container.appendChild(list);
        previewSections.appendChild(container);
      });
      if (!previewSections.children.length) previewSections.appendChild(textNode('p', 'No report sections selected.'));
      previewPanel.hidden = false;
      $('reports-pdf').disabled = false;
      $('reports-csv').disabled = false;
    }
    function renderRecent(items) {
      recentBody.replaceChildren();
      if (!Array.isArray(items) || !items.length) {
        var emptyRow = document.createElement('tr');
        var emptyCell = textNode('td', 'No exports for this patient yet.');
        emptyCell.colSpan = 5; emptyRow.appendChild(emptyCell); recentBody.appendChild(emptyRow); return;
      }
      items.slice().sort(function (a, b) { return new Date(b.exportedAt) - new Date(a.exportedAt); }).forEach(function (item) {
        var row = document.createElement('tr');
        [item.reportType, dateTime(item.exportedAt, true), dateTime(item.periodStart) + ' – ' + dateTime(item.periodEnd), item.format, item.status].forEach(function (value) {
          row.appendChild(textNode('td', value || '—'));
        });
        recentBody.appendChild(row);
      });
    }
    function refreshRecent() {
      var version = ++requestVersion;
      if (!patientSelect.value) { renderRecent([]); return; }
      service.recent(root.dataset.recentUrl, patientSelect.value).then(function (items) {
        if (version === requestVersion) renderRecent(items);
      }).catch(function (xhr) { if (version === requestVersion) showMessage('Could not load recent exports: ' + errorText(xhr), true); });
    }
    function setBusy(busy) {
      $('reports-preview-button').disabled = busy;
      $('reports-pdf').disabled = busy || !latestPreview;
      $('reports-csv').disabled = busy || !latestPreview;
    }
    function preview() {
      var request;
      try { request = buildRequest(); } catch (err) { showMessage(err.message, true); return; }
      resetPreview(); setBusy(true); showMessage('Generating report preview…');
      service.preview(root.dataset.previewUrl, request, token).then(function (data) {
        renderPreview(data); showMessage('Preview generated from recorded patient activity.');
      }).catch(function (xhr) { showMessage('Preview failed: ' + errorText(xhr), true); }).always(function () { setBusy(false); });
    }
    function exportReport(format) {
      var request;
      try { request = buildRequest(); } catch (err) { showMessage(err.message, true); return; }
      setBusy(true); showMessage('Preparing ' + format.toUpperCase() + ' export…');
      service.exportFile(root.dataset.exportUrl, format, request, token).then(function (result) {
        var url = URL.createObjectURL(result.blob);
        var link = document.createElement('a');
        link.href = url;
        link.download = 'Reflect_' + ['EngagementTrends', 'PlatformComparison', 'SessionLog'][selectedTemplate] + '_' + patientSelect.options[patientSelect.selectedIndex].text.replace(/[^a-zA-Z0-9_-]/g, '_') + '.' + format;
        document.body.appendChild(link); link.click(); link.remove();
        window.setTimeout(function () { URL.revokeObjectURL(url); }, 30000);
        showMessage('Export created.'); refreshRecent();
      }).catch(function (xhr) { showMessage('Export failed: ' + errorText(xhr), true); }).finally(function () { setBusy(false); });
    }
    root.querySelectorAll('[data-template]').forEach(function (button) {
      button.addEventListener('click', function () { setTemplate(button.dataset.template); });
    });
    patientSelect.addEventListener('change', function () { resetPreview(); refreshRecent(); });
    ['reports-period', 'reports-platform', 'reports-count', 'reports-module', 'reports-platform-section', 'reports-details'].forEach(function (id) {
      $(id).addEventListener('change', resetPreview);
    });
    $('reports-preview-button').addEventListener('click', preview);
    $('reports-pdf').addEventListener('click', function () { exportReport('pdf'); });
    $('reports-csv').addEventListener('click', function () { exportReport('csv'); });
    $('reports-new').addEventListener('click', function () {
      patientSelect.value = ''; $('reports-period').value = '30'; $('reports-platform').value = '';
      ['reports-count', 'reports-module', 'reports-platform-section', 'reports-details'].forEach(function (id) { $(id).checked = true; });
      setTemplate(0); refreshRecent(); patientSelect.focus();
    });
    var initialPatient = new URLSearchParams(window.location.search).get('patientId');
    service.patients(root.dataset.patientsUrl).then(function (data) {
      var rows = Array.isArray(data) ? data : (data.rows || []);
      patientSelect.replaceChildren();
      var placeholder = document.createElement('option'); placeholder.value = ''; placeholder.textContent = 'Select a patient'; patientSelect.appendChild(placeholder);
      rows.forEach(function (row) {
        var option = document.createElement('option'); option.value = row.patientId; option.textContent = row.patientCode; patientSelect.appendChild(option);
      });
      if (initialPatient && rows.some(function (r) { return r.patientId.toLowerCase() === initialPatient.toLowerCase(); })) {
        patientSelect.value = initialPatient; refreshRecent();
      }
    }).catch(function (xhr) { patientSelect.replaceChildren(); showMessage('Could not load patients: ' + errorText(xhr), true); });
    setTemplate(0);
  }
  window.ReportsIndexController = Object.freeze({ init: init });
  if (document.readyState === 'loading') document.addEventListener('DOMContentLoaded', init, { once: true }); else init();
})(window, document);
