const fs = require('fs-extra');
const path = require('path');

// Define the library mappings similar to our libman.json
const libraries = [
  {
    source: 'node_modules/jquery/dist',
    destination: 'wwwroot/lib/jquery',
    files: ['jquery.min.js', 'jquery.js', 'jquery.min.map']
  },
  {
    source: 'node_modules/jquery-validation-unobtrusive/dist',
    destination: 'wwwroot/lib/jquery-validation-unobtrusive',
    files: ['jquery.validate.unobtrusive.min.js']
  },
  {
    source: 'node_modules/datatables.net/js',
    destination: 'wwwroot/lib/datatables',
    files: ['dataTables.min.js', 'dataTables.js'] // Updated file names with correct casing
  },
  {
    source: 'node_modules/datatables.net-bs5/js',
    destination: 'wwwroot/lib/datatables.net-bs/js',
    files: ['dataTables.bootstrap5.min.js']
  },
  {
    source: 'node_modules/datatables.net-bs5/css',
    destination: 'wwwroot/lib/datatables.net-bs/css',
    files: ['dataTables.bootstrap5.min.css']
  },
  {
    source: 'node_modules/bootstrap/dist/js',
    destination: 'wwwroot/lib/bootstrap/dist/js',
    files: ['bootstrap.min.js', 'bootstrap.bundle.min.js']
  },
  {
    source: 'node_modules/bootstrap/dist/css',
    destination: 'wwwroot/lib/bootstrap/dist/css',
    files: ['bootstrap.min.css']
  },
  // Add jquery-validation files
  {
    source: 'node_modules/jquery-validation/dist',
    destination: 'wwwroot/lib/jquery-validation',
    files: ['jquery.validate.min.js', 'jquery.validate.js']
  },
  // Add Bootstrap Icons
  {
    source: 'node_modules/bootstrap-icons/font',
    destination: 'wwwroot/lib/bootstrap-icons/font',
    files: ['bootstrap-icons.css', 'bootstrap-icons.min.css']
  },
  // Copy Bootstrap Icons fonts
  {
    source: 'node_modules/bootstrap-icons/font/fonts',
    destination: 'wwwroot/lib/bootstrap-icons/font/fonts',
    files: ['bootstrap-icons.woff', 'bootstrap-icons.woff2']
  }
];

// Clean the lib directory first
console.log('Cleaning wwwroot/lib directory...');
fs.emptyDirSync(path.resolve(__dirname, 'wwwroot/lib'));

// Copy each library
console.log('Copying library files...');
libraries.forEach(lib => {
  lib.files.forEach(file => {
    const sourcePath = path.resolve(__dirname, lib.source, file);
    const destPath = path.resolve(__dirname, lib.destination, file);
    
    // Make sure the destination directory exists
    fs.ensureDirSync(path.dirname(destPath));
    
    // Copy the file
    try {
      fs.copySync(sourcePath, destPath);
      console.log(`Copied: ${sourcePath} -> ${destPath}`);
    } catch (err) {
      console.error(`Error copying ${sourcePath}: ${err.message}`);
    }
  });
});

console.log('Library build complete!');