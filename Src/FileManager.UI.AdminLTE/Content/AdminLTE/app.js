(function () {
  const base = window.location.pathname.replace(/\/?$/, '/');
  let currentPath = '';
  let canWrite = true;

  async function api(path, options) {
    const response = await fetch(base + path, options);
    if (!response.ok) throw new Error(await response.text());
    const type = response.headers.get('content-type') || '';
    return type.includes('application/json') ? response.json() : response;
  }

  function formatSize(bytes) {
    if (bytes < 1024) return bytes + ' B';
    if (bytes < 1024 * 1024) return (bytes / 1024).toFixed(1) + ' KB';
    return (bytes / (1024 * 1024)).toFixed(1) + ' MB';
  }

  function renderBreadcrumb() {
    const parts = currentPath ? currentPath.split('/') : [];
    let html = '<li class="nav-item"><a class="nav-link" href="#" data-path="">Root</a></li>';
    let built = '';
    parts.forEach(function (part) {
      built = built ? built + '/' + part : part;
      html += '<li class="nav-item"><a class="nav-link" href="#" data-path="' + built + '">' + part + '</a></li>';
    });
    $('#breadcrumb').html(html).find('a').on('click', function (e) {
      e.preventDefault();
      load($(this).data('path') || '');
    });
  }

  function renderEntries(data) {
    currentPath = data.path || '';
    canWrite = !!data.canWrite;
    renderBreadcrumb();
    const rows = (data.entries || []).map(function (entry) {
      const icon = entry.isDirectory ? 'fa-folder text-warning' : 'fa-file text-secondary';
      const actions = canWrite
        ? '<button class="btn btn-xs btn-outline-secondary rename" data-path="' + entry.path + '" data-name="' + entry.name + '">Rename</button> ' +
          '<button class="btn btn-xs btn-outline-danger delete" data-path="' + entry.path + '" data-name="' + entry.name + '">Delete</button>'
        : '';
      return '<tr><td class="name" data-entry="' + encodeURIComponent(JSON.stringify(entry)) + '"><i class="fas ' + icon + ' mr-2"></i>' +
        (entry.isDirectory ? entry.name + '/' : entry.name) + '</td><td>' +
        (entry.isDirectory ? '-' : formatSize(entry.length)) + '</td><td>' + (entry.modifiedUtc || '') + '</td><td>' + actions + '</td></tr>';
    }).join('');
    $('#entries').html(rows);
    $('.name').on('click', function () {
      const entry = JSON.parse(decodeURIComponent($(this).data('entry')));
      if (entry.isDirectory) load(entry.path);
      else window.open(base + 'api/download?path=' + encodeURIComponent(entry.path), '_blank');
    });
    $('.delete').on('click', function () {
      const path = $(this).data('path');
      const name = $(this).data('name');
      if (!confirm('Delete ' + name + '?')) return;
      api('api/delete?path=' + encodeURIComponent(path), { method: 'POST' }).then(function () { load(currentPath); });
    });
    $('.rename').on('click', function () {
      const path = $(this).data('path');
      const name = $(this).data('name');
      const newName = prompt('New name', name);
      if (!newName || newName === name) return;
      api('api/rename?path=' + encodeURIComponent(path) + '&name=' + encodeURIComponent(newName), { method: 'POST' })
        .then(function () { load(currentPath); });
    });
    $('#btn-mkdir, label.btn-success').toggle(canWrite);
  }

  async function load(path) {
    const data = await api('api/entries?path=' + encodeURIComponent(path || ''));
    renderEntries(data);
  }

  $('#btn-up').on('click', function () {
    if (!currentPath) return;
    const parts = currentPath.split('/');
    parts.pop();
    load(parts.join('/'));
  });
  $('#btn-refresh').on('click', function () { load(currentPath); });
  $('#btn-mkdir').on('click', async function () {
    const name = prompt('Folder name');
    if (!name) return;
    const path = currentPath ? currentPath + '/' + name : name;
    await api('api/mkdir?path=' + encodeURIComponent(path), { method: 'POST' });
    await load(currentPath);
  });
  $('#file-input').on('change', async function (event) {
    for (const file of event.target.files) {
      const form = new FormData();
      form.append('file', file);
      await fetch(base + 'api/upload?path=' + encodeURIComponent(currentPath || ''), { method: 'POST', body: form });
    }
    event.target.value = '';
    await load(currentPath);
  });

  load('');
})();
