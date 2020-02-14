
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
                    text: "Randevu alabileceğiniz saatleri görmeniz için lütfen 'Çekim Türü' seçimi yapınız.",
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
            //Date: $(formId + ' #appDate').val(),
            //DateHourStart: $(formId + ' #appDateHour option:selected').data('start'),
            //DateHourEnd: $(formId + ' #appDateHour option:selected').data('end'),
            Message: $(formId + ' #appMessage').val(),
            ScheduleId: $(formId + ' #appDateHour option:selected').val()
        }
        if (appointment.ScheduleId <= 0) {
            alert("Hata");
            return;
        }
        let formData = new FormData();
        for (let key in appointment) {
            formData.append(key, appointment[key]);
        }
        toggleGlobalLoader(1);

        Api.addAppointment(formData)
            .then(res => {
                if (res.status >= 200 && res.status < 300) {
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
                }
            }).catch(err => {
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

    $('#formContact').submit((event) => {

        event.preventDefault();

        const formId = '#formContact';

        const formData = $(formId).serialize();

        toggleGlobalLoader(1);

        Api.sendContactRequest(formData)
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

    $('#formAppointment #appType').change(function () {
        const date = $('#formAppointment #appDate').val();
        const appType = $(this).val();
        if (date && appType != 0) {
            getAvailableHoursInDay(date, appType);
        }
    });
    $("#appPhone").inputmask({ "mask": "(999) 999-9999" });
    $("#contactPhone").inputmask({ "mask": "(999) 999-9999" });
});


const getAvailableHoursInDay = (date, photoType) => {

    const appDateHour = $('#appDateHour');
    appDateHour.attr('disabled', true);
    toggleGlobalLoader(1);
    Api.getSchedules(date, '', photoType)
        .then(res => {

            toggleGlobalLoader(0);

            if (res.status >= 200 && res.status < 300) {
                const data = res.data;
                appDateHour.removeAttr('disabled');
                appDateHour.empty();
                appDateHour.append('<option value="-1">Randevu Saati</option>')
                for (let i = 0; i < data.length; i++) {
                    appDateHour.append(`<option data-start="${data[i].startHour}" data-end="${data[i].endHour}" value="${data[i].id}">` +
                        `${data[i].startHour} - ${data[i].endHour}</option>`)
                }
                if (data.length == 0) {
                    swal({
                        title: 'Üzgünüz :(',
                        text: `Maalesef ${date} tarihi için uygun randevu saati yok. Dilerseniz başka tarihlere bakabilir veya daha detaylı bilgi için bizim ile iletişime geçebilirsiniz.`,
                        type: "info",
                        showCancelButton: false,
                        confirmButtonText: 'Tamam'
                    });
                }
            }
        })
        .catch(err => {
            toggleGlobalLoader(0);
            console.log(err.response.data);
        });

}