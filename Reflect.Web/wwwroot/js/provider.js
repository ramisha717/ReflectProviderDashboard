
// provider.js – shared interactivity for the Provider layout
(function () {
    'use strict';


    // 2. Debounce helper
    function debounce(func, wait) {
        let timeout;

        return function (...args) {
            const context = this;

            clearTimeout(timeout);

            timeout = setTimeout(function () {
                func.apply(context, args);
            }, wait);
        };
    }

    // 3. Header search event dispatch
    const headerSearch =
        document.querySelector('.header-search-input') ||
        document.querySelector('.patient-search');

    if (headerSearch) {
        const emitSearch = debounce(function () {
            document.dispatchEvent(
                new CustomEvent('provider:search', {
                    detail: { query: headerSearch.value }
                })
            );
        }, 200);

        headerSearch.addEventListener('input', emitSearch);
    }

    // 4. Mobile sidebar hamburger toggle and overlay
    const mobileMenuBtn = document.getElementById('mobileMenuBtn');
    const sidebar = document.getElementById('providerSidebar');
    const overlay = document.getElementById('mobileOverlay');

    if (mobileMenuBtn && sidebar) {
        mobileMenuBtn.addEventListener('click', function () {
            sidebar.classList.toggle('mobile-open');

            if (overlay) {
                overlay.classList.toggle('show');
            }
        });
    }

    if (overlay && sidebar) {
        overlay.addEventListener('click', function () {
            sidebar.classList.remove('mobile-open');
            overlay.classList.remove('show');
        });
    }

    // 5. Accessibility and theme controls
    const root = document.documentElement;

    const accessibilityToggle =
        document.getElementById('accessibilityToggle');

    const accessibilityPanel =
        document.getElementById('accessibilityPanel');

    const accessibilityTheme =
        document.getElementById('accessibilityTheme');

    const accessibilityColorblind =
        document.getElementById('accessibilityColorblind');

    const accessibilityTextSize =
        document.getElementById('accessibilityTextSize');

    function readPreference(key, fallback) {
        try {
            return localStorage.getItem(key) || fallback;
        } catch (error) {
            return fallback;
        }
    }

    function savePreference(key, value) {
        try {
            localStorage.setItem(key, value);
        } catch (error) {
            // Settings still work when browser storage is unavailable.
        }
    }

    // 6. Light/dark theme
    function setTheme(theme, notifyChart = true) {
        const normalizedTheme = theme === 'dark' ? 'dark' : 'light';

        root.setAttribute('data-theme-mode', normalizedTheme);
        savePreference('reflect-theme', normalizedTheme);

        if (accessibilityTheme) {
            accessibilityTheme.value = normalizedTheme;
        }

        // Notify the Overview chart to update its colors.
        if (notifyChart) {
            document.dispatchEvent(
                new CustomEvent('reflect:themechange', {
                    detail: { theme: normalizedTheme }
                })
            );
        }
    }

    // 7. Color-blind friendly mode
    function setColorblind(enabled) {
        root.setAttribute(
            'data-a11y-colorblind',
            enabled ? 'true' : 'false'
        );

        savePreference(
            'reflect-colorblind',
            enabled ? 'true' : 'false'
        );

        if (accessibilityColorblind) {
            accessibilityColorblind.checked = enabled;
        }
    }

    // 8. Text size
    function setTextSize(value) {
        const allowedSizes = ['100', '112.5', '125'];

        const normalizedSize = allowedSizes.includes(String(value))
            ? String(value)
            : '100';

        root.style.fontSize = normalizedSize + '%';

        savePreference('reflect-text-size', normalizedSize);

        if (accessibilityTextSize) {
            accessibilityTextSize.value = normalizedSize;
        }
    }

    // 9. Open or close accessibility panel
    function setPanelOpen(open) {
        if (!accessibilityPanel || !accessibilityToggle) return;

        accessibilityPanel.hidden = !open;

        accessibilityToggle.setAttribute(
            'aria-expanded',
            String(open)
        );
    }

    // 10. Restore saved accessibility preferences
    setTheme(
        readPreference('reflect-theme', 'light'),
        false
    );

    setColorblind(
        readPreference('reflect-colorblind', 'false') === 'true'
    );

    setTextSize(
        readPreference('reflect-text-size', '100')
    );

    // 11. Accessibility panel open/close behavior
    if (accessibilityToggle && accessibilityPanel) {
        accessibilityToggle.addEventListener('click', function () {
            setPanelOpen(accessibilityPanel.hidden);
        });

        // Close when clicking outside the panel.
        document.addEventListener('click', function (event) {
            if (
                !accessibilityPanel.hidden &&
                !accessibilityPanel.contains(event.target) &&
                !accessibilityToggle.contains(event.target)
            ) {
                setPanelOpen(false);
            }
        });

        // Close with Escape and return focus to the button.
        document.addEventListener('keydown', function (event) {
            if (
                event.key === 'Escape' &&
                !accessibilityPanel.hidden
            ) {
                setPanelOpen(false);
                accessibilityToggle.focus();
            }
        });
    }

    // 12. Accessibility panel controls
    if (accessibilityTheme) {
        accessibilityTheme.addEventListener('change', function () {
            setTheme(accessibilityTheme.value);
        });
    }

    if (accessibilityColorblind) {
        accessibilityColorblind.addEventListener('change', function () {
            setColorblind(accessibilityColorblind.checked);
        });
    }

    if (accessibilityTextSize) {
        accessibilityTextSize.addEventListener('change', function () {
            setTextSize(accessibilityTextSize.value);
        });
    }
})();


