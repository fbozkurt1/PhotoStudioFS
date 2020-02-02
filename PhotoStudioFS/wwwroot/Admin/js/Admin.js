
/** this function returns as 2019-12-25 */
let formatDateTo_yyyy_MM_dd = (date) => {
    let now = new Date(date);
    return [now.getFullYear(), now.getMonth() + 1, now.getDay()].join("-");
}



function validateDecimal(event) {

    let asciiCode = event.key.charCodeAt(0);
    if (!((asciiCode >= 48 && asciiCode <= 57) || asciiCode === 44 || asciiCode === 46)) {
        event.preventDefault();
    }
}

document.addEventListener('DOMContentLoaded', () => {
    document.querySelectorAll('input.float').forEach(elem => {
        elem.addEventListener('keypress', event => validateDecimal(event));
    });


}, false);


const deleteAppointment = (el) => {

    const email = el.getAttribute("data-email");
    const fullName = el.getAttribute("data-fullname");
    const id = el.getAttribute("data-id");
    const appDate = el.getAttribute("data-appDate");
    const appDateHour = el.getAttribute("data-appDateHour");

    if (!email || !fullName || !id || !appDate) {
        swal({
            title: "Silinemedi",
            text: "Personel silinirken bir problem oluştu! Lütfen tekrar deneyiniz.",
            type: "warning",
            showCancelButton: true,
            cancelButtonText: 'Kapat'
        });
        return;
    }
    swal({
        title: "Silmek istediğinize emin misiniz?",
        text: `${fullName} adlı kişinin ${appDate} ${appDateHour} tarihindeki randevusunu silmek istediğinize emin misiniz?`,
        type: "warning",
        showCancelButton: true,
        cancelButtonText: 'İptal',
        confirmButtonText: 'Evet',
        dangerMode: true,
    }, function (isConfirm) {
        if (isConfirm) {
            $.ajax({
                url: '/Appointment/Delete',
                method: "POST",
                contentType: 'application/x-www-form-urlencoded; charset=UTF-8',
                data: {
                    id: id,
                    email: email
                }
            }).done(data => {
                console.log(data);
                swal({
                    title: 'Başarıyla silindi!',
                    text: 'Silme işlemi başarıyla gerçekleşti.',
                    type: "success",
                    showCancelButton: false,
                    confirmButtonText: 'Tamam'
                }, (isConfirm) => {
                    if (isConfirm) {
                        setTimeout("location.reload();", 500);
                    }
                });

            }).fail((jqXHR, textStatus) => {

                swal({
                    title: "Silinemedi",
                    text: "Personel silinirken bir problem oluştu! Lütfen tekrar deneyiniz.",
                    type: "warning",
                    showCancelButton: true,
                    cancelButtonText: 'Kapat'
                });
            });
        }

    });

}

(function () {
    function showAlert(alert) {
        const alertContainer = $('.alert-container');

        const alertElement = $(`<div class="alert alert-${alert.type} alert-dismissible" role="alert">` +
            '<button type="button" class="close" data-dismiss="alert" aria-label="Close"><span aria-hidden="true">&times;</span></button>' +
            `<strong>${alert.title}</strong> ${alert.body}` +
            '</div>');

        alertContainer.append(alertElement);
        alertElement.alert();
    }

    $(document).ajaxComplete((event, xhr) => {
        if (xhr.getResponseHeader('x-alert-type')) {
            const alert = {
                type: xhr.getResponseHeader('x-alert-type'),
                title: xhr.getResponseHeader('x-alert-title'),
                body: xhr.getResponseHeader('x-alert-body')
            }

            showAlert(alert);
        }
    });
})();