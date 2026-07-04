(function () {
  function setOnlineStatus() {
    const status = document.getElementById('snapOnlineStatus');
    if (!status) return;
    if (navigator.onLine) {
      status.textContent = '🟢 Online';
      status.classList.remove('offline');
    } else {
      status.textContent = '🔴 Offline';
      status.classList.add('offline');
    }
  }

  function enhanceTables() {
    document.querySelectorAll('table.snap-table, table.responsive-table').forEach(table => {
      if (table.closest('.snap-table-enhanced')) return;
      const wrapper = document.createElement('div');
      wrapper.className = 'snap-table-enhanced';
      table.parentNode.insertBefore(wrapper, table);
      wrapper.appendChild(table);
    });
  }

  function bindSidebarClose() {
    document.querySelectorAll('[data-sidebar-close]').forEach(btn => {
      btn.addEventListener('click', () => document.getElementById('snapSidebar')?.classList.remove('open'));
    });
  }

  window.SnapUX = {
    emptyState(targetSelector, icon, title, message) {
      const target = document.querySelector(targetSelector);
      if (!target) return;
      target.innerHTML = `<div class="snap-empty-state-v2"><span>${icon || '📭'}</span><strong>${title || 'لا توجد بيانات'}</strong><small>${message || 'ابدأ بإضافة أول عنصر.'}</small></div>`;
    },
    showSkeleton(targetSelector, lines) {
      const target = document.querySelector(targetSelector);
      if (!target) return;
      target.innerHTML = `<div class="snap-loading-skeleton">${Array.from({ length: lines || 4 }).map(() => '<i></i>').join('')}</div>`;
    },
    confirm(title, message) {
      const dialog = document.getElementById('snapConfirmDialog');
      if (!dialog) return;
      document.getElementById('snapDialogTitle').textContent = title || 'تأكيد العملية';
      document.getElementById('snapDialogMessage').textContent = message || 'هل تريد المتابعة؟';
      dialog.classList.add('open');
    }
  };

  if (window.SnapUI) {
    window.SnapUI.closeDialog = function (id) { document.getElementById(id)?.classList.remove('open'); };
    window.SnapUI.openDialog = function (id) { document.getElementById(id)?.classList.add('open'); };
  }

  window.addEventListener('online', setOnlineStatus);
  window.addEventListener('offline', setOnlineStatus);
  document.addEventListener('DOMContentLoaded', function () {
    setOnlineStatus();
    enhanceTables();
    bindSidebarClose();
  });
})();
