// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

//Delete User confirm


$("#manageModal").modal("hide");

function passToModal(customer) {

    console.log(customer);
    console.log(customer.id);
    $("#userID").val(customer.id);
    //$("#userData").text(JSON.stringify(customer));
    $("#userid").text("id: "+JSON.stringify(customer.id));
    $("#userName").text("Name: " +JSON.stringify(customer.name));
    $("#userEmail").text("Email: " + JSON.stringify(customer.email));

    $("#warningMessageEmail").val(customer.email);

    if (customer.warnned === false) {
        $("#blockbtn").attr("disabled", "true");
    }
    else {

        $("#blockbtn").removeAttr("disabled");

    }

    if (customer.lockoutEnd === null) {

        $("#blockbtn").text("Block");

    }
    else {
        $("#blockbtn").text("Un-Block");
    }
    var func = "blockUser('" + customer.id + "')";
    $("#blockbtn").attr("onclick", func);

}





document.getElementById("manageForm").onsubmit = function onSubmit(form) {

    if (confirm("Are you sure you want to delete this user ? "))
        return true;
    else
        return false;
}



//Ajax Block the user
function blockUser(userID) {
    $.ajax({
        type: "POST",

        url: 'ManageUsers/BlockUser',

        data: { userID: userID },

        datatype: "JSON",

        //function that before send
        //beforeSend: function () { $("#msg").html("logining"); },

        success: function (data) {

            
            status = document.getElementById("blockbtn").innerText;
            switch (status) {
                case "Block":
                    document.getElementById("blockbtn").innerText = "Un-Block";
                    break;
                case "Un-Block":
                    document.getElementById("blockbtn").innerText = "Block";
                    $("#blockbtn").attr("disabled", "true");

                    break;
                default:
                    break;
            }
            alert(data);
        },
    });
}

$(function () {
    $('#manageModal').on('hide.bs.modal',
        function () {
            window.location.reload();
        })
});

