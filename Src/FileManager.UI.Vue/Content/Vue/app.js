const { createApp } = Vue;

createApp({
  data() {
    return {
      base: window.location.pathname.replace(/\/?$/, '/'),
      currentPath: '',
      canWrite: false,
      entries: []
    };
  },
  computed: {
    breadcrumb() {
      if (!this.currentPath) return [];
      const parts = this.currentPath.split('/');
      let built = '';
      return parts.map(name => {
        built = built ? built + '/' + name : name;
        return { name, path: built };
      });
    }
  },
  methods: {
    async api(path, options) {
      const response = await fetch(this.base + path, options);
      if (!response.ok) throw new Error(await response.text());
      const type = response.headers.get('content-type') || '';
      return type.includes('application/json') ? response.json() : response;
    },
    formatSize(bytes) {
      if (bytes < 1024) return bytes + ' B';
      if (bytes < 1024 * 1024) return (bytes / 1024).toFixed(1) + ' KB';
      return (bytes / (1024 * 1024)).toFixed(1) + ' MB';
    },
    async load(path) {
      const data = await this.api('api/entries?path=' + encodeURIComponent(path || ''));
      this.currentPath = data.path || '';
      this.canWrite = !!data.canWrite;
      this.entries = data.entries || [];
    },
    open(entry) {
      if (entry.isDirectory) this.load(entry.path);
      else window.open(this.base + 'api/download?path=' + encodeURIComponent(entry.path), '_blank');
    },
    goUp() {
      if (!this.currentPath) return;
      const parts = this.currentPath.split('/');
      parts.pop();
      this.load(parts.join('/'));
    },
    refresh() { this.load(this.currentPath); },
    async mkdir() {
      const name = prompt('Folder name');
      if (!name) return;
      const path = this.currentPath ? this.currentPath + '/' + name : name;
      await this.api('api/mkdir?path=' + encodeURIComponent(path), { method: 'POST' });
      await this.load(this.currentPath);
    },
    async upload(event) {
      for (const file of event.target.files) {
        const form = new FormData();
        form.append('file', file);
        await fetch(this.base + 'api/upload?path=' + encodeURIComponent(this.currentPath || ''), { method: 'POST', body: form });
      }
      event.target.value = '';
      await this.load(this.currentPath);
    },
    async remove(entry) {
      if (!confirm('Delete ' + entry.name + '?')) return;
      await this.api('api/delete?path=' + encodeURIComponent(entry.path), { method: 'POST' });
      await this.load(this.currentPath);
    },
    async rename(entry) {
      const name = prompt('New name', entry.name);
      if (!name || name === entry.name) return;
      await this.api('api/rename?path=' + encodeURIComponent(entry.path) + '&name=' + encodeURIComponent(name), { method: 'POST' });
      await this.load(this.currentPath);
    }
  },
  mounted() { this.load(''); }
}).mount('#app');
