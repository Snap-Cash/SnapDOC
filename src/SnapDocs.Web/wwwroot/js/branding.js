(() => {
  const page = document.querySelector('[data-branding-page]');
  if (!page) return;

  const root = document.documentElement;
  const companyInput = document.querySelector('[data-preview="company"]');
  const themeSelect = document.querySelector('[data-preview="theme"]');
  const fontSelect = document.querySelector('[data-preview="font"]');
  const radiusSelect = document.querySelector('[data-preview="radius"]');
  const darkInput = document.querySelector('[data-preview="dark"]');

  document.querySelectorAll('[data-css-var]').forEach(input => {
    const apply = () => root.style.setProperty(input.dataset.cssVar, input.value);
    input.addEventListener('input', apply);
    apply();
  });

  companyInput?.addEventListener('input', () => {
    document.querySelector('[data-preview-company]').textContent = companyInput.value || 'SnapDocs';
  });

  themeSelect?.addEventListener('change', () => {
    document.querySelector('[data-preview-theme]').textContent = themeSelect.value;
    document.body.className = document.body.className.replace(/snap-theme-\w+/g, '').trim() + ' snap-theme-' + themeSelect.value.toLowerCase();
  });

  fontSelect?.addEventListener('change', () => {
    root.style.setProperty('--snap-font', `'${fontSelect.value}', 'Tahoma', system-ui, sans-serif`);
  });

  radiusSelect?.addEventListener('change', () => {
    root.style.setProperty('--snap-radius', radiusSelect.value);
    root.style.setProperty('--snap-radius-dynamic', radiusSelect.value);
  });

  darkInput?.addEventListener('change', () => {
    root.dataset.theme = darkInput.checked ? 'dark' : 'light';
  });
})();
