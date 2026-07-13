(function () {
  const base = window.location.pathname.replace(/\/?$/, '/');
  const params = new URLSearchParams(window.location.search);
  const multiple = params.get('multiple') !== '0';
  let currentPath = '';
  const selected = new Map();

  async function api(path) {
    const response = await fetch(base + path);
    if (!response.ok) throw new Error(await response.text());
    return response.json();
  }

  function formatSize(bytes) {
    if (bytes < 1024) return bytes + ' B';
    if (bytes < 1024 * 1024) return (bytes / 1024).toFixed(1) + ' KB';
    return (bytes / (1024 * 1024)).toFixed(1) + ' MB';
  }

  function updateSelectButton() {
    document.getElementById('btn-select').disabled = selected.size === 0;
  }

  function renderBreadcrumb() {
    const parts = currentPath ? currentPath.split('/') : [];
    let html = '<a href="#" data-path="">Root</a>';
    let built = '';
    parts.forEach(function (part) {
      built = built ? built + '/' + part : part;
      html += ' / <a href="#" data-path="' + built + '">' + part + '</a>';
    });
    const el = document.getElementById('breadcrumb');
    el.innerHTML = html;
    el.querySelectorAll('a').forEach(function (link) {
      link.addEventListener('click', function (e) {
        e.preventDefault();
        load(link.getAttribute('data-path') || '');
      });
    });
  }

  function renderEntries(data) {
    currentPath = data.path || '';
    renderBreadcrumb();
    const tbody = document.getElementById('entries');
    tbody.innerHTML = '';
    (data.entries || []).forEach(function (entry) {
      const row = document.createElement('tr');
      const checkCell = document.createElement('td');
      const nameCell = document.createElement('td');
      const sizeCell = document.createElement('td');
      nameCell.className = 'name';
      nameCell.textContent = entry.isDirectory ? entry.name + '/' : entry.name;
      sizeCell.textContent = entry.isDirectory ? '-' : formatSize(entry.length);

      if (!entry.isDirectory) {
        const checkbox = document.createElement('input');
        checkbox.type = multiple ? 'checkbox' : 'radio';
        checkbox.name = 'pick';
        checkbox.checked = selected.has(entry.path);
        checkbox.addEventListener('change', function () {
          if (!multiple) selected.clear();
          if (checkbox.checked) selected.set(entry.path, entry);
          else selected.delete(entry.path);
          if (!multiple) tbody.querySelectorAll('input[name=pick]').forEach(function (el) {
            if (el !== checkbox) el.checked = false;
          });
          updateSelectButton();
        });
        checkCell.appendChild(checkbox);
      }

      nameCell.addEventListener('click', function () {
        if (entry.isDirectory) load(entry.path);
      });

      row.appendChild(checkCell);
      row.appendChild(nameCell);
      row.appendChild(sizeCell);
      tbody.appendChild(row);
    });
    updateSelectButton();
  }

  async function load(path) {
    const data = await api('api/entries?path=' + encodeURIComponent(path || ''));
    renderEntries(data);
  }

  function returnSelection() {
    const files = Array.from(selected.values()).map(function (entry) {
      return {
        path: entry.path,
        name: entry.name,
        length: entry.length,
        downloadUrl: base + 'api/download?path=' + encodeURIComponent(entry.path)
      };
    });
    const payload = { type: 'filemanager:picker:selected', files: files };
    if (window.opener) window.opener.postMessage(payload, '*');
    if (window.parent && window.parent !== window) window.parent.postMessage(payload, '*');
    window.close();
  }

  document.getElementById('btn-up').addEventListener('click', function () {
    if (!currentPath) return;
    const parts = currentPath.split('/');
    parts.pop();
    load(parts.join('/'));
  });
  document.getElementById('btn-refresh').addEventListener('click', function () { load(currentPath); });
  document.getElementById('btn-select').addEventListener('click', returnSelection);
  document.getElementById('btn-cancel').addEventListener('click', function () { window.close(); });

  load('');
})();
