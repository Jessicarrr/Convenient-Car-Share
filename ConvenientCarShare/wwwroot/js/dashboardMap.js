
//values
var geocoder;
var map;
var infowindow;
var infoWindowList = new Array();



//Init date time picker
var dateNow = new Date();
var day = moment();
$("#start-datetime").datetimepicker({
    format: 'YYYY-MM-DD HH:mm',
    defaultDate: day,

    minDate: day
});

$("#end-datetime").datetimepicker({
    format: 'YYYY-MM-DD HH:mm',
    useCurrent: false,

    minDate: day
});

$("#start-datetime").on("dp.change", function (e) {
    $('#end-datetime').data("DateTimePicker").minDate(e.date);
});
$("#end-datetime").on("dp.change", function (e) {
    $('#start-datetime').data("DateTimePicker").maxDate(e.date);
});




//Filter function using ajax
$(document).ready(function () {
    //console.log("Document ready.");

    // process the form
    $('#filter-form').submit(function (event) {
        //console.log("Submit");

        //prevent default submit function.
        event.preventDefault();
        // had to remove geocode due to api restrictions
        /*if ($('#postcode').val() != "") {

            geocodeLatLng(geocoder, map, infowindowm);

        }*/

        var startDatetime = moment($('#start-datetime').val(), "YYYY-MM-DD HH:mm").toDate();
        if ($('#end-datetime').val() == "") {
            var endDatetime = new Date();
            endDatetime.setHours(startDatetime.getHours() + 2);
        }
        else {
            var endDatetime = moment($('#end-datetime').val(), "YYYY-MM-DD HH:mm").toDate();

        }

        var minPrice = parseInt($('#min-price').val());
        var maxPrice = parseInt($('#max-price').val());

        if (maxPrice < minPrice) {
            //console.log(`Bad maxPrice ${maxPrice} vs minPrice ${minPrice}.`)
            $("#filter-error-text").html("Max price must be the same as or greater than the minimum price.");
            return;
        }

        $("#filter-error-text").html("");

        //console.log("Starting ajax.");
        $.ajax({
            type: "GET",

            url: 'GetCarsNotBookedDuring',

            data: { StartDate: startDatetime.toISOString(), EndDate: endDatetime.toISOString() },

            datatype: "JSON",

            //function that before send
            //beforeSend: function () { $("#msg").html("logining"); },

            success: function (data) {
                //console.log("Ajax success.");
                if (data.error) {
                    $("#filter-error-text").html(data.error);
                    //console.log("Data had an error in it.");
                    //console.log(data);
                    return;
                }

                $("#filter-error-text").html("");

                spotsAndCars.clear();
                var cars = JSON.parse(data);
                spotsAndCars = new Map();
                var select_drop = document.getElementById("car_type");
                var selected = select_drop.options[select_drop.selectedIndex].value;
                cars.forEach(function (car) {

                    var carDetails = new Map();
                    carDetails.set("Id", car.Id);
                    carDetails.set("Model", car.Model);
                    carDetails.set("Brand", car.Brand);
                    carDetails.set("Latitude", car.Latitude);
                    carDetails.set("Longitude", car.Longitude);
                    carDetails.set("Colour", car.Colour);
                    carDetails.set("Price", car.Price);
                    carDetails.set("Capacity", car.Capacity);

                    if (car.CurrentlyParkedAt == null) {
                        return;
                    }
                    if (selected != "All") {

                        if (car.Brand != selected) {
                            return;
                        }
                    }

                    //price filter
                    if (car.Price < $('#min-price').val() || car.Price > $('#max-price').val()) {
                        return;
                    }



                    var parkingAreaDetails = new Map();
                    parkingAreaDetails.set("Longitude", car.CurrentlyParkedAt.Longitude);
                    parkingAreaDetails.set("Latitude", car.CurrentlyParkedAt.Latitude);
                    if (spotsAndCars.get(parkingAreaDetails) == null) {

                        var carArray = new Array();
                        carArray.push(carDetails);
                        spotsAndCars.set(parkingAreaDetails, carArray);

                    }
                    else {

                        var carArray = spotsAndCars.get(parkingAreaDetails);
                        carArray.push(carDetails);
                        spotsAndCars.set(parkingAreaDetails, carArray);


                    }

                });

                //console.log(spotsAndCars);
                initMap();

            },
        });



    });


});

