﻿document.addEventListener('DOMContentLoaded', () => {




});


const onClickEditShootType = (elem) => {

    const name = elem.getAttribute('data-name');
    const id = elem.getAttribute('data-id');
    const isActive = elem.getAttribute('data-isactive');
    console.log(isActive);

    let shootModal = '#shootTypeModal';
    $(shootModal).modal('show');
    $(shootModal + ' #shootTypeName').val(name);
    $(shootModal + ' #shootTypeId').val(id);
    document.getElementById('shootTypeIsActive').checked = true;
}

const editShootType = () => {

    let shootModal = '#shootTypeModal';
    const name = $(shootModal + ' #shootTypeName').val();
    const id = $(shootModal + ' #shootTypeId').val();
    const isActive = document.getElementById('shootTypeIsActive').checked;

    let formData = new FormData();
    formData.append("Name", name);
    formData.append("IsActive", isActive);
    Api.updateShootType(id, formData)
        .then(res => {
            if (res.status == 200) {
                swal({
                    title: 'Başarılı!',
                    text: res.data,
                    type: "success",
                    showCancelButton: false,
                    confirmButtonText: 'Tamam'
                }, (isConfirm) => {
                    if (isConfirm) {
                        setTimeout("location.reload();", 500);
                    }
                });
            }
        }).catch(err => {
            swal({
                title: 'Başarısız!',
                text: err.response.data,
                type: "warning",
                showCancelButton: false,
                confirmButtonText: 'Tamam'
            });
        });
}