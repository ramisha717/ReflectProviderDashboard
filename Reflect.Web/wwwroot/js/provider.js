// provider.js – shared interactivity for the Provider layout
(function () {
    // 1. Highlight active navigation link based on current path
    var path = window.location.pathname.toLowerCase();
    var links = document.querySelectorAll('.provider-sidebar .nav-link');
    links.forEach(function (link) {
        var href = (link.getAttribute('href') || '').toLowerCase();
        if (href && (path === href || (href !== '/' && path.startsWith(href)))) {
            link.classList.add('active');
        } else if (href && path.includes('patients') && href.includes('patients')) {
            link.classList.add('active');
        }
    });

    // 2. Debounce helper
    function debounce(func, wait) {
        var timeout;
        return function () {
            var context = this, args = arguments;
            clearTimeout(timeout);
            timeout = setTimeout(function () { func.apply(context, args); }, wait);
        };
    }

    // 3. Search inputs synchronization & event dispatch
    var headerSearch = document.querySelector('.header-search-input') || document.querySelector('.patient-search');
    if (headerSearch) {
        var emitSearch = debounce(function () {
            var event = new CustomEvent('provider:search', { detail: { query: headerSearch.value } });
            document.dispatchEvent(event);
        }, 200);
        headerSearch.addEventListener('input', emitSearch);
    }
})();
