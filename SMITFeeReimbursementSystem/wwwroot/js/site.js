/* =============================================
   SMIT Fee Reimbursement System — site.js
   ============================================= */

(function () {
    'use strict';

    // ---- Loading Overlay ----
    const overlay = document.getElementById('globalLoadingOverlay');

    function showLoading() {
        if (!overlay) return;
        overlay.classList.remove('d-none');
        overlay.classList.add('show');
        overlay.setAttribute('aria-hidden', 'false');
    }

    function hideLoading() {
        if (!overlay) return;
        overlay.classList.add('d-none');
        overlay.classList.remove('show');
        overlay.setAttribute('aria-hidden', 'true');
    }

    // Attach to forms with data-loading="true"
    document.querySelectorAll('form[data-loading="true"]').forEach(function (form) {
        form.addEventListener('submit', function () {
            const btn = form.querySelector('button[type="submit"]');
            if (btn) {
                btn.disabled = true;
                const originalText = btn.innerHTML;
                btn.innerHTML = '<span class="spinner-border spinner-border-sm me-2" role="status"></span>Please wait...';
                // Re-enable after 10s as fallback
                setTimeout(() => {
                    btn.disabled = false;
                    btn.innerHTML = originalText;
                }, 10000);
            }
            showLoading();
        });
    });

    window.showAppLoading = showLoading;
    window.hideAppLoading = hideLoading;

    // ---- Toast Notifications ----
    function showToast(message, type) {
        type = type || 'success';
        const icons = {
            success: 'fa-circle-check',
            danger: 'fa-circle-xmark',
            warning: 'fa-triangle-exclamation',
            info: 'fa-circle-info'
        };
        const icon = icons[type] || icons.info;

        let container = document.querySelector('.toast-container');
        if (!container) {
            container = document.createElement('div');
            container.className = 'toast-container position-fixed top-0 end-0 p-3';
            container.style.zIndex = '11000';
            document.body.appendChild(container);
        }

        const toastEl = document.createElement('div');
        toastEl.className = 'toast align-items-center text-bg-' + type + ' border-0 show';
        toastEl.setAttribute('role', 'alert');
        toastEl.innerHTML = `
            <div class="d-flex">
                <div class="toast-body">
                    <i class="fa-solid ${icon} me-2"></i>${message}
                </div>
                <button type="button" class="btn-close btn-close-white me-2 m-auto" data-bs-dismiss="toast"></button>
            </div>`;

        container.appendChild(toastEl);

        // Auto-remove after 4s
        setTimeout(() => {
            toastEl.classList.remove('show');
            setTimeout(() => toastEl.remove(), 300);
        }, 4000);
    }

    window.showToast = showToast;

    // ---- Auto-dismiss alerts ----
    document.querySelectorAll('.alert.alert-success').forEach(function (alert) {
        setTimeout(function () {
            const bsAlert = bootstrap.Alert.getOrCreateInstance(alert);
            if (bsAlert) bsAlert.close();
        }, 5000);
    });

    // ---- Confirm delete dialogs ----
    document.querySelectorAll('[data-confirm]').forEach(function (el) {
        el.addEventListener('click', function (e) {
            const msg = el.getAttribute('data-confirm') || 'Are you sure?';
            if (!confirm(msg)) {
                e.preventDefault();
                e.stopPropagation();
            }
        });
    });

    // ---- File input preview ----
    document.querySelectorAll('input[type="file"][data-preview]').forEach(function (input) {
        const previewId = input.getAttribute('data-preview');
        const preview = document.getElementById(previewId);
        if (!preview) return;

        input.addEventListener('change', function () {
            const file = input.files[0];
            if (file && file.type.startsWith('image/')) {
                const reader = new FileReader();
                reader.onload = function (e) {
                    preview.src = e.target.result;
                    preview.classList.remove('d-none');
                };
                reader.readAsDataURL(file);
            }
        });
    });

})();