function geocodeLatLng(geocoder, map, infowindow) {
    var postcode = document.getElementById('postcode').value;
    var hello = new XMLHttpRequest();
    var address = "https://maps.googleapis.com/maps/api/geocode/json?&address=" + postcode + "&components=country:AU&key=AIzaSyDRKU2-51-N915Lv4-_YRRoiPOku5VDS08w";
    hello.open("GET", address, true);
    hello.send(null);
    var p = JSON.parse(hello.response);
    console.log(p);
    if (p.results[0].formatted_address === 'Australia') {

        window.alert('wrong postcode');
    }
    else {
        var latlng = { lat: parseFloat(p.results[0].geometry.location.lat), lng: parseFloat(p.results[0].geometry.location.lng) };
        geocoder.geocode({ 'location': latlng }, function (results, status) {
            if (status === 'OK') {
                if (results[0]) {
                    map.setZoom(15);
                    map.setCenter(latlng);
                    infowindow.setContent(results[0].formatted_address);
                } else {
                    window.alert('No results found');
                }
            } else {
                window.alert('Geocoder failed due to: ' + status);
            }
        });
    }
}

function initMap() {
    var options = {
        zoom: 15,
        center: { lat: -37.8136, lng: 144.9631 },
        styles: [
            {
                "elementType": "geometry",
                "stylers": [
                    {
                        "color": "#f5f5f5"
                    }
                ]
            },
            {
                "elementType": "labels.icon",
                "stylers": [
                    {
                        "visibility": "off"
                    }
                ]
            },
            {
                "elementType": "labels.text.fill",
                "stylers": [
                    {
                        "color": "#616161"
                    }
                ]
            },
            {
                "elementType": "labels.text.stroke",
                "stylers": [
                    {
                        "color": "#f5f5f5"
                    }
                ]
            },
            {
                "featureType": "administrative",
                "elementType": "geometry",
                "stylers": [
                    {
                        "visibility": "off"
                    }
                ]
            },
            {
                "featureType": "administrative.land_parcel",
                "elementType": "labels.text.fill",
                "stylers": [
                    {
                        "color": "#bdbdbd"
                    }
                ]
            },
            {
                "featureType": "poi",
                "stylers": [
                    {
                        "visibility": "off"
                    }
                ]
            },
            {
                "featureType": "poi",
                "elementType": "geometry",
                "stylers": [
                    {
                        "color": "#eeeeee"
                    }
                ]
            },
            {
                "featureType": "poi",
                "elementType": "labels.text.fill",
                "stylers": [
                    {
                        "color": "#757575"
                    }
                ]
            },
            {
                "featureType": "poi.park",
                "elementType": "geometry",
                "stylers": [
                    {
                        "color": "#e5e5e5"
                    }
                ]
            },
            {
                "featureType": "poi.park",
                "elementType": "labels.text.fill",
                "stylers": [
                    {
                        "color": "#9e9e9e"
                    }
                ]
            },
            {
                "featureType": "road",
                "elementType": "geometry",
                "stylers": [
                    {
                        "color": "#ffffff"
                    }
                ]
            },
            {
                "featureType": "road",
                "elementType": "labels.icon",
                "stylers": [
                    {
                        "visibility": "off"
                    }
                ]
            },
            {
                "featureType": "road.arterial",
                "elementType": "labels.text.fill",
                "stylers": [
                    {
                        "color": "#757575"
                    }
                ]
            },
            {
                "featureType": "road.highway",
                "elementType": "geometry",
                "stylers": [
                    {
                        "color": "#dadada"
                    }
                ]
            },
            {
                "featureType": "road.highway",
                "elementType": "labels.text.fill",
                "stylers": [
                    {
                        "color": "#616161"
                    }
                ]
            },
            {
                "featureType": "road.local",
                "elementType": "labels.text.fill",
                "stylers": [
                    {
                        "color": "#9e9e9e"
                    }
                ]
            },
            {
                "featureType": "transit",
                "stylers": [
                    {
                        "visibility": "off"
                    }
                ]
            },
            {
                "featureType": "transit.line",
                "elementType": "geometry",
                "stylers": [
                    {
                        "color": "#e5e5e5"
                    }
                ]
            },
            {
                "featureType": "transit.station",
                "elementType": "geometry",
                "stylers": [
                    {
                        "color": "#eeeeee"
                    }
                ]
            },
            {
                "featureType": "water",
                "elementType": "geometry",
                "stylers": [
                    {
                        "color": "#c9c9c9"
                    }
                ]
            },
            {
                "featureType": "water",
                "elementType": "labels.text.fill",
                "stylers": [
                    {
                        "color": "#9e9e9e"
                    }
                ]
            }
        ]

    };
    map = new google.maps.Map(document.getElementById('map'), options);

    // A marker with a custom PNG glyph.
    const imageElement = document.createElement("img");

    imageElement.src =
        "/images/car.png";

    const glyphPngPinElement = new PinElement({
        glyph: imageElement,
    });

    /*var image = {
        url: '/images/car.png',
        width: 27,
        height: 39,
        size: new google.maps.Size(27, 39)

    };*/

    
    for (let [key, value] of spotsAndCars) {
        var parkingArea = key;
        var carList = spotsAndCars.get(parkingArea);
        var contentString = "";
        var totalCar = 0;

        carList.forEach(function (car) {
            contentString = contentString +
                '<div id="bodyContent">' +
                '<p><b>' + car.get("Brand") + " " + car.get("Model") + '</b></p>' +
                //'<p><b>' + ca.results[0].formatted_address + '</b></p>' +
                '<p><b>$' + car.get("Price") + '/hr</b></p>' +
                '<p><b>' + car.get("Capacity") + '</b></p>' +
                '<p><b>Available</b></p>' +
                '</div>' +
                '<form method="get" action = "/Bookings/Book">' +
                '<input value="' + car.get("Id") + '" class="form-control"  name="Id" type="hidden"/>' +
                '<input class="form-control" value = "' + document.getElementById("start-datetime").value + '" name="startTime" type="hidden" id = "start-time"/>' +
                '<input class="form-control" value = "' + document.getElementById("end-datetime").value + '" name="endTime" type="hidden" id = "end-time"/>' +
                '<button type="submit" class="btn btn-info">Book</button>' +
                '</form>' +
                "<hr />";
            totalCar += 1;
        });
        //
        if (totalCar == 1 || 0) {
            var wordCar = totalCar + " car ";
        }
        else {
            var wordCar = totalCar + " cars ";
        }

        var infoWindow = new google.maps.InfoWindow(
            {

                content: '<div class="mapInfoWindow">' + '<h5 id="iwAddressField"></h5>' + '<div class="alert alert-success">' + wordCar + 'in current parking' +
                    '</div> ' + '<div class="content">' + contentString + '</div> ' + '</div> ',
                latitude: parkingArea.get("Latitude"),
                longitude: parkingArea.get("Longitude")
            });

        infoWindowList.push(infoWindow);

        var marker = new google.maps.marker.AdvancedMarkerElement({
            position: {
                lat: parkingArea.get("Latitude"),
                lng: parkingArea.get("Longitude")

            },
            map: map,
            draggable: false,
            content: glyphSvgPinElement.element,
            //animation: google.maps.Animation.DROP,
            title: "Car park spot",
            infowindow: infoWindow

        });

        google.maps.event.addListener(marker, 'click', function () {
            infoWindowList.forEach(function (window) {
                window.close();

            });



            this.infowindow.open(map, this);

            //center the map onclick
            map.setCenter(this.getPosition());

        });

    }

    google.maps.event.addListener(map, 'click', function () {
        infoWindowList.forEach(function (window) {
            window.close();

        });

    });



    //marker.addListener('click', toggleBounce);

    geocoder = new google.maps.Geocoder;
    infowindowm = new google.maps.InfoWindow;

    addTitles();
}

