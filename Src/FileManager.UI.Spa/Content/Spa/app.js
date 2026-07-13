(function () {
  const base = window.location.pathname.replace(/\/?$/, '/');
  let currentPath = '';
  let canWrite = true;

  const entriesEl = document.getElementById('entries');
  const breadcrumbEl = document.getElementById('breadcrumb');
  const rowTemplate = document.getElementById('row-template');

  async function api(path, options) {
    const response = await fetch(base + path, options);
    if (!response.ok) {
      const text = await response.text();
      throw new Error(text || response.statusText);
    }
    const contentType = response.headers.get('content-type') || '';
    if (contentType.indexOf('application/json') >= 0) {
      return response.json();
    }
    return response;
  }

  function formatSize(bytes) {
    if (bytes < 1024) return bytes + ' B';
    if (bytes < 1024 * 1024) return (bytes / 1024).toFixed(1) + ' KB';
    return (bytes / (1024 * 1024)).toFixed(1) + ' MB';
  }

  function renderBreadcrumb() {
    const parts = currentPath ? currentPath.split('/') : [];
    let html = '<a href="#" data-path="">Root</a>';
    let built = '';
    parts.forEach(function (part) {
      built = built ? built + '/' + part : part;
      html += ' / <a href="#" data-path="' + built + '">' + part + '</a>';
    });
    breadcrumbEl.innerHTML = html;
    breadcrumbEl.querySelectorAll('a').forEach(function (link) {
      link.addEventListener('click', function (event) {
        event.preventDefault();
        load(link.getAttribute('data-path') || '');
      });
    });
  }

  function renderEntries(data) {
    currentPath = data.path || '';
    canWrite = !!data.canWrite;
    entriesEl.innerHTML = '';
    renderBreadcrumb();

    data.entries.forEach(function (entry) {
      const row = rowTemplate.content.firstElementChild.cloneNode(true);
      const nameCell = row.querySelector('.name');
      nameCell.textContent = entry.isDirectory ? entry.name + '/' : entry.name;
      row.querySelector('.size').textContent = entry.isDirectory ? '-' : formatSize(entry.length);
      row.querySelector('.modified').textContent = entry.modifiedUtc || '';

      if (entry.isDirectory) {
        nameCell.addEventListener('click', function () { load(entry.path); });
      } else {
        nameCell.addEventListener('click', function () {
          window.open(base + 'api/download?path=' + encodeURIComponent(entry.path), '_blank');
        });
      }

      const actions = row.querySelector('.actions');
      if (canWrite) {
        const renameBtn = document.createElement('button');
        renameBtn.type = 'button';
        renameBtn.textContent = 'Rename';
        renameBtn.addEventListener('click', function () { renameEntry(entry); });
        actions.appendChild(renameBtn);

        const deleteBtn = document.createElement('button');
        deleteBtn.type = 'button';
        deleteBtn.textContent = 'Delete';
        deleteBtn.addEventListener('click', function () { deleteEntry(entry); });
        actions.appendChild(deleteBtn);
      }

      entriesEl.appendChild(row);
    });
  }

  async function load(path) {
    const data = await api('api/entries?path=' + encodeURIComponent(path || ''));
    renderEntries(data);
  }

  async function deleteEntry(entry) {
    if (!confirm('Delete ' + entry.name + '?')) return;
    await api('api/delete?path=' + encodeURIComponent(entry.path), { method: 'POST' });
    await load(currentPath);
  }

  async function renameEntry(entry) {
    const name = prompt('New name', entry.name);
    if (!name || name === entry.name) return;
    await api('api/rename?path=' + encodeURIComponent(entry.path) + '&name=' + encodeURIComponent(name), { method: 'POST' });
    await load(currentPath);
  }

  document.getElementById('btn-up').addEventListener('click', function () {
    if (!currentPath) return;
    const parts = currentPath.split('/');
    parts.pop();
    load(parts.join('/'));
  });

  document.getElementById('btn-refresh').addEventListener('click', function () { load(currentPath); });

  document.getElementById('btn-mkdir').addEventListener('click', async function () {
    const name = prompt('Folder name');
    if (!name) return;
    const path = currentPath ? currentPath + '/' + name : name;
    await api('api/mkdir?path=' + encodeURIComponent(path), { method: 'POST' });
    await load(currentPath);
  });

  document.getElementById('file-input').addEventListener('change', async function (event) {
    const files = event.target.files;
    for (const file of files) {
      const form = new FormData();
      form.append('file', file);
      await fetch(base + 'api/upload?path=' + encodeURIComponent(currentPath || ''), {
        method: 'POST',
        body: form
      });
    }
    event.target.value = '';
    await load(currentPath);
  });

  load('');
})();
