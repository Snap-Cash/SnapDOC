const SNAPDOCS_CACHE_VERSION = 'snapdocs-sprint11-v1';
const SNAPDOCS_RUNTIME_CACHE = 'snapdocs-runtime-v1';
const SNAPDOCS_STATIC_ASSETS = [
  '/',
  '/offline.html',
  '/manifest.json',
  '/css/snapui.css',
  '/css/print-engine.css',
  '/js/app.js',
  '/js/document-lines.js',
  '/js/pwa/install.js',
  '/js/pwa/sync.js',
  '/js/pwa/update.js',
  '/icons/icon-192.svg',
  '/icons/icon-512.svg'
];

self.addEventListener('install', event => {
  event.waitUntil(
    caches.open(SNAPDOCS_CACHE_VERSION)
      .then(cache => cache.addAll(SNAPDOCS_STATIC_ASSETS))
      .then(() => self.skipWaiting())
  );
});

self.addEventListener('activate', event => {
  event.waitUntil(
    caches.keys().then(keys => Promise.all(
      keys
        .filter(key => ![SNAPDOCS_CACHE_VERSION, SNAPDOCS_RUNTIME_CACHE].includes(key))
        .map(key => caches.delete(key))
    )).then(() => self.clients.claim())
  );
});

self.addEventListener('message', event => {
  if (!event.data) return;
  if (event.data.type === 'SKIP_WAITING') self.skipWaiting();
  if (event.data.type === 'CACHE_URLS' && Array.isArray(event.data.urls)) {
    event.waitUntil(caches.open(SNAPDOCS_RUNTIME_CACHE).then(cache => cache.addAll(event.data.urls)));
  }
});

self.addEventListener('fetch', event => {
  const request = event.request;
  if (request.method !== 'GET') return;

  const url = new URL(request.url);
  if (url.origin !== location.origin) return;

  if (request.mode === 'navigate') {
    event.respondWith(networkFirstPage(request));
    return;
  }

  if (isStaticAsset(url.pathname)) {
    event.respondWith(cacheFirst(request));
    return;
  }

  event.respondWith(staleWhileRevalidate(request));
});

async function networkFirstPage(request) {
  try {
    const response = await fetch(request);
    const cache = await caches.open(SNAPDOCS_RUNTIME_CACHE);
    cache.put(request, response.clone());
    return response;
  } catch (error) {
    const cached = await caches.match(request);
    return cached || caches.match('/offline.html');
  }
}

async function cacheFirst(request) {
  const cached = await caches.match(request);
  if (cached) return cached;
  const response = await fetch(request);
  const cache = await caches.open(SNAPDOCS_CACHE_VERSION);
  cache.put(request, response.clone());
  return response;
}

async function staleWhileRevalidate(request) {
  const cache = await caches.open(SNAPDOCS_RUNTIME_CACHE);
  const cached = await cache.match(request);
  const fetched = fetch(request).then(response => {
    if (response && response.ok) cache.put(request, response.clone());
    return response;
  }).catch(() => null);
  return cached || fetched || caches.match('/offline.html');
}

function isStaticAsset(pathname) {
  return pathname.startsWith('/css/') ||
    pathname.startsWith('/js/') ||
    pathname.startsWith('/icons/') ||
    pathname.endsWith('.svg') ||
    pathname.endsWith('.png') ||
    pathname.endsWith('.jpg') ||
    pathname.endsWith('.webp') ||
    pathname.endsWith('.woff2');
}
