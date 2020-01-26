let upload = '';

$(document).ready(function () {
    $("#loader-upload").fadeOut('slow');
    upload = new FileUploadWithPreview('divUploadPhotos', {
        showDeleteButtonOnImages: true,
        text: {
            chooseFile: 'Fotoğraf Seç',
            browse: 'Fotoğraf Seç',
            selectedCount: 'adet fotoğraf seçildi',
        },
        //images: {
        //    baseImage: importedBaseImage,
        //},
        //presetFiles: [
        //    'https://images.unsplash.com/photo-1557090495-fc9312e77b28?ixlib=rb-1.2.1&ixid=eyJhcHBfaWQiOjEyMDd9&auto=format&fit=crop&w=668&q=80',
        //],
    });


});

$('#inputUploadPhotos').on('click', function (e) {
    e.preventDefault();
    let loadingSpinner = $("#loader-upload");

    let appointmentId = document.getElementById('appointmentId').value;
    let customerId = document.getElementById('customerId').value;
    if (!appointmentId || !customerId) {
        swal({
            title: "Yükleme Başarısız",
            text: `Yükleme işlemi başlarken bir sorun oluştu. Lütfen tekrar deneyiniz.`,
            type: "warning",
            showCancelButton: false,
            confirmButtonText: 'Tamam',
        });
        return;
    }

    let photos = upload.cachedFileArray
    if (photos.length < 1) {
        swal({
            title: "Yükleme Başarısız",
            text: `Hiç fotoğraf seçmediniz. Lütfen en az 1 fotoğraf seçiniz.`,
            type: "warning",
            showCancelButton: false,
            confirmButtonText: 'Tamam',
        });
        return;
    }

    loadingSpinner.fadeIn("slow");
    let eachLen = 100 / photos.length;
    let currentValue = 0;
    let uploadedPhotoNumber = 0;
    changeProgressValue(0, 0, photos.length);
    for (var i = 0; i != photos.length; i++) {
        Api.uploadPhoto(photos[i], customerId, appointmentId)
            .then(res => {
                currentValue += eachLen;
                uploadedPhotoNumber++;
                changeProgressValue(currentValue, uploadedPhotoNumber, photos.length);

                if (uploadedPhotoNumber >= photos.length) {
                    loadingSpinner.fadeOut("slow");
                    swal({
                        title: "Yükleme Başarılı",
                        text: `${uploadedPhotoNumber} fotoğraf başarıyla yüklendi!`,
                        type: "success",
                        showCancelButton: false,
                        confirmButtonText: 'Tamam',
                    });
                }
            }).catch(err => {

                loadingSpinner.fadeOut("slow");
                swal({
                    title: "Yükleme Başarısız!",
                    text: `Fotoğraflar yüklenirken bir sorun oluştu! Lütfen tekrar deneyiniz!`,
                    type: "warning",
                    showCancelButton: false,
                    confirmButtonText: 'Tamam',
                });
            });

    }

});

function changeProgressValue(value, uploadedPhotoNumber, totalLength) {
    $("#loader-upload #current-progress")
        .text(Number.parseFloat(value).toFixed(2) + " % tamamlandı");
    $("#loader-upload #current-progress-count")
        .text(`${uploadedPhotoNumber}/${totalLength} fotoğraf yüklendi`);

}
