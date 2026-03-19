window.confirmAction = async function (title, text) {
    const result = await Swal.fire({
        title: title || 'Xác nhận',
        text: text || 'Bạn chắc chắn muốn thực hiện thao tác này?',
        icon: 'warning',
        showCancelButton: true,
        confirmButtonText: 'OK',
        cancelButtonText: 'Hủy',
        reverseButtons: true
    });
    return result.isConfirmed;
};