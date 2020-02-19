
const onClickEditShootType = (elem) => {

    const name = elem.getAttribute('data-name');
    const description = elem.getAttribute('data-description');
    const id = elem.getAttribute('data-id');
    const isActive = elem.getAttribute('data-isactive');
    console.log(isActive);

    let shootModal = '#shootTypeModal';
    $(shootModal).modal('show');
    $(shootModal + ' #shootTypeName').val(name);
    $(shootModal + ' #shootTypeId').val(id);
    $(shootModal + ' #shootTypeDesc').val(description);
    document.getElementById('shootTypeIsActive').checked = true;
}

const editShootType = () => {

    let shootModal = '#shootTypeModal';
    const name = $(shootModal + ' #shootTypeName').val();
    const id = $(shootModal + ' #shootTypeId').val();
    const description = $(shootModal + ' #shootTypeDesc').val();
    const isActive = document.getElementById('shootTypeIsActive').checked;

    let formData = new FormData();
    formData.append("Name", name);
    formData.append("IsActive", isActive);
    formData.append("Description", description);

    Api.updateShootType(id, formData)
        .then(res => {
            if (res.status >= 200 && res.status < 300) {
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

const onChangeShootTypePhoto = (elem) => {

    let selectedPhotoName = elem.files.item(0).name;
    document.getElementById('labelShootTypePhoto').innerHTML = selectedPhotoName;
    console.log(elem.files.item(0).name);

    let reader = new FileReader();
    reader.onload = function (e) {
        $('#shootTypePreviewPhoto').attr('src', e.target.result);
    }
    reader.readAsDataURL(elem.files.item(0));
}


$('#formCreateShootType').submit((event) => {

    event.preventDefault();

    const formId = '#formCreateShootType';

    toggleGlobalLoader(1);
    const formData = new FormData($(formId)[0]);

    Api.addPhotoShootType(newFormData)
        .then(res => {

            toggleGlobalLoader(0);

            swal({
                title: 'Başarılı!',
                text: res.data,
                type: "success",
                showCancelButton: false,
                confirmButtonText: 'Tamam'
            }, (isConfirm) => {
                $(formId)[0].reset();

            });

        }).catch(error => {
            toggleGlobalLoader(0);

            swal({
                title: 'Başarısız!',
                text: error.response.data,
                type: "warning",
                showCancelButton: false,
                confirmButtonText: 'Tamam'
            });
        });

});