

// Theme Management for IMS Admin Shell
window.IMSTheme = (function () {
    var STORAGE_KEY = 'ims_admin_theme';
    var DEFAULT_THEME = 'semi-dark';

    function getSavedTheme() {
        try {
            return localStorage.getItem(STORAGE_KEY) || DEFAULT_THEME;
        } catch (e) {
            return DEFAULT_THEME;
        }
    }

    function updateSwitcherUI(theme) {
        var iconElem = document.getElementById('themeSwitcherIcon');
        if (iconElem) {
            iconElem.className = 'fa-solid ' + (
                theme === 'light' ? 'fa-sun text-warning' :
                theme === 'dark' ? 'fa-moon text-info' :
                'fa-circle-half-stroke text-primary'
            );
        }

        var checkIcons = document.querySelectorAll('.theme-check-icon');
        for (var i = 0; i < checkIcons.length; i++) {
            var val = checkIcons[i].getAttribute('data-theme-check');
            if (val === theme) {
                checkIcons[i].classList.remove('d-none');
            } else {
                checkIcons[i].classList.add('d-none');
            }
        }

        var btns = document.querySelectorAll('.theme-option-btn');
        for (var j = 0; j < btns.length; j++) {
            var bVal = btns[j].getAttribute('data-theme-value');
            if (bVal === theme) {
                btns[j].classList.add('active');
            } else {
                btns[j].classList.remove('active');
            }
        }
    }

    function applyTheme(theme) {
        if (!theme || (theme !== 'light' && theme !== 'dark' && theme !== 'semi-dark')) {
            theme = DEFAULT_THEME;
        }
        document.documentElement.setAttribute('data-theme', theme);
        document.documentElement.setAttribute('data-bs-theme', theme === 'dark' ? 'dark' : 'light');
        if (document.body) {
            document.body.setAttribute('data-theme', theme);
            document.body.setAttribute('data-bs-theme', theme === 'dark' ? 'dark' : 'light');
        }
        try {
            localStorage.setItem(STORAGE_KEY, theme);
        } catch (e) { }
        updateSwitcherUI(theme);

        window.dispatchEvent(new CustomEvent('ims:themechange', { detail: { theme: theme } }));
    }

    function init() {
        var currentTheme = getSavedTheme();
        applyTheme(currentTheme);

        document.querySelectorAll('.theme-option-btn').forEach(function (btn) {
            btn.onclick = function (e) {
                e.preventDefault();
                var selectedTheme = this.getAttribute('data-theme-value');
                applyTheme(selectedTheme);
            };
        });
    }

    return {
        init: init,
        getTheme: getSavedTheme,
        setTheme: applyTheme
    };
})();

