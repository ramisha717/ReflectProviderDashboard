// Temporary Provider demo shell: navigation, theme and accessibility controls.
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