(function (window, document) {
    'use strict';

    function ensureToastContainer() {
        let container = document.getElementById('reflect-toast-container');

        if (!container) {
            container = document.createElement('div');
            container.id = 'reflect-toast-container';
            container.className = 'toast-container position-fixed top-0 end-0 p-3';
            container.style.zIndex = '2000';
            document.body.appendChild(container);
        }

        return container;
    }

    function showToastrMessage(message, messageType = 'info', timeOut = 3000) {
        if (!message) return;

        const types = {
            success: 'success',
            error: 'danger',
            danger: 'danger',
            warning: 'warning',
            info: 'info'
        };

        const type = types[messageType] || 'info';
        const container = ensureToastContainer();

        const element = document.createElement('div');
        element.className = 'toast border-0 text-bg-' + type;
        element.setAttribute('role', type === 'danger' ? 'alert' : 'status');
        element.setAttribute('aria-live', type === 'danger' ? 'assertive' : 'polite');

        const body = document.createElement('div');
        body.className = 'd-flex align-items-center';

        const text = document.createElement('div');
        text.className = 'toast-body flex-grow-1';
        text.textContent = message;

        const close = document.createElement('button');
        close.type = 'button';
        close.className = 'btn-close btn-close-white me-2 m-auto';
        close.setAttribute('data-bs-dismiss', 'toast');
        close.setAttribute('aria-label', 'Close');

        body.append(text, close);
        element.appendChild(body);
        container.appendChild(element);

        const toast = new bootstrap.Toast(element, {
            autohide: timeOut > 0,
            delay: timeOut > 0 ? timeOut : 3000
        });

        element.addEventListener('hidden.bs.toast', function () {
            element.remove();
        });

        toast.show();
    }

    function clearAllMessages() {
        document.querySelectorAll('#reflect-toast-container .toast')
            .forEach(function (element) {
                bootstrap.Toast.getInstance(element)?.hide();
            });
    }

    function showConfirmation(
        title,
        message,
        yesButtonText,
        noButtonText,
        onConfirm,
        onCancel,
        style
    ) {
        const wrapper = document.createElement('div');
        wrapper.className = 'modal fade';
        wrapper.tabIndex = -1;
        wrapper.setAttribute('aria-hidden', 'true');

        const dialog = document.createElement('div');
        dialog.className = 'modal-dialog modal-dialog-centered';

        const content = document.createElement('div');
        content.className = 'modal-content';

        const header = document.createElement('div');
        header.className = 'modal-header';

        const heading = document.createElement('h2');
        heading.className = 'modal-title fs-5';
        heading.textContent = title || 'Confirm action';

        const close = document.createElement('button');
        close.type = 'button';
        close.className = 'btn-close';
        close.setAttribute('data-bs-dismiss', 'modal');
        close.setAttribute('aria-label', 'Close');

        header.append(heading, close);

        const body = document.createElement('div');
        body.className = 'modal-body';
        body.innerHTML = message || '';

        const footer = document.createElement('div');
        footer.className = 'modal-footer';

        const cancelButton = document.createElement('button');
        cancelButton.type = 'button';
        cancelButton.className = 'btn btn-secondary';
        cancelButton.textContent = noButtonText || 'Cancel';

        const confirmButton = document.createElement('button');
        confirmButton.type = 'button';
        confirmButton.className =
            'btn btn-' + (style === 'delete' ? 'danger' : 'primary');
        confirmButton.textContent = yesButtonText || 'Confirm';

        footer.append(cancelButton, confirmButton);
        content.append(header, body, footer);
        dialog.appendChild(content);
        wrapper.appendChild(dialog);
        document.body.appendChild(wrapper);

        const modal = new bootstrap.Modal(wrapper);
        let confirmed = false;

        confirmButton.addEventListener('click', function () {
            confirmed = true;
            modal.hide();
        });

        cancelButton.addEventListener('click', function () {
            modal.hide();
        });

        wrapper.addEventListener('hidden.bs.modal', function () {
            modal.dispose();
            wrapper.remove();

            if (confirmed) {
                if (typeof onConfirm === 'function') onConfirm();
            } else {
                if (typeof onCancel === 'function') onCancel();
            }
        }, { once: true });

        modal.show();
    }

    window.MessageService = {
        showToastrMessage,
        clearAllMessages,
        cancelAction: function () { },

        showConfirmationDialogSuccess: function (
            title, message, yes, no, onConfirm, onCancel
        ) {
            showConfirmation(title, message, yes, no, onConfirm, onCancel, 'success');
        },

        showConfirmationDialogDelete: function (
            title, message, yes, no, onConfirm, onCancel
        ) {
            showConfirmation(title, message, yes, no, onConfirm, onCancel, 'delete');
        },

        showConfirmationDialogInfo: function (
            title, message, yes, no, onConfirm, onCancel
        ) {
            showConfirmation(title, message, yes, no, onConfirm, onCancel, 'info');
        }
    };

})(window, document);


