
document.addEventListener('DOMContentLoaded', () => {
    let calendarEl = document.getElementById('appFullcalendar');

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
                //start: nowDate
            };
        },

        events: {
            url: `/Appointment/GetAppointments`,
            method: 'GET',
            extraParams: {
                isApproved: 1
            },
            success: function (data) {
                console.log(data);
            },
            failure: function () {
                alert('hata');
            }
        },

        eventClick: function (info) {
            console.log("event", info.event);
            let infoModal = '#appInfoModal';
            let eventInfo = info.event.extendedProps;
            $(infoModal).modal('show');

            $(infoModal + ' #infoModalSubtitle').text(eventInfo.name + " - " + eventInfo.startDate);
            $(infoModal + ' #infoCustomerPhone').val(eventInfo.phone);
            $(infoModal + ' #infoCustomerEmail').val(eventInfo.email);
            $(infoModal + ' #infoStartTime').val(eventInfo.startHour);
            $(infoModal + ' #infoEndTime').val(eventInfo.endHour);
            $(infoModal + ' #infoCustomerShootType').val(eventInfo.photoShootType);
            $(infoModal + ' #btnEditAppointment').attr("data-appointmentid", info.event.id);
            $(infoModal + ' #btnCustomerDetails').attr("data-customeremail", eventInfo.email);

        },
        selectable: true,

        dateClick: function (dateInfo) {
            //console.log(dateInfo);

            //$('#appInfoModal').modal('show');
            //$('#eventModalDateInfo').text(dateInfo.dateStr + ' tarihine randevu saati oluşturmak üzeresiniz.');
            //$('#eventModalDate').val(dateInfo.dateStr);
        }
    });
    calendar.setOption('locale', 'tr');
    calendar.render();

    $('#startTime').timepicker({
        timeFormat: 'HH:mm',
        interval: 30,
        minTime: '08',
        maxTime: '18:00',
        defaultTime: '08',
        startTime: '08:00',
        dynamic: false,
        dropdown: true,
        scrollbar: true,
        zindex: 3500
    });

    $('#endTime').timepicker({
        timeFormat: 'HH:mm',
        interval: 30,
        minTime: '08',
        maxTime: '18:00',
        defaultTime: '08',
        startTime: '08:00',
        dynamic: false,
        dropdown: true,
        scrollbar: true,
        zindex: 3500
    });

});

const editAppointment = (el) => {

    const appointmentId = el.getAttribute("data-appointmentid");

    if (appointmentId) {
        window.location = `/Appointment/Edit/${appointmentId}`;
    }

}

const customerDetails = (el) => {

    const customerId = el.getAttribute("data-customeremail");

    if (customerId) {
        window.location = `/Customer/Details?email=${customerId}`;
    }



}