(function (window, $) {
  'use strict';
  if (!$ || !$.ajax) throw new Error('ReportsIndexService requires jQuery.');
  function postJson(url, data, token) {
    return $.ajax({
      url: url, method: 'POST', contentType: 'application/json; charset=utf-8',
      dataType: 'json', data: JSON.stringify(data), headers: { RequestVerificationToken: token, Accept: 'application/json' }
    });
  }
  function exportFile(url, format, data, token, signed) {
    return new Promise(function (resolve, reject) {
      $.ajax({
        url: url + (url.indexOf('?') < 0 ? '?' : '&') + 'format=' + encodeURIComponent(format) + (signed ? '&signed=true' : ''),
        method: 'POST', contentType: 'application/json; charset=utf-8',
        data: JSON.stringify(data), xhrFields: { responseType: 'blob' },
        headers: { RequestVerificationToken: token },
        success: function (blob, status, xhr) { resolve({ blob: blob, disposition: xhr.getResponseHeader('Content-Disposition') }); },
        error: function (xhr) { reject(xhr); }
      });
    });
  }
  function verifyPackage(url, file, token) {
    var form = new FormData();
    form.append('package', file);
    return $.ajax({
      url: url, method: 'POST', data: form,
      processData: false, contentType: false, dataType: 'json',
      headers: { RequestVerificationToken: token }
    });
  }
  window.ReportsIndexService = Object.freeze({
    patients: function (url) { return $.ajax({ url: url, method: 'GET', dataType: 'json' }); },
    preview: postJson,
    recent: function (url, patientId) { return $.ajax({ url: url, method: 'GET', data: { patientId: patientId }, dataType: 'json' }); },
    exportFile: exportFile,
    verifyPackage: verifyPackage
  });
})(window, window.jQuery);
