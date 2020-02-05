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

                // other view-specific options here
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
            //extraParams: {
            //    custom_param1: 'something',
            //    custom_param2: 'somethingelse'
            //},
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

            if (!eventProps.isEmpty) {
                swal({
                    title: "Üzgünüz",
                    text: "Randevu talep etmek istediğiniz saat maalesef dolu. Yeşil renkli alanlar müsait randevu saatlerini göstermektedir. Randevu almak için lütfen yeşil alanları seçiniz.",
                    type: "warning",
                    confirmButtonText: 'Tamam',
                    dangerMode: true,
                });
            } else {

            }
            

            //});

            // change the border color just for fun
            //info.el.style.borderColor = 'red';
        },
        selectable: true,
        eventMouseEnter: function (info) {
            //alert(info.event.title);
        }
        
        //dateClick: function (dateInfo) {
        //    console.log(dateInfo);

        //    $('#eventModal').modal('show');
        //    $('#eventModalDateInfo').text(dateInfo.dateStr + ' tarihine randevu saati oluşturmak üzeresiniz.');
        //    $('#eventModalDate').val(dateInfo.dateStr);
        //}
    });
    calendar.setOption('locale', 'tr');
    calendar.render();

});