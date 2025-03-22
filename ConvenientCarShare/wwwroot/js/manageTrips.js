/*jshint esversion: 6 */ 

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

        dataType: "JSON",

        //beforeSend: function () { $("#status").html("logining status"); },

        success: function (data) {
            //alert(data);
            //console.log("Data: " + data);
            $("#booking-price").html(`Total price: $${data.price}`);
            $("#status").html(data.status);

            if (data.status === "Not Available") {
                $("#extend-btn").prop("disabled", true);
            }
            else if (data.status === "Available") {
                $("#extend-btn").prop("disabled", false);
            }
            else {
                $("#status").html("Something went wrong when fetching booking extension data.");
                $("#extend-btn").prop("disabled", true);
            }
        },
        error: function (error) {
            console.error(error);
            $("#extend-btn").prop("disabled", true);
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