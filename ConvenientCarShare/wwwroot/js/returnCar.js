
$("#manageModal").modal("hide");
function passToModal(spotId) {

    
    $("#spotId").val(spotId);
    $("#description").text("The car will be returned to Spot #" + spotId);

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
    
    var image = {
        url: '/images/car.png',

        size: new google.maps.Size(27, 39)
    };


    var infoWindowList = new Array();
    console.log(spotsArray);
    for ( let spot of spotsArray) {
        console.log(spot);

        var infoWindow = new google.maps.InfoWindow(
            {
                content:
                    "<p>This parking is availiable now.</p>" + 
                    "<button class= 'btn btn-info'data-toggle='modal'data-target='#manageModal' " +
                    "onclick = 'passToModal(" + spot.get("id")
                    + ")'>"
                    +
                    "Return here</button>"

            });

        infoWindowList.push(infoWindow);

        var marker = new google.maps.Marker({
            position: {
                lat: spot.get("Latitude"),
                lng: spot.get("Longitude")
            },
            map: map,
            draggable: false,
            icon: image,
            animation: google.maps.Animation.DROP,
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

        google.maps.event.addListener(map, 'click', function () {
            infoWindowList.forEach(function (window) {
                window.close();

            });

        });

    }


}