(function () {
  let deferredInstallPrompt = null;

  function createInstallBanner() {
    if (document.querySelector('[data-pwa-install-banner]')) return;

    const banner = document.createElement('div');
    banner.className = 'snap-pwa-install no-print';
    banner.setAttribute('data-pwa-install-banner', 'true');
    banner.innerHTML = `
      <div>
        <strong>ثبّت SnapDocs كتطبيق</strong>
        <span>استخدمه من شاشة الموبايل أو سطح المكتب بسرعة.</span>
      </div>
      <div class="snap-pwa-actions">
        <button class="snap-btn snap-btn-primary" type="button" data-pwa-install>تثبيت</button>
        <button class="snap-icon-btn" type="button" data-pwa-dismiss>×</button>
      </div>`;
    document.body.appendChild(banner);
  }

  window.addEventListener('beforeinstallprompt', (event) => {
    event.preventDefault();
    deferredInstallPrompt = event;
    if (localStorage.getItem('snap.pwa.install.dismissed') !== '1') createInstallBanner();
  });

  document.addEventListener('click', async (event) => {
    if (event.target.matches('[data-pwa-dismiss]')) {
      localStorage.setItem('snap.pwa.install.dismissed', '1');
      document.querySelector('[data-pwa-install-banner]')?.remove();
    }

    if (event.target.matches('[data-pwa-install]') && deferredInstallPrompt) {
      deferredInstallPrompt.prompt();
      await deferredInstallPrompt.userChoice;
      deferredInstallPrompt = null;
      document.querySelector('[data-pwa-install-banner]')?.remove();
    }
  });

  window.addEventListener('appinstalled', () => {
    deferredInstallPrompt = null;
    localStorage.setItem('snap.pwa.installed', '1');
    window.SnapUI?.toast('تم تثبيت SnapDocs بنجاح');
  });
})();
