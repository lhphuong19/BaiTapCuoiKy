// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
// Tìm kiếm phân trang bằng AJAX (tương tự Admin)
function paginationSearch(event, form, page) {
    if (event) event.preventDefault();
    if (!form) return;

    const url = form.action;
    const method = (form.method || "GET").toUpperCase();
    const targetId = form.dataset.target;

    const formData = new FormData(form);
    formData.set("Page", page);

    let fetchUrl = url;
    if (method === "GET") {
        const params = new URLSearchParams(formData).toString();
        fetchUrl = url + "?" + params;
    }

    let targetEl = null;
    if (targetId) {
        targetEl = document.getElementById(targetId);
        if (targetEl) {
            targetEl.innerHTML = `
                <div class="text-center py-5">
                    <div class="spinner-border text-secondary" role="status"></div>
                    <p class="mt-2 text-muted">Đang tải dữ liệu...</p>
                </div>`;
        }
    }

    fetch(fetchUrl, {
        method: method,
        body: method === "GET" ? null : formData
    })
        .then(res => res.text())
        .then(html => {
            if (targetEl) {
                targetEl.innerHTML = html;
            }
        })
        .catch(() => {
            if (targetEl) {
                targetEl.innerHTML = `
                <div class="text-danger text-center py-4">
                    Không tải được dữ liệu. Vui lòng thử lại.
                </div>`;
            }
        });
}

// Mở modal và load nội dung từ link vào modal
(function () {
    //dialogModal là id của modal dùng chung đuơc định nghĩa trong _Layout.cshtml
    const modalEl = document.getElementById("dialogModal");
    if (!modalEl) return;

    const modalContent = modalEl.querySelector(".modal-content");

    // Clear nội dung khi modal đóng
    modalEl.addEventListener('hidden.bs.modal', function () {
        modalContent.innerHTML = '';
    });

    window.openModal = function (event, link) {
        if (!link) return;
        if (event) event.preventDefault();

        const url = link.getAttribute("href");

        // Hiển thị loading
        modalContent.innerHTML = `
            <div class="modal-body text-center py-5">
                <span>Đang tải dữ liệu...</span>
            </div>`;

        // Khởi tạo modal (chỉ tạo 1 lần)
        let modal = bootstrap.Modal.getInstance(modalEl);
        if (!modal) {
            modal = new bootstrap.Modal(modalEl, {
                backdrop: 'static',
                keyboard: false
            });
        }

        modal.show();

        // Load nội dung
        fetch(url)
            .then(res => res.text())
            .then(html => {
                modalContent.innerHTML = html;
            })
            .catch(() => {
                modalContent.innerHTML = `
                    <div class="modal-body text-danger">
                        Không tải được dữ liệu
                    </div>`;
            });
    };
})();


function previewImage(input) {
    if (!input.files || !input.files[0]) return;

    const previewId = input.dataset.imgPreview; // lấy data-img-preview
    if (!previewId) return;

    const img = document.getElementById(previewId);
    if (!img) return;

    const reader = new FileReader();
    reader.onload = function (e) {
        img.src = e.target.result;
    };
    reader.readAsDataURL(input.files[0]);
}