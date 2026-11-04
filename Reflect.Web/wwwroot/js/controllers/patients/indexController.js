
(function (window, document) {
  'use strict';

  var service = window.PatientsIndexService;

  function init(options) {
    if (!service) {
      throw new Error('PatientsIndexService must load before PatientsIndexController.');
    }

    var config = options || {};
    var configElement = document.getElementById('patients-config');

    if (!configElement) return;

    var dataUrl = config.indexUrl || configElement.dataset.indexUrl;
    var profileBaseUrl = config.profileUrl || configElement.dataset.profileUrl;
    var addDemoUrl = config.addDemoUrl || configElement.dataset.addDemoUrl;

    var loading = document.getElementById('patients-loading');
    var errorDiv = document.getElementById('patients-error');
    var emptyDiv = document.getElementById('patients-empty');
    var table = document.getElementById('patients-table');
    var tbody = document.getElementById('patients-tbody');
    var displayedCount = document.getElementById('displayed-count');
    var totalCount = document.getElementById('total-count');
    var searchInput = document.getElementById('patient-search-input');
    var filterAll = document.getElementById('filter-all');
    var filterReview = document.getElementById('filter-review');
    var filterActive = document.getElementById('filter-active');

    var addModalEl = document.getElementById('addPatientModal');
    var btnOpenModal = document.getElementById('btn-open-add-modal');
    var btnCloseModalX = document.getElementById('btn-close-modal-x');
    var btnCancelModal = document.getElementById('btn-cancel-modal');
    var addForm = document.getElementById('add-patient-form');
    var addError = document.getElementById('add-patient-error');

    var drawerEl = document.getElementById('profileOffcanvas');
    var drawerCloseX = document.getElementById('drawer-close-x');
    var drawerBackBtn = document.getElementById('drawer-back-btn');

    var allRows = [];
    var currentFilter = 'all';
    var currentQuery = '';
    var sortColumn = 'date';
    var sortAsc = false;
    var profileRequestId = 0;
    var lastDrawerTrigger = null;

    function parseDate(value) {
      if (!value) return null;
      var date = new Date(value);
      return Number.isNaN(date.getTime()) ? null : date;
    }

    function formatDate(value) {
      return window.ReflectDate.toUsDateString(value, '-');
    }

    function formatDateTime(value) {
      return window.ReflectDate.toUsDateTimeString(value, '-');
    }

    function formatWeek(value) {
      return window.ReflectDate.toChartDateString(value, '-');
    }

    function element(tag, className, text) {
      var result = document.createElement(tag);
      if (className) result.className = className;
      if (text !== undefined && text !== null) {
        result.textContent = String(text);
      }
      return result;
    }

    function icon(name, extraClass) {
      var result = element(
        'i',
        'ri-' + name + '-line' + (extraClass ? ' ' + extraClass : '')
      );
      result.setAttribute('aria-hidden', 'true');
      return result;
    }

    function cell(row, className, content) {
      var td = element('td', className);
      if (content instanceof Node) {
        td.appendChild(content);
      } else {
        td.textContent = String(content ?? '');
      }
      row.appendChild(td);
      return td;
    }

    function getDisplayName(row) {
      return String(row.patientCode || row.patientId || 'Unknown patient');
    }

    function isActive(row) {
      var date = parseDate(row.lastActive);
      if (!date) return false;
      var now = Date.now();
      return date.getTime() <= now &&
        date.getTime() >= now - 30 * 86400000;
    }

    function makeProfileLink(patientId, patientName, label, className) {
      var link = element('a', className, label);
      link.href = profileBaseUrl +
        '?patientId=' + encodeURIComponent(patientId);
      link.classList.add('btn-open-profile');

      link.addEventListener('click', function (event) {
        event.preventDefault();
        lastDrawerTrigger = link;
        openProfileDrawer(patientId, patientName);
      });

      return link;
    }

    function renderRows(rows) {
      tbody.replaceChildren();
      displayedCount.textContent = String(rows.length);
      emptyDiv.classList.toggle('d-none', rows.length !== 0);

      var fragment = document.createDocumentFragment();

      rows.forEach(function (row) {
        var tr = element('tr');
        var patientId = String(row.patientId || '');
        var patientName = getDisplayName(row);
        var needsReview = String(row.reviewStatus || '')
          .toLowerCase().includes('needs');

        var patientLink = makeProfileLink(
          patientId,
          patientName,
          patientName,
          'fw-bold text-decoration-none'
        );
        patientLink.style.color = 'var(--primary-navy)';
        patientLink.title = 'Open Patient Profile';

        cell(tr, 'col-patient', patientLink);
        cell(tr, 'col-date', formatDate(row.lastActive));
        cell(tr, 'col-sessions', row.numOfSessions ?? 0);
        cell(tr, 'col-platform', row.platform || '-');
        cell(tr, 'col-trend', row.trend || '-');

        cell(
          tr,
          'col-status' + (needsReview ? ' text-amber fw-bold' : ''),
          needsReview ? 'Needs review' : (row.reviewStatus || 'Reviewed')
        );

        cell(
          tr,
          'col-profile',
          makeProfileLink(
            patientId,
            patientName,
            'Open →',
            'profile-link'
          )
        );

        fragment.appendChild(tr);
      });

      tbody.appendChild(fragment);
    }

    function applyFilterAndSearch() {
      var filtered = allRows.slice();
      var query = currentQuery.trim().toLowerCase();

      if (query) {
        filtered = filtered.filter(function (row) {
          return [
            getDisplayName(row),
            row.patientCode,
            row.patientId,
            row.platform
          ].some(function (value) {
            return String(value || '').toLowerCase().includes(query);
          });
        });
      }

      if (currentFilter === 'review') {
        filtered = filtered.filter(function (row) {
          return String(row.reviewStatus || '')
            .toLowerCase().includes('needs');
        });
      } else if (currentFilter === 'active') {
        filtered = filtered.filter(isActive);
      }

      filtered.sort(function (a, b) {
        var left;
        var right;

        if (sortColumn === 'patient') {
          left = String(a.patientCode || '').toLowerCase();
          right = String(b.patientCode || '').toLowerCase();
        } else if (sortColumn === 'sessions') {
          left = Number(a.numOfSessions) || 0;
          right = Number(b.numOfSessions) || 0;
        } else if (sortColumn === 'status') {
          left = String(a.reviewStatus || '').toLowerCase();
          right = String(b.reviewStatus || '').toLowerCase();
        } else {
          var leftDate = parseDate(a.lastActive);
          var rightDate = parseDate(b.lastActive);
          left = leftDate ? leftDate.getTime() : 0;
          right = rightDate ? rightDate.getTime() : 0;
        }

        if (left < right) return sortAsc ? -1 : 1;
        if (left > right) return sortAsc ? 1 : -1;
        return 0;
      });

      renderRows(filtered);
    }

    document.querySelectorAll('.patients-table th.sortable')
      .forEach(function (th) {
        th.setAttribute('tabindex', '0');
        th.setAttribute('role', 'button');

        function activateSort() {
          var column = th.dataset.sort;
          if (sortColumn === column) {
            sortAsc = !sortAsc;
          } else {
            sortColumn = column;
            sortAsc = true;
          }

          document.querySelectorAll('.sort-icon')
            .forEach(function (sortIcon) {
              sortIcon.className = 'sort-icon ri-arrow-up-down-line';
            });

          var currentIcon = th.querySelector('.sort-icon');
          if (currentIcon) {
            currentIcon.className = sortAsc
              ? 'sort-icon ri-sort-asc'
              : 'sort-icon ri-sort-desc';
          }

          document.querySelectorAll('.patients-table th.sortable')
            .forEach(function (header) {
              header.setAttribute(
                'aria-sort',
                header === th
                  ? (sortAsc ? 'ascending' : 'descending')
                  : 'none'
              );
            });

          applyFilterAndSearch();
        }

        th.addEventListener('click', activateSort);
        th.addEventListener('keydown', function (event) {
          if (event.key === 'Enter' || event.key === ' ') {
            event.preventDefault();
            activateSort();
          }
        });
      });

    function ajaxErrorMessage(xhr, textStatus, errorThrown) {
      if (textStatus === 'timeout') return 'Request timed out.';
      if (textStatus === 'abort') return 'Request cancelled.';

      var response = xhr && xhr.responseJSON;
      if (response && typeof response.message === 'string') {
        return response.message;
      }
      if (response && typeof response.title === 'string') {
        return response.title;
      }
      if (xhr && xhr.status) return 'HTTP ' + xhr.status;
      return errorThrown || 'Network request failed.';
    }

    function loadData() {
      errorDiv.classList.add('d-none');

      return service.getPatients(dataUrl)
        .then(function (viewModel) {
          allRows = Array.isArray(viewModel.rows)
            ? viewModel.rows
            : [];

          totalCount.textContent = String(allRows.length);
          loading.classList.add('d-none');
          table.classList.remove('d-none');
          applyFilterAndSearch();
        })
        .catch(function (xhr) {
          loading.classList.add('d-none');
          errorDiv.textContent =
            'Failed to load patients: ' +
            ajaxErrorMessage(xhr);
          errorDiv.classList.remove('d-none');
          throw xhr;
        });
    }

    searchInput.addEventListener('input', function (event) {
      currentQuery = event.target.value;
      applyFilterAndSearch();
    });

    document.addEventListener('provider:search', function (event) {
      currentQuery = String(event.detail?.query || '');
      searchInput.value = currentQuery;
      applyFilterAndSearch();
    });

    function setActivePill(button, filterType) {
      [filterAll, filterReview, filterActive].forEach(function (item) {
        item.classList.remove('active');
        item.setAttribute('aria-pressed', 'false');
      });

      button.classList.add('active');
      button.setAttribute('aria-pressed', 'true');
      currentFilter = filterType;
      applyFilterAndSearch();
    }

    filterAll.addEventListener('click', function () {
      setActivePill(filterAll, 'all');
    });
    filterReview.addEventListener('click', function () {
      setActivePill(filterReview, 'review');
    });
    filterActive.addEventListener('click', function () {
      setActivePill(filterActive, 'active');
    });

    setActivePill(filterAll, 'all');

    function showModal() {
      if (window.bootstrap && window.bootstrap.Modal) {
        window.bootstrap.Modal.getOrCreateInstance(addModalEl).show();
      } else {
        addModalEl.classList.add('show');
        addModalEl.style.display = 'block';
        document.body.classList.add('modal-open');
      }
    }

    function hideModal() {
      if (window.bootstrap && window.bootstrap.Modal) {
        var instance = window.bootstrap.Modal.getInstance(addModalEl);
        if (instance) instance.hide();
      } else {
        addModalEl.classList.remove('show');
        addModalEl.style.display = 'none';
        document.body.classList.remove('modal-open');
      }
    }

    if (btnOpenModal) {
      btnOpenModal.addEventListener('click', showModal);
    }
    if (btnCloseModalX) {
      btnCloseModalX.addEventListener('click', hideModal);
    }
    if (btnCancelModal) {
      btnCancelModal.addEventListener('click', hideModal);
    }

    if (addForm) {
      addForm.addEventListener('submit', function (event) {
        event.preventDefault();

        addError.classList.add('d-none');
        addError.textContent = '';

        var submitButton = addForm.querySelector(
          'button[type="submit"]'
        );
        var tokenInput = addForm.querySelector(
          'input[name="__RequestVerificationToken"]'
        );
        var token = tokenInput ? tokenInput.value : '';

        submitButton.disabled = true;

        var patient = {
          patientCode: document.getElementById(
            'new-patient-code'
          ).value.trim(),
          reviewStatus: document.getElementById(
            'new-patient-status'
          ).value
        };

        service.addDemoPatient(addDemoUrl, patient, token)
          .then(function () {
            return loadData();
          })
          .then(function () {
            hideModal();
            addForm.reset();
          })
          .catch(function (xhr) {
            addError.textContent =
              'Could not save patient: ' +
              ajaxErrorMessage(xhr);
            addError.classList.remove('d-none');
          })
          .always(function () {
            submitButton.disabled = false;
          });
      });
    }

    function closeProfileDrawer() {
      profileRequestId++;
      drawerEl.classList.remove('show');
      drawerEl.setAttribute('aria-hidden', 'true');

      if (lastDrawerTrigger && lastDrawerTrigger.isConnected) {
        lastDrawerTrigger.focus();
      }
    }

    if (drawerCloseX) {
      drawerCloseX.addEventListener('click', closeProfileDrawer);
    }

    if (drawerBackBtn) {
      drawerBackBtn.addEventListener('click', function (event) {
        event.preventDefault();
        closeProfileDrawer();
      });
    }

    document.addEventListener('keydown', function (event) {
      if (event.key === 'Escape' &&
        drawerEl.classList.contains('show')) {
        event.preventDefault();
        closeProfileDrawer();
      }

      if (event.key === 'Tab' &&
        drawerEl.classList.contains('show')) {
        var focusables = Array.from(
          drawerEl.querySelectorAll(
            'a[href], button:not([disabled]), ' +
            'input:not([disabled]), select:not([disabled]), ' +
            '[tabindex]:not([tabindex="-1"])'
          )
        ).filter(function (item) {
          return item.getClientRects().length > 0;
        });

        if (!focusables.length) return;

        var first = focusables[0];
        var last = focusables[focusables.length - 1];

        if (event.shiftKey && document.activeElement === first) {
          event.preventDefault();
          last.focus();
        } else if (!event.shiftKey &&
          document.activeElement === last) {
          event.preventDefault();
          first.focus();
        }
      }
    });

    function renderWeeklyEngagement(weeklyData) {
      var container = document.getElementById(
        'drawer-weekly-engagement'
      );
      container.replaceChildren();

      var weeks = Array.isArray(weeklyData) ? weeklyData : [];

      function minutes(value) {
        var number = Number(value);
        return Number.isFinite(number) && number > 0
          ? number
          : 0;
      }

      var maxMinutes = Math.max(
        1,
        ...weeks.map(function (week) {
          return minutes(week.reflectXRMinutes) +
            minutes(week.reflectMobileMinutes);
        })
      );

      weeks.forEach(function (week) {
        var xrMinutes = minutes(week.reflectXRMinutes);
        var mobileMinutes = minutes(week.reflectMobileMinutes);
        var totalMinutes = xrMinutes + mobileMinutes;
        var weekLabel = formatWeek(week.weekStart);

        var column = element('div');
        column.className = 'patient-weekly-column';

        var totalLabel = element(
          'div',
          'patient-weekly-total',
          totalMinutes
        );

        var bars = element('div', 'patient-weekly-bars');
        var xrBar = element('div', 'patient-weekly-xr');
        var mobileBar = element('div', 'patient-weekly-mobile');

        xrBar.style.height =
          (xrMinutes / maxMinutes * 100) + '%';
        mobileBar.style.height =
          (mobileMinutes / maxMinutes * 100) + '%';

        bars.appendChild(xrBar);
        bars.appendChild(mobileBar);

        var dateLabel = element(
          'div',
          'patient-weekly-date',
          weekLabel
        );

        column.appendChild(totalLabel);
        column.appendChild(bars);
        column.appendChild(dateLabel);

        column.title =
          weekLabel + ': Reflect XR ' + xrMinutes +
          ' min, Reflect Mobile ' + mobileMinutes + ' min';

        column.setAttribute(
          'aria-label',
          column.title
        );

        container.appendChild(column);
      });
    }

    function renderSessionHistory(sessions, patientId) {
      var sessionsTbody = document.getElementById(
        'drawer-sessions-tbody'
      );
      sessionsTbody.replaceChildren();

      var rows = Array.isArray(sessions) ? sessions : [];

      if (rows.length === 0) {
        var emptyRow = element('tr');
        var emptyCell = element(
          'td',
          '',
          'No recorded sessions.'
        );
        emptyCell.colSpan = 5;
        emptyCell.className = 'patient-session-empty';
        emptyRow.appendChild(emptyCell);
        sessionsTbody.appendChild(emptyRow);
        return;
      }

      var fragment = document.createDocumentFragment();

      rows.forEach(function (session) {
        var tr = element('tr');

        function sessionCell(content) {
          var td = cell(tr, 'patient-session-cell', content);
          return td;
        }

        var link = element(
          'a',
          'fw-semibold text-decoration-none'
        );
        link.style.color = 'var(--primary-navy)';
        link.href =
          '/Provider/Patients/SessionDetails?sessionId=' +
          encodeURIComponent(session.sessionId || '') +
          '&patientId=' + encodeURIComponent(patientId);

        link.textContent = formatDate(session.startedAt) + ' ';
        link.title = session.startedAt
          ? formatDateTime(session.startedAt) + ' Eastern Time'
          : 'Session details';

        link.appendChild(icon('external-link', 'ms-1'));
        sessionCell(link);

        var badge = element(
          'span',
          'badge bg-light text-dark border',
          session.platform || '-'
        );
        badge.style.borderRadius = '0.25rem';
        sessionCell(badge);

        sessionCell(session.moduleName || '-');

        var duration = Number(session.durationMinutes);
        sessionCell(
          Number.isFinite(duration) && duration > 0
            ? duration + ' min'
            : '-'
        );

        var status = element(
          'span',
          'fw-semibold',
          session.recordingStatus || 'Succeeded'
        );
        status.classList.add('patient-session-status');
        sessionCell(status);

        fragment.appendChild(tr);
      });

      sessionsTbody.appendChild(fragment);
    }

    function showProfileError(loadingEl, message) {
      loadingEl.replaceChildren();
      var alert = element(
        'div',
        'alert alert-danger m-3',
        message
      );
      alert.setAttribute('role', 'alert');
      loadingEl.appendChild(alert);
      loadingEl.classList.remove('d-none');
    }

    function openProfileDrawer(patientId, patientName) {
      var requestId = ++profileRequestId;

      drawerEl.classList.add('show');
      drawerEl.setAttribute('aria-hidden', 'false');

      var titleEl = document.getElementById(
        'profile-drawer-patient-name'
      );
      var statusBadge = document.getElementById(
        'profile-drawer-status-badge'
      );
      var loadingEl = document.getElementById(
        'profile-drawer-loading'
      );
      var contentEl = document.getElementById(
        'profile-drawer-content'
      );

      var navNotes = document.getElementById('drawer-nav-notes');
      var navSession = document.getElementById('drawer-nav-session');

      if (navNotes) {
        navNotes.href =
          '/Provider/ClinicalResponse/IndexPage?patientId=' +
          encodeURIComponent(patientId);
      }

      if (navSession) {
        navSession.href = '/Provider/PatientSessionHistory/IndexPage';
        navSession.textContent = 'Session history';
      }

      titleEl.textContent = patientName || 'Patient Details';
      statusBadge.textContent = '';
      statusBadge.className = 'badge';

      loadingEl.replaceChildren();

      var spinner = element('div', 'spinner-border');
      spinner.setAttribute('role', 'status');

      var message = element(
        'p',
        'small mt-2',
        'Loading patient profile data…'
      );

      loadingEl.appendChild(spinner);
      loadingEl.appendChild(message);
      loadingEl.classList.remove('d-none');
      contentEl.classList.add('d-none');

      if (drawerCloseX) drawerCloseX.focus();

      service.getPatientProfile(profileBaseUrl, patientId)
        .then(function (data) {
          if (requestId !== profileRequestId) return;

          var needsReview = String(data.reviewStatus || '')
            .toLowerCase().includes('needs');

          statusBadge.textContent = needsReview
            ? 'NEEDS REVIEW'
            : 'REVIEWED';

          statusBadge.className = needsReview
            ? 'badge bg-warning text-dark'
            : 'badge bg-success';

          document.getElementById(
            'drawer-total-sessions'
          ).textContent = String(data.numOfSessions ?? 0);

          document.getElementById(
            'drawer-duration'
          ).textContent =
            String(data.totalDurationMinutes ?? 0) + ' min';

          document.getElementById(
            'drawer-last-active'
          ).textContent = formatDate(data.lastActive);

          document.getElementById(
            'drawer-last-module'
          ).textContent = data.mostUsedModule || 'Reflect Session';

          document.getElementById(
            'drawer-top-platform'
          ).textContent = data.mostUsedPlatform || 'Reflect XR';

          var xrCount = Number(data.reflectXRSessionCount) || 0;
          var mobileCount = Number(data.reflectMobileSessionCount) || 0;
          var total = xrCount + mobileCount || 1;

          document.getElementById(
            'drawer-xr-count'
          ).textContent = String(xrCount);

          document.getElementById(
            'drawer-mobile-count'
          ).textContent = String(mobileCount);

          document.getElementById(
            'drawer-progress-xr'
          ).style.width = (xrCount / total * 100) + '%';

          document.getElementById(
            'drawer-progress-mobile'
          ).style.width = (mobileCount / total * 100) + '%';

          renderWeeklyEngagement(data.weeklyEngagement);
          renderSessionHistory(data.sessions, patientId);
          if (navSession && Array.isArray(data.sessions) && data.sessions.length) {
            var latestSession = data.sessions[0];
            navSession.href =
              '/Provider/Patients/SessionDetails?sessionId=' +
              encodeURIComponent(latestSession.sessionId) +
              '&patientId=' + encodeURIComponent(patientId);
            navSession.textContent = 'Latest session';
          }


          loadingEl.classList.add('d-none');
          contentEl.classList.remove('d-none');
        })
        .catch(function (xhr) {
          if (requestId !== profileRequestId) return;

          showProfileError(
            loadingEl,
            'Could not load details: ' + ajaxErrorMessage(xhr)
          );
        });
    }


    drawerEl.setAttribute('aria-hidden', 'true');

    loadData()
      .then(function () {
        var requestedPatientId =
          new URLSearchParams(window.location.search).get('patientId');

        if (!requestedPatientId) return;

        var matchingPatient = allRows.find(function (row) {
          return String(row.patientId).toLowerCase() ===
            requestedPatientId.toLowerCase();
        });

        if (!matchingPatient) return;

        openProfileDrawer(
          matchingPatient.patientId,
          getDisplayName(matchingPatient)
        );
      })
      .catch(function () {
        // Error is already displayed in the patient table.
      });

  }

  window.PatientsIndexController = Object.freeze({
    init: init
  });

  function start() {
    if (document.getElementById('patients-config')) {
      window.PatientsIndexController.init();
    }
  }

  if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', start, { once: true });
  } else {
    start();
  }

})(window, document);

