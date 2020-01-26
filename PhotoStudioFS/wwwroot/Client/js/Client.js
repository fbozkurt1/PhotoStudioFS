
$(document).ready(($) => {
    "use strict";

    $('#appDate').datepicker({
        dateFormat: 'dd/mm/yy',
        minDate: 0,
        onSelect: function (dateText) {
            //console.log("Selected date: " + dateText + ", Current Selected Value= " + this.value);
            const photoType = $('#formAppointment #appType option:selected');
            if (photoType.val() != 0) {
                getAvailableHoursInDay(dateText, photoType.text());
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
            //Todo: get available hours from server
        }
    });

    $('#formAppointment').submit((event) => {

        event.preventDefault();
        const formId = '#formAppointment';
        const appointment = {
            Name: $(formId + ' #appName').val(),
            Email: $(formId + ' #appEmail').val(),
            Phone: $(formId + ' #appPhone').val(),
            Type: $(formId + ' #appType option:selected').text(),
            Date: $(formId + ' #appDate').val(),
            DateHourStart: $(formId + ' #appDateHour option:selected').data('start'),
            DateHourEnd: $(formId + ' #appDateHour option:selected').data('end'),
            Message: $(formId + ' #appMessage').val(),
            ScheduleId: $(formId + ' #appDateHour option:selected').val()
        }

        $.ajax({
            url: '/Appointment/Create',
            method: "POST",
            contentType: 'application/x-www-form-urlencoded; charset=UTF-8',
            data: appointment
        }).done(response => {

            console.log('başarılı', response);
            swal({
                title: 'Başarılı!',
                text: response,
                type: "success",
                showCancelButton: false,
                confirmButtonText: 'Tamam'
            }, (isConfirm) => {
                if (isConfirm) {
                    setTimeout("location.reload();", 500);
                }
            });

        }).fail((jqXHR, textStatus) => {
            console.log('Hata', jqXHR.responseText);
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
    $.ajax({
        url: '/Schedule/GetAllSchedules',
        method: "GET",
        contentType: 'application/x-www-form-urlencoded; charset=UTF-8',
        dataType: 'json',
        data: {
            start: date,
            end: '',
            photoType: photoType
        }
    }).done(data => {
        appDateHour.removeAttr('disabled');
        appDateHour.empty();
        appDateHour.append('<option value="-1">Randevu Saati</option>')
        for (let i = 0; i < data.length; i++) {
            appDateHour.append(`<option data-start="${data[i].startHour}" data-end="${data[i].endHour}" value="${data[i].id}">` +
                `${data[i].startHour} - ${data[i].endHour}</option>`)
        }


    }).fail((jqXHR, textStatus) => {
        console.log(jqXHR.responseText);

    });
}