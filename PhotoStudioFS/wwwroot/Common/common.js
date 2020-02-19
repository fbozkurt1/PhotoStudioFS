const UPLOAD_STATUS_UPLOAD_STARTED = 'upload-stated';
const UPLOAD_STATUS_UPLOAD_ENDED = 'upload-ended';
const UPLOAD_STATUS_UPLOAD_ERROR = 'upload-error';
const UPLOAD_STATUS_UPLOAD_PROGRESS = 'upload-progress';

function makeXMLHttpRequest(url, data, callback) {
    var request = new XMLHttpRequest();
    request.onreadystatechange = function () {
        if (request.readyState === 4) {
            if (request.status === 200) {
                callback(UPLOAD_STATUS_UPLOAD_ENDED, request.response);
            } else {
                callback(UPLOAD_STATUS_UPLOAD_ERROR, "Istenmeyen cevap " + request.status);
            }
        }
    };

    request.upload.onloadstart = function () {
        callback(UPLOAD_STATUS_UPLOAD_STARTED);
    };

    request.upload.onprogress = function (event) {
        callback(UPLOAD_STATUS_UPLOAD_PROGRESS, event);
    };


    request.upload.onerror = function (error) {
        callback(UPLOAD_STATUS_UPLOAD_ERROR, error);
    };

    request.onloadend = function () {
        if (request.status === 404) {
            callback(UPLOAD_STATUS_UPLOAD_ERROR, "Istenmeyen cevap " + request.status);
        }
    };

    request.open('POST', url);
    request.setRequestHeader('X-CSRFToken', csrftoken);
    try {
        request.send(data);
    } catch (e) {
        callback(UPLOAD_STATUS_UPLOAD_ERROR, "Istenmeyen cevap");
        console.log(e);
    }

}


axios.defaults.headers.common = {
    'X-Requested-With': 'XMLHttpRequest',
    //'X-CSRFToken': csrftoken,
    'Content-Type': 'application/x-www-form-urlencoded'
};

function getData(url, data) {
    return axios.get(url, {
        params: data
    });
}

function postData(url = ``, data = {}, options = {}) {
    return axios.post(url, data, options);
}

const Api = {
    //getBrands: async function (year) {
    //    const result = await getData(`/client/getVehicleBrands/${year}/`);
    //    return result.data.map(elem => {
    //        return { 'value': elem.markakodu, 'text': elem.markaadi }
    //    });
    //},

    uploadPhoto: function (photo, customerId, appointmentId) {
        const formData = new FormData();
        formData.append('File', photo);
        formData.append("AppointmentId", appointmentId);
        formData.append("CustomerId", customerId);

        return postData(`/Photo/AddPhoto`, formData, {
            headers: {
                'Content-Type': 'multipart/form-data'
            }
        });
    },

    sendContactRequest: function (data) {
        return postData(`Contact/Create`, data);
    },

    getPhotos: function (appointmentId, customerId) {
        return getData(`/CustomerSection/GetPhotosTemp`, {
            appointmentId: appointmentId, customerId: customerId
        });
    },

    downloadPhotos: function (appointmentId, customerId) {
        let formData = new FormData();
        formData.append("AppointmentId", appointmentId);
        formData.append("CustomerId", customerId);
        return postData(`/CustomerSection/Download`,
            formData,
            { responseType: 'blob' }
        );
    },
    getAppointment: function (appointmentId) {
        return getData(`/Appointment/Edit`, { id: appointmentId })
    },

    updateContactIsReadState: function (id) {
        return postData(`/Contact/UpdateIsRead/${id}`);
    },

    deleteContactRequest: function (id) {
        return postData(`/Contact/Delete/${id}`);
    },
    updateShootType: function (id, data) {
        return postData(`/Setting/EditShootType/${id}`, data);
    },
    addSchedule: function (data) {
        return postData(`/Schedule/AddSchedule`, data);
    },
    getSchedules: function (start, end, shootId) {
        return getData(`/Common/GetAllSchedules`, {
            start,
            end,
            photoType: shootId
        });
    },
    addAppointment: function (data) {
        return postData(`/Common/Create`, data);
    },
    addPhotoShootType: function (formData) {
        return postData(`/Setting/CreateShootType`, formData);
    }
};


function convertDateTo_dmy(str) {
    let date = new Date(str),
        m = ("0" + (date.getMonth() + 1)).slice(-2),
        d = ("0" + date.getDate()).slice(-2);
    return [d, m, date.getFullYear()].join("/");
}

/**
 * it shows or closes global loader
 * @param {any} isShow
 */
function toggleGlobalLoader(isShow) {
    if (isShow)
        $(".global-loader").fadeIn("slow");
    else
        $(".global-loader").fadeOut("slow");
}