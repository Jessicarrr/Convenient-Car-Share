


function showModal(bookingId) {

    $("#status").html("");
    $("#extendTime").val(0);
    $('#bookingId').val(bookingId);
    $("#extend-btn").prop("disabled", true);

    $("#manageModal").modal();

}


function getStatus() {

    $("#extend-btn").prop("disabled", true);

    var bookingId = $('#bookingId').val();

    if ($("#extendTime").val() == 0) {
        $("#status").html("");
        return;
    }

    $('#extendHour').val($("#extendTime").val());

    $.ajax({
        type: "POST",

        url: '../Extend/ExtendBooking',

        data: { bookingId: bookingId, extendTime: $("#extendTime").val() },

        datatype: "JSON",

        beforeSend: function () { $("#status").html("logining status"); },

        success: function (data) {
            $("#status").html("*" + data);

            switch (data) {

                case "Not Available":
                    $("#extend-btn").prop("disabled", true);
                    break;

                case "Available":
                    $("#extend-btn").prop("disabled", false);
                    break;

            }
        },
    });



}

function addAddress(latitude, longitude, element) {
    var geocoder = new google.maps.Geocoder();

    var latlng = { lat: parseFloat(latitude), lng: parseFloat(longitude) };
    geocoder.geocode({'location': latlng}, function (results, status) {
        if (status == 'OK') {
            if (results[0]) {
                element.innerHTML = results[0].formatted_address;
                return true;
            }
            else {
                return false;
            }
        }
        else {
            return false;
        }
    });
}

function initAddresses() {
    $(".address").each(function (i, element) {
        var latitude = element.dataset.lat;
        var longitude = element.dataset.long;

        var result = addAddress(latitude, longitude, element);
    });
}

function showCancelWindow(carId, carModel, carBrand, startDate, endDate, price) {
    console.log("car id = " + carId);
    $('input[name="cancelBookingId"]').attr('value', carId);

    var popupWindowTextLine1 = "You are about to cancel your booking with the "
        + carBrand + " " + carModel + ". This booking starts at " + startDate + " and ends at " + endDate + ".";
    var popupWindowTextLine2 = "You will be refunded $" + price + " upon cancellation. "
        + "Are you sure you wish to cancel this booking?";

    $("#cancel-booking-body-line1").text(popupWindowTextLine1);
    $("#cancel-booking-body-line2").text(popupWindowTextLine2);

    $("#cancelBookingModal").modal();
}