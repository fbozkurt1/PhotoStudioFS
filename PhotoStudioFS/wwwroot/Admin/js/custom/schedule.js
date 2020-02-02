document.addEventListener('DOMContentLoaded', () => {
    let calendarEl = document.getElementById('scheduleFullcalendar');
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
            url: `/Schedule/GetAllSchedules`,
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
            console.log(info.event);
            swal({
                title: "Silmek istediğinize emin misiniz?",
                text: "Bu randevu saatini sildiğinizde, artık müşteriler bu saate randevu alamayacaklar.",
                type: "warning",
                showCancelButton: true,
                cancelButtonText: 'İptal',
                confirmButtonText: 'Evet',
                dangerMode: true,
            }, function (isConfirm) {
                console.log(isConfirm);
                if (isConfirm) {
                    deleteSchedule(info.event.id, info.event.start);
                } else {
                    swal("İptal Edildi", "Silme işlemini iptal ettiniz!", "error");
                }

            });

            // change the border color just for fun
            //info.el.style.borderColor = 'red';
        },
        selectable: true,
        eventMouseEnter: function (info) {
            //alert(info.event.title);
        },
        dateClick: function (dateInfo) {
            console.log(dateInfo);

            $('#eventModal').modal('show');
            $('#eventModalDateInfo').text(dateInfo.dateStr + ' tarihine randevu saati oluşturmak üzeresiniz.');
            $('#eventModalDate').val(dateInfo.dateStr);
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

function deleteSchedule(scheduleId, eventStartTime) {
    $.ajax({
        url: '/Schedule/DeleteSchedule',
        method: "POST",
        contentType: 'application/x-www-form-urlencoded; charset=UTF-8',
        data: {
            id: scheduleId
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
            text: "Randevu saati silinirken bir sorun oluştu! Lütfen tekrar deneyiniz.",
            type: "warning",
            showCancelButton: true,
            cancelButtonText: 'Kapat'
        });
    });
}

function addSchedule() {

    const startTime = $('#startTime').val();
    const endTime = $('#endTime').val();
    const eventDate = $('#eventModalDate').val();
    //const photoShootType = $('#photoShootType').select2('data')[0].text;
    const photoShootTypeId = $('#photoShootType').select2('data')[0].id;


    if (endTime <= startTime || photoShootTypeId == 0) {
        swal({
            title: "Uyarı",
            text: photoShootTypeId == 0 ? "Lütfen Çekim Türünü seçiniz" :
                "Randevu takvimi başlangıç saati, bitiş saatinden büyük ya da eşit olamaz!",
            type: "warning",
            showCancelButton: false,
            confirmButtonText: 'Tamam'
        });
    }
    else {
        const start = eventDate + ' ' + startTime + ':00';
        const end = eventDate + ' ' + endTime + ':00';

        let formData = new FormData();
        formData.append("start", start);
        formData.append("end", end);
        formData.append("photoShootTypeId", photoShootTypeId);

        Api.addSchedule(formData)
            .then(res => {
                if (res.status == 200) {
                    $('#eventModal').modal('hide');
                    swal({
                        title: "Başarılı",
                        text: "Randevu saati başarıyla eklendi!",
                        type: "success",
                        showCancelButton: false,
                        confirmButtonText: 'Tamam'
                    }, (isConfirm) => {
                        if (isConfirm) {
                            setTimeout("location.reload();", 500);
                        }
                    });
                }
            })
            .catch(err => {
                swal({
                    title: "Uyarı",
                    text: err.response.data,
                    type: "warning",
                    showCancelButton: false,
                    confirmButtonText: 'Tamam'
                });
            });
    }
}