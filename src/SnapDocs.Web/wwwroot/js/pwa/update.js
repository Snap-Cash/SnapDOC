(function () {
  if (!('serviceWorker' in navigator)) return;

  function showUpdateBanner(registration) {
    if (document.querySelector('[data-pwa-update-banner]')) return;
    const banner = document.createElement('div');
    banner.className = 'snap-pwa-update no-print';
    banner.setAttribute('data-pwa-update-banner', 'true');
    banner.innerHTML = `
      <strong>يتوفر تحديث جديد</strong>
      <span>اضغط تحديث لتحميل أحدث نسخة من SnapDocs.</span>
      <button class="snap-btn snap-btn-primary" type="button" data-pwa-update>تحديث</button>`;
    document.body.appendChild(banner);

    banner.querySelector('[data-pwa-update]').addEventListener('click', () => {
      registration.waiting?.postMessage({ type: 'SKIP_WAITING' });
    });
  }

  navigator.serviceWorker.register('/service-worker.js').then(registration => {
    if (registration.waiting) showUpdateBanner(registration);

    registration.addEventListener('updatefound', () => {
      const newWorker = registration.installing;
      if (!newWorker) return;
      newWorker.addEventListener('statechange', () => {
        if (newWorker.state === 'installed' && navigator.serviceWorker.controller) {
          showUpdateBanner(registration);
        }
      });
    });
  }).catch(() => {});

  let refreshing = false;
  navigator.serviceWorker.addEventListener('controllerchange', () => {
    if (refreshing) return;
    refreshing = true;
    window.location.reload();
  });
})();