function addTitles() {
    infoWindowList.forEach(function (infoWindow) {
        var newContent = infoWindow.content.replace(
            '<h5 id="iwAddressField"></h5>',
            '<h5 id="iwAddressField">Car Park</h5>'
        );
        infoWindow.setContent(newContent);
    });
}



function addInfoWindowToMarker(infoWindowList, infoWindow, contentString, map, marker) {
    infoWindowList.forEach(function (window) {
        window.close();

    });

    infoWindow.setContent(contentString);
    infoWindow.open(map, marker);

    //center the map onclick
    map.setCenter(marker.getPosition());
}

function toggleBounce() {
    if (marker.getAnimation() !== null) {
        marker.setAnimation(null);
    } else {
        marker.setAnimation(google.maps.Animation.BOUNCE);
    }
}
//values
var geocoder;
var map;
var infowindow;
var infoWindowList = new Array();



//Init date time picker
var dateNow = new Date();
var day = moment();
$("#start-datetime").datetimepicker({
    format: 'YYYY-MM-DD HH:mm',
    defaultDate: day,

    minDate: day
});

$("#end-datetime").datetimepicker({
    format: 'YYYY-MM-DD HH:mm',
    useCurrent: false,

    minDate: day
});

$("#start-datetime").on("dp.change", function (e) {
    $('#end-datetime').data("DateTimePicker").minDate(e.date);
});
$("#end-datetime").on("dp.change", function (e) {
    $('#start-datetime').data("DateTimePicker").maxDate(e.date);
});




