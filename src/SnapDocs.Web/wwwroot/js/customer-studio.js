(function () {
    const tabButtons = document.querySelectorAll('[data-tabs="customerStudioTabs"] [data-tab-target]');
    const panels = document.querySelectorAll('[data-tab-panel]');

    if (!tabButtons.length || !panels.length) return;

    function activate(tabName) {
        tabButtons.forEach(btn => btn.classList.toggle('active', btn.dataset.tabTarget === tabName));
        panels.forEach(panel => panel.classList.toggle('active', panel.dataset.tabPanel === tabName));
        if (history.replaceState) {
            history.replaceState(null, '', '#' + tabName);
        }
    }

    tabButtons.forEach(btn => btn.addEventListener('click', () => activate(btn.dataset.tabTarget)));

    const initial = (location.hash || '').replace('#', '');
    if (initial && document.querySelector(`[data-tab-target="${initial}"]`)) {
        activate(initial);
    }
})();
