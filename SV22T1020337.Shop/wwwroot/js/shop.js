/* ============================================================
   LiteCommerce Shop - Global JavaScript
   ============================================================ */

// Hàm format tiền VNĐ
function formatVND(amount) {
    return amount.toLocaleString('vi-VN') + ' đ';
}

// Hàm gọi API JSON (POST)
async function postJson(url, body) {
    const resp = await fetch(url, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(body)
    });
    return resp.json();
}

// Cập nhật cart badge trên navbar
function updateCartBadge(count) {
    const badge = document.getElementById('cart-badge');
    if (!badge) return;
    badge.textContent = count > 0 ? count : '';
}

// Hiển thị Toast bootstrap
function showToast(message, type = 'success') {
    const toastEl = document.getElementById('cartToast');
    if (!toastEl) return;
    toastEl.className = `toast align-items-center text-bg-${type} border-0`;
    const msgEl = document.getElementById('toastMessage') || document.getElementById('toastMsg');
    if (msgEl) msgEl.textContent = message;
    new bootstrap.Toast(toastEl, { delay: 2500 }).show();
}

// Auto-dismiss alerts after 4s
document.addEventListener('DOMContentLoaded', function () {
    document.querySelectorAll('.alert-dismissible').forEach(function (alert) {
        setTimeout(function () {
            const bsAlert = bootstrap.Alert.getInstance(alert);
            if (bsAlert) bsAlert.close();
            else alert.remove();
        }, 4000);
    });
});