//Filter function using ajax
$(document).ready(function () {

    // process the form
    $('#filter-form').submit(function (event) {

        //prevent default submit function.
        event.preventDefault();
        // had to remove geocode due to api restrictions
        /*if ($('#postcode').val() != "") {

            geocodeLatLng(geocoder, map, infowindowm);

        }*/

        var startDatetime = moment($('#start-datetime').val(), "YYYY-MM-DD HH:mm").toDate();
        if ($('#end-datetime').val() == "") {
            var endDatetime = new Date();
            endDatetime.setHours(startDatetime.getHours() + 2);
        }
        else {
            var endDatetime = moment($('#end-datetime').val(), "YYYY-MM-DD HH:mm").toDate();

        }

        var minPrice = $('#min-price').val();
        var maxPrice = $('#max-price').val();

        if (maxPrice > minPrice) {
            $("#filter-error-text").html = "Max price must be the same as or greater than the minimum price.";
            return;
        }

        $("#filter-error-text").html = "";

        $.ajax({
            type: "GET",

            url: '/Customer/GetCarsNotBookedDuring',

            data: { StartDate: startDatetime.toISOString(), EndDate: endDatetime.toISOString() },

            datatype: "JSON",

            //function that before send
            //beforeSend: function () { $("#msg").html("logining"); },

            success: function (data) {
                if (data.error !== null) {
                    $("#filter-error-text").html = data.error;
                    return;
                }

                $("#filter-error-text").html = "";

                spotsAndCars.clear();
                var cars = JSON.parse(data);
                spotsAndCars = new Map();
                var select_drop = document.getElementById("car_type");
                var selected = select_drop.options[select_drop.selectedIndex].value;
                cars.forEach(function (car) {

                    var carDetails = new Map();
                    carDetails.set("Id", car.Id);
                    carDetails.set("Model", car.Model);
                    carDetails.set("Brand", car.Brand);
                    carDetails.set("Latitude", car.Latitude);
                    carDetails.set("Longitude", car.Longitude);
                    carDetails.set("Colour", car.Colour);
                    carDetails.set("Price", car.Price);
                    carDetails.set("Capacity", car.Capacity);

                    if (car.CurrentlyParkedAt == null) {
                        return;
                    }
                    if (selected != "All") {

                        if (car.Brand != selected) {
                            return;
                        }
                    }

                    //price filter
                    if (car.Price < $('#min-price').val() || car.Price > $('#max-price').val()) {
                        return;
                    }



                    var parkingAreaDetails = new Map();
                    parkingAreaDetails.set("Longitude", car.CurrentlyParkedAt.Longitude);
                    parkingAreaDetails.set("Latitude", car.CurrentlyParkedAt.Latitude);
                    if (spotsAndCars.get(parkingAreaDetails) == null) {

                        var carArray = new Array();
                        carArray.push(carDetails);
                        spotsAndCars.set(parkingAreaDetails, carArray);

                    }
                    else {

                        var carArray = spotsAndCars.get(parkingAreaDetails);
                        carArray.push(carDetails);
                        spotsAndCars.set(parkingAreaDetails, carArray);


                    }

                });

                //console.log(spotsAndCars);
                initMap();

            },
        });



    });


});

function geocodeLatLng(geocoder, map, infowindow) {
    var postcode = document.getElementById('postcode').value;
    var hello = new XMLHttpRequest();
    var address = "https://maps.googleapis.com/maps/api/geocode/json?&address=" + postcode + "&components=country:AU&key=AIzaSyDRKU2-51-N915Lv4-_YRRoiPOku5VDS08w";
    hello.open("GET", address, true);
    hello.send(null);
    var p = JSON.parse(hello.response);
    console.log(p);
    if (p.results[0].formatted_address === 'Australia') {

        window.alert('wrong postcode');
    }
    else {
        var latlng = { lat: parseFloat(p.results[0].geometry.location.lat), lng: parseFloat(p.results[0].geometry.location.lng) };
        geocoder.geocode({ 'location': latlng }, function (results, status) {
            if (status === 'OK') {
                if (results[0]) {
                    map.setZoom(15);
                    map.setCenter(latlng);
                    infowindow.setContent(results[0].formatted_address);
                } else {
                    window.alert('No results found');
                }
            } else {
                window.alert('Geocoder failed due to: ' + status);
            }
        });
    }
}

