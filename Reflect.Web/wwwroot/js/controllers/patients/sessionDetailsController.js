(function (window, document) {
  'use strict';
  function init() {
    var page = document.getElementById('session-detail-page');
    if (!page || page.dataset.initialized === 'true') return;
    page.dataset.initialized = 'true';
    var form = page.querySelector('.session-detail-review form');
    if (form) form.addEventListener('submit', function () {
      var button = form.querySelector('button[type="submit"]');
      if (button) button.disabled = true;
    });
  }
  window.PatientSessionDetailsController = Object.freeze({ init: init });
  if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', init, { once: true });
  } else { init(); }
})(window, document);
