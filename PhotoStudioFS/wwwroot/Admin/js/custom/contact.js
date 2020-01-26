const updateIsReadState = (el) => {

    const contactId = el.getAttribute('data-id');
    if (!contactId) {
        return;
    }
    let switchSpan = document.getElementById('switchSpan' + contactId);

    Api.updateContactIsReadState(contactId)
        .then(res => {
            if (el.checked) {
                switchSpan.style.color = "#2e384d";
                switchSpan.style.fontWeight = 600;
                switchSpan.innerText = "Okundu";
            } else {
                switchSpan.style.color = "#cbd2dc";
                switchSpan.innerText = "Okunmadı";
            }
        }).catch(err => {
            el.checked = false;
        });
};

const showContactRequestDetail = (el) => {
    const contact = {
        id: el.getAttribute('data-id'),
        name: el.getAttribute('data-name'),
        email: el.getAttribute('data-email'),
        date: el.getAttribute('data-date'),
        subject: el.getAttribute('data-subject'),
        message: el.getAttribute('data-message'),
        phone: el.getAttribute('data-phone')
    }
    console.log(contact);
    if (contact.id) {
        let contactModal = $('#contactModal');
        contactModal.modal('show');
        contactModal.find('#contactModalSubtitle').text(`${contact.name} - ${contact.date}`);
        contactModal.find('#contactModalPhone').val(`${contact.phone}`);
        contactModal.find('#contactModalEmail').val(`${contact.email}`);
        contactModal.find('#contactModalSubject').val(`${contact.subject}`);
        contactModal.find('#contactModalMessage').text(`${contact.message}`);
    }
}

const deleteContactRequest = (el) => {
    const contactId = el.getAttribute('data-id');
    const contactName = el.getAttribute('data-name');
    const contactDate = el.getAttribute('data-date');
    if (!contactId) {
        swal({
            title: "Silinemedi",
            text: "İletişim isteği silinirken bir problem oluştu! Lütfen tekrar deneyiniz.",
            type: "warning",
            showCancelButton: true,
            cancelButtonText: 'Kapat'
        });
        return;
    }
    swal({
        title: "Silmek istediğinize emin misiniz?",
        text: `${contactName} adlı kişinin ${contactDate} tarihli iletişim isteğini silmek üzeresiniz!`,
        type: "warning",
        showCancelButton: true,
        cancelButtonText: 'İptal',
        confirmButtonText: 'Evet',
        dangerMode: true,
    }, function (isConfirm) {
        if (isConfirm) {
            Api.deleteContactRequest(contactId)
                .then(res => {
                    swal({
                        title: "Başarılı!",
                        text: "Silme işlemi başarıyla gerçekleşti.",
                        type: "success",
                        showCancelButton: false,
                        confirmButtonText: 'Tamam',
                    }, (isConfirm) => {
                        isConfirm ? setTimeout("location.reload();", 500) : null;

                    });
                })
                .catch(err => {
                    swal({
                        title: "Silinemedi",
                        text: "İletişim isteği silinirken bir problem oluştu! Lütfen tekrar deneyiniz.",
                        type: "warning",
                        showCancelButton: false,
                        confirmButtonText: 'Tamam'
                    });
                });
        }
    });

}