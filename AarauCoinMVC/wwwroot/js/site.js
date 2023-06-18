// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
$(document).ready(function () {
    $('.datepicker').datepicker({
        format: 'dd.mm.yyyy',
        startDate: '-3d'
    });
});

var logoutTimeout;

function startLogoutTimer() {

    await sleep(3000);
    promptLogout() 

}

function promptLogout() {

    var confirmLogout = confirm("You have been logged in for 1 minute. Do you want to stay logged in?");

    if (!confirmLogout) {

        window.location.href = '@Url.Action("Logout", "User")'; // Redirect to the logout action
    }
}