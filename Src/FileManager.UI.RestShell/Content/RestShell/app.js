const apiBase = window.location.pathname.replace(/\/?$/, '/');
let currentPath = '';
let canWrite = false;
let entries = [];

const i18n = {
  en: { up: 'Up', mkdir: 'New Folder', upload: 'Upload', refresh: 'Refresh', name: 'Name', size: 'Size', modified: 'Modified (UTC)', actions: 'Actions' },
  zh: { up: '上级', mkdir: '新建文件夹', upload: '上传', refresh: '刷新', name: '名称', size: '大小', modified: '修改时间 (UTC)', actions: '操作' }
};

function formatSize(bytes) {
  if (bytes < 1024) return bytes + ' B';
  if (bytes < 1024 * 1024) return (bytes / 1024).toFixed(1) + ' KB';
  return (bytes / (1024 * 1024)).toFixed(1) + ' MB';
}

async function api(path, options) {
  const response = await fetch(apiBase + path, options);
  if (!response.ok) throw new Error(await response.text());
  const type = response.headers.get('content-type') || '';
  return type.includes('application/json') ? response.json() : response;
}

function applyLang(lang) {
  document.querySelectorAll('[data-i18n]').forEach(el => {
    const key = el.getAttribute('data-i18n');
    if (!i18n[lang] || !i18n[lang][key]) return;
    if (el.tagName === 'LABEL') el.childNodes[0].textContent = i18n[lang][key] + ' ';
    else el.textContent = i18n[lang][key];
  });
}

function renderBreadcrumb() {
  const el = document.getElementById('breadcrumb');
  if (!el) return;
  let html = '<a href="#" data-path="">Root</a>';
  if (!currentPath) { el.innerHTML = html; bindBreadcrumb(el); return; }
  let built = '';
  currentPath.split('/').forEach(name => {
    built = built ? built + '/' + name : name;
    html += ' / <a href="#" data-path="' + built + '">' + name + '</a>';
  });
  el.innerHTML = html;
  bindBreadcrumb(el);
}

function bindBreadcrumb(el) {
  el.querySelectorAll('a').forEach(a => a.onclick = e => { e.preventDefault(); load(a.dataset.path); });
}

function renderEntries() {
  const tbody = document.querySelector('#file-table tbody');
  if (!tbody) return;
  tbody.innerHTML = entries.map(entry => {
    const actions = canWrite ? '<button data-act="rename" data-path="' + entry.path + '" data-name="' + entry.name + '">Rename</button> <button data-act="delete" data-path="' + entry.path + '" data-name="' + entry.name + '">Delete</button>' : '';
    const label = entry.isDirectory ? entry.name + '/' : entry.name;
    return '<tr><td><a href="#" data-open="' + entry.path + '" data-dir="' + entry.isDirectory + '">' + label + '</a></td><td>' + (entry.isDirectory ? '-' : formatSize(entry.length)) + '</td><td>' + entry.modifiedUtc + '</td><td>' + actions + '</td></tr>';
  }).join('');
  tbody.querySelectorAll('[data-open]').forEach(a => a.onclick = e => { e.preventDefault(); openEntry(a.dataset.open, a.dataset.dir === 'true'); });
  tbody.querySelectorAll('[data-act=delete]').forEach(b => b.onclick = () => removeEntry(b.dataset.path, b.dataset.name));
  tbody.querySelectorAll('[data-act=rename]').forEach(b => b.onclick = () => renameEntry(b.dataset.path, b.dataset.name));
}

async function load(path) {
  const data = await api('api/entries?path=' + encodeURIComponent(path || ''));
  currentPath = data.path || '';
  canWrite = !!data.canWrite;
  entries = data.entries || [];
  renderBreadcrumb();
  renderEntries();
  document.querySelectorAll('.toolbar').forEach(t => t.style.display = canWrite ? '' : 'none');
}