function initMap() {
    var options = {
        zoom: 15,
        center: { lat: -37.8136, lng: 144.9631 },
        mapId: 'convenient-car-share-map-id',
    };

    map = new google.maps.Map(document.getElementById('map'), options);

    /*var image = {
        url: '/images/car.png',
        width: 27,
        height: 39,
        size: new google.maps.Size(27, 39)

    };*/

    
    for (let [key, value] of spotsAndCars) {
        var parkingArea = key;
        var carList = spotsAndCars.get(parkingArea);
        var contentString = "";
        var totalCar = 0;

        carList.forEach(function (car) {
            contentString = contentString +
                '<div id="bodyContent">' +
                '<p><b>' + car.get("Brand") + " " + car.get("Model") + '</b></p>' +
                //'<p><b>' + ca.results[0].formatted_address + '</b></p>' +
                '<p><b>$' + car.get("Price") + '/hr</b></p>' +
                '<p><b>' + car.get("Capacity") + '</b></p>' +
                '<p><b>Available</b></p>' +
                '</div>' +
                '<form method="get" action = "/Bookings/Book">' +
                '<input value="' + car.get("Id") + '" class="form-control"  name="Id" type="hidden"/>' +
                '<input class="form-control" value = "' + document.getElementById("start-datetime").value + '" name="startTime" type="hidden" id = "start-time"/>' +
                '<input class="form-control" value = "' + document.getElementById("end-datetime").value + '" name="endTime" type="hidden" id = "end-time"/>' +
                '<button type="submit" class="btn btn-info">Book</button>' +
                '</form>' +
                "<hr />";
            totalCar += 1;
        });
        //
        if (totalCar == 1 || 0) {
            var wordCar = totalCar + " car ";
        }
        else {
            var wordCar = totalCar + " cars ";
        }

        // A marker with a custom PNG glyph.
        const imageElement = document.createElement("img");

        imageElement.src =
            "/images/car.png";

        const glyphPngPinElement = new google.maps.marker.PinElement({
            glyph: imageElement,
        });

        let infoWindow = new google.maps.InfoWindow(
            {

                content: '<div class="mapInfoWindow">' + '<h5 id="iwAddressField"></h5>' + '<div class="alert alert-success">' + wordCar + 'in current parking' +
                    '</div> ' + '<div class="content">' + contentString + '</div> ' + '</div> ',
                latitude: parkingArea.get("Latitude"),
                longitude: parkingArea.get("Longitude")
            });

        infoWindowList.push(infoWindow);

        let marker = new google.maps.marker.AdvancedMarkerElement({
            position: {
                lat: parkingArea.get("Latitude"),
                lng: parkingArea.get("Longitude")

            },
            map: map,
            draggable: false,
            content: glyphPngPinElement.element,
            //animation: google.maps.Animation.DROP,
            title: "Car park spot",
            //infowindow: infoWindow

        });

        marker.addListener("gmp-click", () => {
            infoWindowList.forEach(function (window) {
                window.close();

            });

            infoWindow.open({
                anchor: marker,
                map: map,
                shouldFocus: true
            });

            //center the map onclick
            //map.setCenter(this.getPosition());
        });

        /*google.maps.event.addListener(marker, 'click', function () {
            infoWindowList.forEach(function (window) {
                window.close();

            });



            this.infowindow.open(map, this);

            //center the map onclick
            map.setCenter(this.getPosition());

        });*/

    }

    google.maps.event.addListener(map, 'click', function () {
        infoWindowList.forEach(function (window) {
            window.close();

        });

    });



    //marker.addListener('click', toggleBounce);

    geocoder = new google.maps.Geocoder;
    infowindowm = new google.maps.InfoWindow;

    addTitles();
}

function addTitles() {
    infoWindowList.forEach(function (infoWindow) {
        var newContent = infoWindow.content.replace(
            '<h5 id="iwAddressField"></h5>',
            '<h5 id="iwAddressField">Car Park</h5>'
        );
        infoWindow.setContent(newContent);
    });
}



function addInfoWindowToMarker(infoWindowList, infoWindow, contentString, map, marker) {
    infoWindowList.forEach(function (window) {
        window.close();

    });

    infoWindow.setContent(contentString);
    infoWindow.open(map, marker);

    //center the map onclick
    map.setCenter(marker.getPosition());
}

function toggleBounce() {
    if (marker.getAnimation() !== null) {
        marker.setAnimation(null);
    } else {
        marker.setAnimation(google.maps.Animation.BOUNCE);
    }
}