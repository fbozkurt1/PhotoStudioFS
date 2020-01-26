$(document).ready(function () {






});

const deleteEmployee = (el) => {
    const email = el.getAttribute("data-email");
    const fullName = el.getAttribute("data-fullname");
    swal({
        title: "Silmek istediğinize emin misiniz?",
        text: fullName + " adlı personel silindiğinde artık sisteme giriş yapamayacak!",
        type: "warning",
        showCancelButton: true,
        cancelButtonText: 'İptal',
        confirmButtonText: 'Evet',
        dangerMode: true,
    }, function (isConfirm) {
        if (isConfirm) {
            $.ajax({
                url: '/Employee/Delete',
                method: "POST",
                contentType: 'application/x-www-form-urlencoded; charset=UTF-8',
                data: {
                    email: email
                }
            }).done(data => {
                console.log(data);
                swal({
                    title: 'Başarıyla silindi!',
                    //text: 'Silinen randevu saati: ' + eventStartTime + '. Artık müşteriler bu saate randevu alamayacak!',
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