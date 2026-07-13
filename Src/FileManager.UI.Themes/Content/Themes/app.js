const apiBase = window.location.pathname.replace(/\/?$/, '/');
let currentPath = '';
let canWrite = true;
let entries = [];

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

function renderBreadcrumb() {
  const el = document.getElementById('breadcrumb');
  if (!el) return;
  let html = '<a href="#" data-path="">Root</a>';
  if (!currentPath) { el.innerHTML = html; return; }
  let built = '';
  currentPath.split('/').forEach(name => {
    built = built ? built + '/' + name : name;
    html += ' / <a href="#" data-path="' + built + '">' + name + '</a>';
  });
  el.innerHTML = html;
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

document.addEventListener('DOMContentLoaded', () => {
  document.getElementById('btn-up')?.addEventListener('click', goUp);
  document.getElementById('btn-mkdir')?.addEventListener('click', mkdir);
  document.getElementById('btn-refresh')?.addEventListener('click', () => load(currentPath));
  document.getElementById('file-input')?.addEventListener('change', e => { uploadFiles(e.target.files); e.target.value = ''; });
  const drop = document.getElementById('dropzone');
  if (drop) {
    ['dragenter','dragover'].forEach(ev => drop.addEventListener(ev, e => { e.preventDefault(); drop.classList.add('hover'); }));
    ['dragleave','drop'].forEach(ev => drop.addEventListener(ev, e => { e.preventDefault(); drop.classList.remove('hover'); }));
    drop.addEventListener('drop', e => uploadFiles(e.dataTransfer.files));
  }
  load('');
});


document.getElementById('btn-theme')?.addEventListener('click', () => document.body.classList.toggle('dark'));

