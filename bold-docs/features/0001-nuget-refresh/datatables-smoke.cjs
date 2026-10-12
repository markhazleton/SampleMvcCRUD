const fs = require('node:fs');
const path = require('node:path');
const { JSDOM } = require('./tools/node_modules/jsdom');
const root = path.resolve(__dirname, '../../../UISampleSpark.UI');
const dom = new JSDOM('<!doctype html><table id="employees"><thead><tr><th>Name</th><th>Age</th></tr></thead><tbody><tr><td>Jane</td><td>30</td></tr><tr><td>John</td><td>40</td></tr></tbody></table>', { runScripts: 'outside-only', url: 'http://localhost/' });
for (const file of ['jquery/jquery.min.js', 'datatables/dataTables.min.js', 'datatables.net-bs/js/dataTables.bootstrap5.min.js']) {
  dom.window.eval(fs.readFileSync(path.join(root, 'wwwroot/lib', file), 'utf8'));
}
(async () => {
const table = dom.window.$('#employees').DataTable();
await new Promise(resolve => table.ready(resolve));
if (table.rows().count() !== 2) throw new Error('Row count changed');
table.search('Jane').draw();
if (dom.window.document.querySelectorAll('tbody tr').length !== 1 || !dom.window.document.querySelector('tbody').textContent.includes('Jane')) throw new Error('Search rendering failed');
if (dom.window.DataTable.version !== '3.1.3') throw new Error('Wrong DataTables version');
table.destroy();
console.log('DataTables 3.1.3: legacy jQuery initialization, Bootstrap integration, rows, search and destroy passed.');
dom.window.close();

})().catch(error => { console.error(error); process.exitCode = 1; });