(function (window, document) {
    'use strict';

    // Full-page loading overlay
    let loadingOverlay = null;

    window.displayLoading = function (message = 'Loading...') {
        if (!loadingOverlay) {
            loadingOverlay = document.createElement('div');
            loadingOverlay.id = 'reflect-loading-overlay';
            loadingOverlay.setAttribute('role', 'status');
            loadingOverlay.setAttribute('aria-live', 'polite');

            Object.assign(loadingOverlay.style, {
                position: 'fixed',
                inset: '0',
                zIndex: '2100',
                background: 'var(--bg-body, rgba(0,0,0,0.85))',
                opacity: '0.96',
                display: 'flex',
                alignItems: 'center',
                justifyContent: 'center'
            });

            const content = document.createElement('div');
            content.className = 'd-flex flex-column align-items-center gap-3';

            const spinner = document.createElement('div');
            spinner.className = 'spinner-border text-primary';
            spinner.setAttribute('aria-hidden', 'true');

            const label = document.createElement('span');
            label.id = 'reflect-loading-message';

            content.append(spinner, label);
            loadingOverlay.appendChild(content);
            document.body.appendChild(loadingOverlay);
        }

        loadingOverlay.querySelector('#reflect-loading-message').textContent =
            message || 'Loading...';

        loadingOverlay.hidden = false;
        loadingOverlay.style.display = 'flex';
    };

    window.hideDisplayLoading = function () {
        if (loadingOverlay) {
            loadingOverlay.hidden = true;
            loadingOverlay.style.display = 'none';
        }
    };

    // Safe HTML escaping for table badge content
    function escapeHtml(value) {
        return String(value ?? '').replace(/[&<>"']/g, function (character) {
            return {
                '&': '&amp;',
                '<': '&lt;',
                '>': '&gt;',
                '"': '&quot;',
                "'": '&#39;'
            }[character];
        });
    }

    window.renderBadge = function (color, text, iconClass) {
        const safeColor = /^[a-z0-9-]+$/i.test(color || '')
            ? color
            : 'secondary';

        const safeIcon = String(iconClass || '')
            .split(/\s+/)
            .filter(function (name) {
                return /^[a-z0-9-]+$/i.test(name);
            })
            .join(' ');

        return '<span class="badge bg-' + safeColor +
            '-transparent text-' + safeColor + '">' +
            (safeIcon
                ? '<i class="' + safeIcon + ' me-1" aria-hidden="true"></i>'
                : '') +
            escapeHtml(text) +
            '</span>';
    };

    // Adjust DataTables when a hidden tab becomes visible
    window.initTabDataTableFix = function (selector) {
        const container = document.querySelector(selector);
        if (!container) return;

        container.addEventListener('shown.bs.tab', function () {
            if (!window.jQuery || !window.jQuery.fn.DataTable) return;

            window.jQuery(container)
                .find('table')
                .each(function () {
                    if (!window.jQuery.fn.DataTable.isDataTable(this)) return;

                    const table = window.jQuery(this).DataTable();
                    table.columns.adjust();

                    if (table.responsive) {
                        table.responsive.recalc();
                    }
                });
        });
    };

    // Remember the active tab across reloads
    window.initTabPersistence = function (containerId) {
        const id = String(containerId).replace(/^#/, '');
        const container = document.getElementById(id);
        if (!container) return;

        const key = 'reflect-active-tab-' + id;
        let savedId = null;

        try {
            savedId = window.sessionStorage.getItem(key);
        } catch (_) {
            // Storage may be unavailable.
        }

        if (savedId) {
            const savedTab = Array.from(
                container.querySelectorAll('[data-bs-toggle="tab"]')
            ).find(function (tab) {
                return tab.id === savedId;
            });

            if (savedTab && window.bootstrap?.Tab) {
                window.bootstrap.Tab.getOrCreateInstance(savedTab).show();
            }
        }

        container.addEventListener('shown.bs.tab', function (event) {
            if (!event.target.id) return;

            try {
                window.sessionStorage.setItem(key, event.target.id);
            } catch (_) {
                // Storage may be unavailable.
            }
        });
    };

    function initializeSharedBehaviors() {
        // Confirm-before-submit
        document.querySelectorAll('.js-confirm-submit').forEach(function (button) {
            button.addEventListener('click', function () {
                const form = button.closest('form');
                if (!form || !window.MessageService) return;

                const title = button.dataset.confirmTitle || 'Confirm action';
                const message = button.dataset.confirmText || 'Continue?';
                const label = button.dataset.confirmLabel || 'Confirm';
                const style = button.dataset.confirmStyle || 'delete';

                const methods = {
                    delete: 'showConfirmationDialogDelete',
                    success: 'showConfirmationDialogSuccess',
                    info: 'showConfirmationDialogInfo'
                };

                const method = methods[style] || methods.delete;

                window.MessageService[method](
                    title,
                    message,
                    label,
                    'Cancel',
                    function () {
                        if (form.requestSubmit) {
                            form.requestSubmit();
                        } else {
                            form.submit();
                        }
                    },
                    window.MessageService.cancelAction
                );
            });
        });

        // Required-field indicators
        document.querySelectorAll(
            'input[required], select[required], textarea[required], ' +
            'input[data-val-required], select[data-val-required], ' +
            'textarea[data-val-required]'
        ).forEach(function (field) {
            if (!field.id) return;

            const label = Array.from(document.querySelectorAll('label[for]'))
                .find(function (item) {
                    return item.htmlFor === field.id;
                });

            if (!label || label.querySelector('.reflect-required-star')) return;

            const star = document.createElement('span');
            star.className = 'reflect-required-star text-danger ms-1';
            star.textContent = '*';
            star.setAttribute('aria-hidden', 'true');
            label.appendChild(star);
        });

        // Client-side DataTables, when the library is available
        if (window.jQuery && window.jQuery.fn.DataTable) {
            window.jQuery('table.dt-client').each(function () {
                if (window.jQuery.fn.DataTable.isDataTable(this)) return;

                const lastHeader = this.querySelector('thead th:last-child');
                const lastIsActions = lastHeader &&
                    lastHeader.textContent.trim().toLowerCase() === 'actions';

                const options = {
                    responsive: true
                };

                if (lastIsActions) {
                    options.columnDefs = [{
                        targets: -1,
                        orderable: false
                    }];
                }

                window.jQuery(this).DataTable(options);
            });
        }

        // Clickable cards
        document.querySelectorAll('.card-clickable[data-href]')
            .forEach(function (card) {
                card.addEventListener('click', function (event) {
                    if (event.target.closest('a, button, input, select, textarea')) {
                        return;
                    }

                    const destination = card.dataset.href;
                    if (!destination) return;

                    const url = new URL(destination, window.location.href);

                    if (url.origin === window.location.origin) {
                        window.location.assign(url.href);
                    }
                });
            });

        // Automatically submit a form when a select changes
        document.querySelectorAll('select.js-auto-submit-select')
            .forEach(function (select) {
                select.addEventListener('change', function () {
                    const form = select.closest('form');
                    if (form) {
                        if (form.requestSubmit) {
                            form.requestSubmit();
                        } else {
                            form.submit();
                        }
                    }
                });
            });

        // Prevent click bubbling
        document.querySelectorAll('.js-stop-propagation')
            .forEach(function (element) {
                element.addEventListener('click', function (event) {
                    event.stopPropagation();
                });
            });
    }

    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', initializeSharedBehaviors);
    } else {
        initializeSharedBehaviors();
    }

})(window, document);


