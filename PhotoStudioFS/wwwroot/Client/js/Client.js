
$(document).ready(($) => {
    "use strict";

    $('#appDate').datepicker({
        dateFormat: 'dd/mm/yy',
        minDate: 0,
        onSelect: function (dateText) {
            //console.log("Selected date: " + dateText + ", Current Selected Value= " + this.value);
            const photoType = $('#formAppointment #appType option:selected');
            if (photoType.val() != 0) {
                getAvailableHoursInDay(dateText, photoType.val());
            }
            else {
                swal({
                    title: 'Uyarı',
                    text: "Randevu alabileceğiniz saatleri görmeniz için lütfen ilk önce 'Çekim Türü' seçimi yapınız.",
                    type: "warning",
                    showCancelButton: false,
                    confirmButtonText: 'Tamam'
                });
            }
        }
    });

    $('#formAppointment').submit((event) => {

        event.preventDefault();
        const formId = '#formAppointment';
        const appointment = {
            Name: $(formId + ' #appName').val(),
            Email: $(formId + ' #appEmail').val(),
            Phone: $(formId + ' #appPhone').val(),
            ShootTypeId: $(formId + ' #appType option:selected').val(),
            Date: $(formId + ' #appDate').val(),
            DateHourStart: $(formId + ' #appDateHour option:selected').data('start'),
            DateHourEnd: $(formId + ' #appDateHour option:selected').data('end'),
            Message: $(formId + ' #appMessage').val(),
            ScheduleId: $(formId + ' #appDateHour option:selected').val()
        }
        let formData = new FormData();
        for (let key in appointment) {
            formData.append(key, appointment[key]);
        }

        Api.addAppointment(formData)
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
                console.log(err.response.data);
            });

    });

    $('#formContact').submit((event) => {

        event.preventDefault();

        const formId = '#formContact';

        const formData = $(formId).serialize();
        console.log(formData);

        Api.sendContactRequest(formData)
            .then(res => {

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

            }).catch(error => {

                swal({
                    title: 'Başarısız!',
                    text: error.response.data,
                    type: "warning",
                    showCancelButton: false,
                    confirmButtonText: 'Tamam'
                });
            });

    });
});


const getAvailableHoursInDay = (date, photoType) => {

    const appDateHour = $('#appDateHour');
    appDateHour.attr('disabled', true);

    Api.getSchedules(date, '', photoType)
        .then(res => {
            if (res.status >= 200 && res.status < 300) {
                const data = res.data;
                appDateHour.removeAttr('disabled');
                appDateHour.empty();
                appDateHour.append('<option value="-1">Randevu Saati</option>')
                for (let i = 0; i < data.length; i++) {
                    appDateHour.append(`<option data-start="${data[i].startHour}" data-end="${data[i].endHour}" value="${data[i].id}">` +
                        `${data[i].startHour} - ${data[i].endHour}</option>`)
                }
            }
        })
        .catch(err => {
            console.log(err.response.data);
        });

}