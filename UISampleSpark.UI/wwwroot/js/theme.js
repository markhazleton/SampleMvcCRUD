// Light (Cream) / Dark (Ink) theme toggle. The initial theme is set inline in
// _SiteHead.cshtml before first paint; this wires up the toggle buttons.
(function () {
    var root = document.documentElement;

    function label(btn) {
        var dark = root.getAttribute('data-bs-theme') === 'dark';
        btn.setAttribute('aria-label', dark ? 'Switch to light theme' : 'Switch to dark theme');
        btn.setAttribute('aria-pressed', dark ? 'true' : 'false');
    }

    document.querySelectorAll('[data-theme-toggle]').forEach(function (btn) {
        label(btn);
        btn.addEventListener('click', function () {
            var next = root.getAttribute('data-bs-theme') === 'dark' ? 'light' : 'dark';
            root.setAttribute('data-bs-theme', next);
            try { localStorage.setItem('mbs-theme', next); } catch (e) { }
            document.querySelectorAll('[data-theme-toggle]').forEach(label);
        });
    });
})();
