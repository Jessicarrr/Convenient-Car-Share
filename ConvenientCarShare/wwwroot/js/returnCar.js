
$("#manageModal").modal("hide");
function passToModal(spotId) {

    
    $("#spotId").val(spotId);
    $("#description").text("The car will be returned to Spot #" + spotId);

}


function initMap() {
    var options = {
        zoom: 15,
        center: { lat: -37.8136, lng: 144.9631 },

    };
    map = new google.maps.Map(document.getElementById('map'), options);
    
    /*var image = {
        url: '/images/car.png',

        size: new google.maps.Size(27, 39)
    };*/


    var infoWindowList = new Array();
    console.log(spotsArray);

    for ( let spot of spotsArray) {
        console.log(spot);

        // A marker with a custom PNG glyph.
        const imageElement = document.createElement("img");

        imageElement.src =
            "/images/car.png";

        const glyphPngPinElement = new google.maps.marker.PinElement({
            glyph: imageElement,
        });


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

        var marker = new google.maps.marker.AdvancedMarkerElement({
            position: {
                lat: spot.get("Latitude"),
                lng: spot.get("Longitude")
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

        /*google.maps.event.addListener(map, 'click', function () {
            infoWindowList.forEach(function (window) {
                window.close();

            });

        });*/

    }


}