document.addEventListener('DOMContentLoaded', function () {
    IMSTheme.init();

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

// Sidebar Scroll Position and Active Item Preservation
(function () {
    var SIDEBAR_SCROLL_KEY = 'ims_sidebar_scroll_pos';

    function getSidebar() {
        return document.getElementById('IMSSidebar');
    }

    function saveScroll() {
        var sidebar = getSidebar();
        if (sidebar) {
            try {
                sessionStorage.setItem(SIDEBAR_SCROLL_KEY, sidebar.scrollTop);
            } catch (e) { }
        }
    }

    function restoreScroll() {
        var sidebar = getSidebar();
        if (!sidebar) return;

        var savedScroll = sessionStorage.getItem(SIDEBAR_SCROLL_KEY);
        var activeItem = sidebar.querySelector('.nav-link.active, .sub-link.active');

        if (savedScroll !== null && !isNaN(parseInt(savedScroll, 10))) {
            sidebar.scrollTop = parseInt(savedScroll, 10);
        } else if (activeItem) {
            activeItem.scrollIntoView({ block: 'nearest', inline: 'nearest' });
        }

        // Secondary check after layout and accordion expansion finish
        setTimeout(function () {
            if (savedScroll !== null && !isNaN(parseInt(savedScroll, 10))) {
                sidebar.scrollTop = parseInt(savedScroll, 10);
            } else if (activeItem) {
                var itemRect = activeItem.getBoundingClientRect();
                var sidebarRect = sidebar.getBoundingClientRect();
                if (itemRect.top < sidebarRect.top || itemRect.bottom > sidebarRect.bottom) {
                    activeItem.scrollIntoView({ block: 'nearest', inline: 'nearest' });
                }
            }
        }, 60);
    }

    function initSidebarScroll() {
        var sidebar = getSidebar();
        if (!sidebar) return;

        restoreScroll();

        // Save scroll position when user scrolls sidebar
        var scrollTimeout;
        sidebar.addEventListener('scroll', function () {
            clearTimeout(scrollTimeout);
            scrollTimeout = setTimeout(saveScroll, 50);
        }, { passive: true });

        // Save immediately when clicking navigation links inside sidebar
        sidebar.addEventListener('click', function (e) {
            var target = e.target.closest('a');
            if (target && target.getAttribute('href') && target.getAttribute('href') !== 'javascript:void(0);' && target.getAttribute('href') !== '#') {
                saveScroll();
            }
        });

        // Save on unload / pagehide
        window.addEventListener('beforeunload', saveScroll);
        window.addEventListener('pagehide', saveScroll);
    }

    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', initSidebarScroll);
    } else {
        initSidebarScroll();
    }
})();

// ----------------------------------------------------------------------------
// Universal Classic Confirmation Dialog System (Swing Animated)
// ----------------------------------------------------------------------------
window.IMSConfirm = (function () {
    var confirmCallback = null;

    function getOrCreateModal() {
        var el = document.getElementById('imsGlobalConfirmModal');
        if (!el) {
            var modalHtml = `
            <div class="modal fade ims-confirm-modal" id="imsGlobalConfirmModal" tabindex="-1" aria-hidden="true" data-bs-backdrop="static">
                <div class="modal-dialog modal-dialog-centered">
                    <div class="modal-content ims-confirm-content border-0 shadow-lg">
                        <div class="modal-body text-center p-4">
                            <div class="ims-confirm-icon-wrap mb-3">
                                <div class="ims-confirm-icon-circle" id="imsConfirmIconWrap">
                                    <i class="fa-solid fa-triangle-exclamation" id="imsConfirmIcon"></i>
                                </div>
                            </div>
                            <h5 class="ims-confirm-title fw-bold mb-2" id="imsConfirmTitle">Delete Confirmation</h5>
                            <p class="ims-confirm-message mb-4" id="imsConfirmMessage">Are you sure you want to delete this record? This action cannot be undone.</p>
                            <div class="d-flex justify-content-center gap-2">
                                <button type="button" class="btn btn-cancel" data-bs-dismiss="modal" id="imsConfirmCancelBtn">Cancel</button>
                                <button type="button" class="btn btn-danger btn-confirm d-flex align-items-center gap-2" id="imsConfirmOkBtn">
                                    <i class="fa-solid fa-trash-can" id="imsConfirmOkIcon"></i>
                                    <span id="imsConfirmOkText">Yes, Delete</span>
                                </button>
                            </div>
                        </div>
                    </div>
                </div>
            </div>`;
            document.body.insertAdjacentHTML('beforeend', modalHtml);
            el = document.getElementById('imsGlobalConfirmModal');

            var okBtn = document.getElementById('imsConfirmOkBtn');
            okBtn.addEventListener('click', function () {
                var btn = this;
                if (typeof confirmCallback === 'function') {
                    var originalHtml = btn.innerHTML;
                    btn.disabled = true;
                    btn.innerHTML = '<span class="spinner-border spinner-border-sm me-1" role="status" aria-hidden="true"></span> Deleting...';

                    var cb = confirmCallback;
                    confirmCallback = null;

                    var closeDialog = function () {
                        btn.disabled = false;
                        btn.innerHTML = originalHtml;
                        var inst = bootstrap.Modal.getInstance(el);
                        if (inst) inst.hide();
                    };

                    try {
                        var res = cb(closeDialog);
                        if (res && typeof res.then === 'function') {
                            res.finally(closeDialog);
                        } else if (cb.length === 0) {
                            closeDialog();
                        }
                    } catch (e) {
                        closeDialog();
                    }
                } else {
                    var inst = bootstrap.Modal.getInstance(el);
                    if (inst) inst.hide();
                }
            });
        }
        return el;
    }

    function show(options) {
        options = options || {};
        var el = getOrCreateModal();
        var titleEl = document.getElementById('imsConfirmTitle');
        var msgEl = document.getElementById('imsConfirmMessage');
        var okText = document.getElementById('imsConfirmOkText');
        var okIcon = document.getElementById('imsConfirmOkIcon');
        var cancelBtn = document.getElementById('imsConfirmCancelBtn');
        var iconWrap = document.getElementById('imsConfirmIconWrap');
        var iconEl = document.getElementById('imsConfirmIcon');
        var okBtn = document.getElementById('imsConfirmOkBtn');

        titleEl.textContent = options.title || 'Delete Confirmation';
        msgEl.innerHTML = options.message || 'Are you sure you want to delete this record? This action cannot be undone.';
        okText.textContent = options.confirmText || 'Yes, Delete';
        cancelBtn.textContent = options.cancelText || 'Cancel';

        if (options.type === 'warning') {
            iconWrap.className = 'ims-confirm-icon-circle warning';
            iconEl.className = 'fa-solid fa-triangle-exclamation';
            okBtn.className = 'btn btn-warning text-dark btn-confirm d-flex align-items-center gap-2';
        } else {
            iconWrap.className = 'ims-confirm-icon-circle';
            iconEl.className = 'fa-solid fa-triangle-exclamation';
            okBtn.className = 'btn btn-danger btn-confirm d-flex align-items-center gap-2';
        }

        if (options.icon) {
            okIcon.className = options.icon;
        } else {
            okIcon.className = 'fa-solid fa-trash-can';
        }

        return new Promise(function (resolve) {
            confirmCallback = function (done) {
                if (options.onConfirm) {
                    options.onConfirm(done);
                }
                resolve(true);
            };

            var inst = bootstrap.Modal.getOrCreateInstance(el);
            inst.show();

            el.addEventListener('hidden.bs.modal', function handler() {
                el.removeEventListener('hidden.bs.modal', handler);
                resolve(false);
            }, { once: true });
        });
    }

    function deleteConfirm(targetNameOrOptions, onConfirm) {
        var options = {};
        if (typeof targetNameOrOptions === 'string') {
            options = {
                title: 'Delete Confirmation',
                message: targetNameOrOptions.indexOf('?') > -1 || targetNameOrOptions.indexOf('<') > -1
                    ? targetNameOrOptions
                    : (targetNameOrOptions ? `Are you sure you want to delete <strong>${targetNameOrOptions}</strong>? This action cannot be undone.` : 'Are you sure you want to delete this record? This action cannot be undone.'),
                confirmText: 'Yes, Delete',
                onConfirm: onConfirm
            };
        } else if (typeof targetNameOrOptions === 'object') {
            options = targetNameOrOptions;
            if (onConfirm) options.onConfirm = onConfirm;
        }
        return show(options);
    }

    return {
        show: show,
        delete: deleteConfirm
    };
})();

// Capture-phase prevention to stop Bootstrap 5 native toggle from opening duplicate inline modal
document.addEventListener('click', function (e) {
    var btn = e.target.closest && e.target.closest('.confirm-delete-btn, [data-confirm-delete], .btn-delete-confirm, .action-delete-btn');
    if (btn && btn.getAttribute('data-bs-toggle') === 'modal') {
        btn.removeAttribute('data-bs-toggle');
    }
}, true);

// Global interceptor for delete buttons across all modules
$(document).on('click', '.confirm-delete-btn, [data-confirm-delete], .btn-delete-confirm, .action-delete-btn', function (e) {
    var $btn = $(this);
    var targetModalId = $btn.attr('data-bs-target') || $btn.attr('href') || ('#deleteModal-' + $btn.data('id'));
    var $inlineModal = $(targetModalId);

    if ($inlineModal.length && $inlineModal.hasClass('modal')) {
        e.preventDefault();
        e.stopImmediatePropagation();

        if (window.bootstrap && bootstrap.Modal) {
            var bsInstance = bootstrap.Modal.getInstance($inlineModal[0]);
            if (bsInstance) bsInstance.hide();
        }

        var modalMsg = $inlineModal.find('.modal-body').html() || '';
        modalMsg = modalMsg.trim();
        if (!modalMsg) {
            var rowName = $btn.closest('tr').find('td:nth-child(2), td:nth-child(3)').first().text().trim();
            modalMsg = rowName ? `Are you sure you want to delete <strong>${rowName}</strong>? This action cannot be undone.` : 'Are you sure you want to delete this record? This action cannot be undone.';
        }

        IMSConfirm.delete(modalMsg, function (done) {
            var $doDeleteBtn = $inlineModal.find('.do-delete-btn');
            if ($doDeleteBtn.length) {
                $doDeleteBtn.trigger('click');
            }
            done();
        });
        return false;
    }
});