(function (window) {
    'use strict';

    const timeZone = 'America/New_York';

    const dateFormatter = new Intl.DateTimeFormat('en-US', {
        timeZone: timeZone,
        month: 'numeric',
        day: 'numeric',
        year: 'numeric'
    });


    const timeFormatter = new Intl.DateTimeFormat('en-US', {
        timeZone: timeZone,
        hour: 'numeric',
        minute: '2-digit',
        hour12: true
    });

    const chartDateFormatter = new Intl.DateTimeFormat('en-US', {
        timeZone: timeZone,
        month: 'short',
        day: 'numeric'
    });

    function parseDate(value) {
        if (value === null || value === undefined || value === '') {
            return null;
        }

        const date = value instanceof Date ? value : new Date(value);

        return Number.isNaN(date.getTime()) ? null : date;
    }

    function format(value, formatter, fallback) {
        const date = parseDate(value);
        return date ? formatter.format(date) : fallback;
    }

    window.ReflectDate = Object.freeze({
        toUsDateString: function (value, fallback = '—') {
            return format(value, dateFormatter, fallback);
        },

        toUsDateTimeString: function (value, fallback = '—') {
            const date = parseDate(value);
            if (!date) return fallback;

            return dateFormatter.format(date) + ' ' +
                timeFormatter.format(date);
        },

        toUsTimeString: function (value, fallback = '—') {
            return format(value, timeFormatter, fallback);
        },

        toChartDateString: function (value, fallback = '—') {
            return format(value, chartDateFormatter, fallback);
        }
    });

})(window);






