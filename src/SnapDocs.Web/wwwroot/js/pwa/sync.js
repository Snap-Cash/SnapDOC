(function () {
  const QUEUE_KEY = 'snap.offline.queue';

  function readQueue() {
    try { return JSON.parse(localStorage.getItem(QUEUE_KEY) || '[]'); }
    catch { return []; }
  }

  function writeQueue(queue) {
    localStorage.setItem(QUEUE_KEY, JSON.stringify(queue));
    updateQueueBadge();
  }

  function updateConnectionBadge() {
    let badge = document.querySelector('[data-connection-status]');
    if (!badge) {
      badge = document.createElement('div');
      badge.className = 'snap-connection-status no-print';
      badge.setAttribute('data-connection-status', 'true');
      document.body.appendChild(badge);
    }

    const online = navigator.onLine;
    badge.classList.toggle('offline', !online);
    badge.innerHTML = online ? '🟢 Online' : '🔴 Offline';
    if (!online) window.SnapUI?.toast('أنت تعمل بدون إنترنت');
  }

  function updateQueueBadge() {
    const count = readQueue().length;
    let badge = document.querySelector('[data-sync-queue]');
    if (!badge) {
      badge = document.createElement('button');
      badge.className = 'snap-sync-queue no-print';
      badge.type = 'button';
      badge.setAttribute('data-sync-queue', 'true');
      document.body.appendChild(badge);
      badge.addEventListener('click', () => window.SnapUI?.toast(count ? `${count} عملية في انتظار المزامنة` : 'لا توجد عمليات معلقة'));
    }
    badge.style.display = count ? 'inline-flex' : 'none';
    badge.textContent = `⏳ ${count}`;
  }

  window.SnapOffline = {
    enqueue(type, payload) {
      const queue = readQueue();
      queue.push({ id: crypto.randomUUID?.() || String(Date.now()), type, payload, createdAt: new Date().toISOString() });
      writeQueue(queue);
      window.SnapUI?.toast('تم حفظ العملية في قائمة المزامنة');
    },
    getQueue: readQueue,
    clearQueue() { writeQueue([]); },
    async trySync() {
      const queue = readQueue();
      if (!navigator.onLine || !queue.length) return;
      // Starter sync hook: real API sync will be implemented when API endpoints are added.
      console.info('SnapDocs offline queue ready for API sync:', queue);
      window.SnapUI?.toast('قائمة المزامنة جاهزة للربط مع API');
    }
  };

  window.addEventListener('online', () => { updateConnectionBadge(); window.SnapOffline.trySync(); });
  window.addEventListener('offline', updateConnectionBadge);
  document.addEventListener('DOMContentLoaded', () => { updateConnectionBadge(); updateQueueBadge(); });
})();
