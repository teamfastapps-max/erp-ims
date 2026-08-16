

document.addEventListener('DOMContentLoaded', function () {
    var app = document.getElementById('IMSApp');
    var toggleBtn = document.getElementById('sidebarToggle');
    var backdrop = document.getElementById('IMSBackdrop');

    if (!app || !toggleBtn) {
        return;
    }

    var isMobile = function () {
        return window.innerWidth < 992;
    };

    // Restore the desktop collapsed preference
    if (!isMobile() && localStorage.getItem('IMS-sidebar-collapsed') === 'true') {
        app.classList.add('sidebar-collapsed');
    }

    toggleBtn.addEventListener('click', function () {
        if (isMobile()) {
            app.classList.toggle('sidebar-open');
        } else {
            app.classList.toggle('sidebar-collapsed');
            localStorage.setItem('IMS-sidebar-collapsed', app.classList.contains('sidebar-collapsed'));
        }
    });

    if (backdrop) {
        backdrop.addEventListener('click', function () {
            app.classList.remove('sidebar-open');
        });
    }

    // If the window is resized past the mobile breakpoint, close the drawer state
    window.addEventListener('resize', function () {
        if (!isMobile()) {
            app.classList.remove('sidebar-open');
        }
    });
});