function openEntry(path, isDirectory) {
  if (isDirectory) load(path);
  else window.open(apiBase + 'api/download?path=' + encodeURIComponent(path), '_blank');
}

async function goUp() {
  if (!currentPath) return;
  const parts = currentPath.split('/');
  parts.pop();
  await load(parts.join('/'));
}

async function mkdir() {
  const name = prompt('Folder name');
  if (!name) return;
  const path = currentPath ? currentPath + '/' + name : name;
  await api('api/mkdir?path=' + encodeURIComponent(path), { method: 'POST' });
  await load(currentPath);
}

async function uploadFiles(fileList) {
  for (const file of fileList) {
    const form = new FormData();
    form.append('file', file);
    await fetch(apiBase + 'api/upload?path=' + encodeURIComponent(currentPath || ''), { method: 'POST', body: form });
  }
  await load(currentPath);
}

async function removeEntry(path, name) {
  if (!confirm('Delete ' + name + '?')) return;
  await api('api/delete?path=' + encodeURIComponent(path), { method: 'POST' });
  await load(currentPath);
}

async function renameEntry(path, name) {
  const newName = prompt('New name', name);
  if (!newName || newName === name) return;
  await api('api/rename?path=' + encodeURIComponent(path) + '&name=' + encodeURIComponent(newName), { method: 'POST' });
  await load(currentPath);
}

function wireShellFeatures(features) {
  const set = new Set(features || []);
  if (set.has('dropzone')) document.getElementById('dropzone')?.removeAttribute('hidden');
  if (set.has('sdk-hint')) document.getElementById('sdk-hint')?.removeAttribute('hidden');
  if (set.has('dark-toggle')) document.getElementById('btn-theme')?.removeAttribute('hidden');
  if (set.has('i18n')) document.getElementById('lang-select')?.removeAttribute('hidden');
  if (set.has('bootstrap')) {
    const link = document.getElementById('bootstrap-css');
    if (link) {
      link.href = 'https://cdn.jsdelivr.net/npm/bootstrap@5.3.3/dist/css/bootstrap.min.css';
      link.disabled = false;
      document.getElementById('file-table')?.classList.add('table-hover');
    }
  }
  if (set.has('media')) document.body.classList.add('media-layout');
  if (set.has('mobile')) document.body.classList.add('mobile-layout');
}

document.addEventListener('DOMContentLoaded', async () => {
  try {
    const cfg = await fetch(apiBase + 'shell-config.json').then(r => r.json());
    document.documentElement.style.setProperty('--fm-accent', cfg.accentColor || '#2563eb');
    const title = document.querySelector('header h1');
    if (title && cfg.title) title.textContent = cfg.title;
    document.title = cfg.title || 'File Manager';
    wireShellFeatures(cfg.features);
  } catch { /* shell-config optional */ }

  document.getElementById('btn-up')?.addEventListener('click', goUp);
  document.getElementById('btn-mkdir')?.addEventListener('click', mkdir);
  document.getElementById('btn-refresh')?.addEventListener('click', () => load(currentPath));
  document.getElementById('file-input')?.addEventListener('change', e => { uploadFiles(e.target.files); e.target.value = ''; });
  document.getElementById('btn-theme')?.addEventListener('click', () => document.body.classList.toggle('dark'));
  document.getElementById('lang-select')?.addEventListener('change', e => applyLang(e.target.value));

  const drop = document.getElementById('dropzone');
  if (drop && !drop.hidden) {
    ['dragenter', 'dragover'].forEach(ev => drop.addEventListener(ev, e => { e.preventDefault(); drop.classList.add('hover'); }));
    ['dragleave', 'drop'].forEach(ev => drop.addEventListener(ev, e => { e.preventDefault(); drop.classList.remove('hover'); }));
    drop.addEventListener('drop', e => uploadFiles(e.dataTransfer.files));
  }

  applyLang('en');
  load('');
});
