// Hiển thị ảnh được chọn từ input file lên thẻ img
// (Thẻ input có thuộc tính data-img-preview trỏ đến id của thẻ img dung để hiển thị ảnh)
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

// Tìm kiếm phân trang bằng AJAX
function paginationSearch(event, form, page) {
    if (event) event.preventDefault();
    if (!form) return;

    // Xử lý trường hợp action dùng "~/..." (Razor tilde không được resolve khi viết thẳng)
    const rawUrl = form.getAttribute("action") || form.action;
    const url = rawUrl.startsWith("~/")
        ? window.location.origin + "/" + rawUrl.slice(2)
        : new URL(rawUrl, window.location.origin).href;
    const method = (form.method || "GET").toUpperCase();
    const targetId = form.dataset.target;

    // Xóa định dạng AutoNumeric trên các ô money-input trước khi lấy FormData
    // (AutoNumeric chỉ unformat khi submit thật, không unformat khi dùng AJAX)
    form.querySelectorAll(".money-input").forEach(function (el) {
        var raw = el.value.replace(/\./g, "").replace(/,/g, ".");
        el.value = isNaN(parseFloat(raw)) ? "0" : parseFloat(raw).toString();
    });

    const formData = new FormData(form);
    formData.append("page", page);

    let fetchUrl = url;
    if (method === "GET") {
        const params = new URLSearchParams(formData).toString();
        fetchUrl = url + "?" + params;
        // Cập nhật URL trên thanh địa chỉ mà không reload trang
        history.pushState(null, "", fetchUrl);
    }

    let targetEl = null;
    if (targetId) {
        targetEl = document.getElementById(targetId);
        if (targetEl) {
            targetEl.innerHTML = `
                <div class="text-center py-4">
                    <span>Đang tải dữ liệu...</span>
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
                <div class="text-danger">
                    Không tải được dữ liệu
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