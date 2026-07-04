window.DocumentStudio = (() => {
  function openTab(key) {
    document.querySelectorAll('[data-doc-studio-tab]').forEach(btn => {
      btn.classList.toggle('active', btn.dataset.docStudioTab === key);
    });
    document.querySelectorAll('[data-doc-studio-section]').forEach(section => {
      section.classList.toggle('active', section.dataset.docStudioSection === key);
    });
    try { localStorage.setItem('snapdocs:document-studio-tab', key); } catch (_) {}
  }
  function init() {
    document.querySelectorAll('[data-doc-studio-tab]').forEach(btn => {
      btn.addEventListener('click', () => openTab(btn.dataset.docStudioTab));
    });
    const saved = localStorage.getItem('snapdocs:document-studio-tab');
    if (saved && document.querySelector(`[data-doc-studio-tab="${saved}"]`)) openTab(saved);
  }
  document.addEventListener('DOMContentLoaded', init);
  return { openTab };
})();
