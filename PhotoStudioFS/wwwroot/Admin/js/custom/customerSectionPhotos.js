$(document).ready(function () {
    $('#loader-upload').fadeOut("fast");

});

const downloadPhotos = (el) => {

    const appointmentId = el.getAttribute("data-appointmentid");
    const customerId = el.getAttribute("data-customerid");
    const customerFullName = el.getAttribute("data-customerfullname");
    if (!appointmentId || !customerId) {
        swal({
            title: 'Başarısız',
            text: "Fotoğraflarınız indirilirken bir sorun oluştu. Lütfen tekrar deneyiniz!",
            type: "warning",
            showCancelButton: false,
            confirmButtonText: 'Tamam'
        });
        return;
    }
    let loadingSpinner = $('#loader-upload');
    loadingSpinner.fadeIn('slow');
    Api.downloadPhotos(appointmentId, customerId)
        .then(res => {
            const url = window.URL.createObjectURL(new Blob([res.data]));
            const link = document.createElement('a');
            link.href = url;
            link.setAttribute('download', `${customerFullName}.zip`);
            document.body.appendChild(link);
            link.click();
            loadingSpinner.fadeOut('slow');
        })
        .catch(err => {
            loadingSpinner.fadeOut('slow');
            swal({
                title: 'Başarısız',
                text: "Fotoğraflarınız indirilirken bir sorun oluştu. Lütfen tekrar deneyiniz!",
                type: "warning",
                showCancelButton: false,
                confirmButtonText: 'Tamam'
            });
        });

}