document.addEventListener('DOMContentLoaded', () => {
    let calendarEl = document.getElementById('createAppointmentFullcalendar');
    let calendar = new FullCalendar.Calendar(calendarEl, {
        eventLimit: true,
        plugins: ['dayGrid', 'timeGrid', 'list', 'interaction'],
        header: {
            right: 'dayGridMonth,timeGridWeek',
            left: 'prev,next today',
            center: 'title'
        },
        views: {
            dayGridMonth: { // name of view
                titleFormat: { year: 'numeric', month: '2-digit', day: '2-digit' },
                displayEventEnd: true
            },
            timeGridWeek: {
                titleFormat: { year: 'numeric', month: '2-digit', day: '2-digit' }
            }
        },
        eventTimeFormat: { // like '14:30:00'
            hour: '2-digit',
            minute: '2-digit',
            hour12: false
        },
        validRange: (nowDate) => {
            return {
                start: nowDate
            };
        },
        events: {
            url: `/Common/GetAllSchedules`,
            method: 'GET',
            success: function (data) {
                console.log(data);
            },
            failure: function () {
                alert('hata');
            }
        },
        eventClick: function (info) {
            let event = info.event;
            let eventProps = info.event.extendedProps;
            console.log(info.event);
            console.log(info.event.extendedProps);
            let appModal = '#appInfoModal';
            if (!eventProps.isEmpty) {
                swal({
                    title: "Üzgünüz",
                    text: "Randevu talep etmek istediğiniz saat maalesef dolu. Yeşil renkli alanlar müsait randevu saatlerini göstermektedir. Randevu almak için lütfen yeşil alanları seçiniz.",
                    type: "warning",
                    confirmButtonText: 'Tamam',
                    dangerMode: true,
                });
            } else {
                const app = {
                    date: convertDateTo_dmy(event.start),
                    startH: eventProps.startHour,
                    endH: eventProps.endHour,
                    shootType: eventProps.photoShootType,
                    shootTypeId: eventProps.photoShootTypeId,
                    scheduleId: event.id
                };
                $(appModal).modal('show');
                $(appModal + ' #infoAppShootType').val(app.shootType);
                $(appModal + ' #infoAppDate').val(app.date);
                $(appModal + ' #infoAppTime').val(app.startH + ' - ' + app.endH);
                $(appModal + ' #scheduleId').val(app.scheduleId);
                $(appModal + ' #shootTypeId').val(app.shootTypeId);


            }
        },
        selectable: true,
    });
    calendar.setOption('locale', 'tr');
    calendar.render();

});


$('#appForm').submit((event) => {

    event.preventDefault();
    const formId = '#appForm';

    const formData = $(formId).serialize();

    toggleGlobalLoader(1);

    Api.addAppointment(formData)
        .then(res => {

            toggleGlobalLoader(0);

            swal({
                title: 'Başarılı!',
                text: res.data,
                type: "success",
                showCancelButton: false,
                confirmButtonText: 'Tamam'
            }, (isConfirm) => {
                if (isConfirm) {
                    window.location.href = "/CustomerSection/Index";
                }
            });
        })
        .catch(err => {

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