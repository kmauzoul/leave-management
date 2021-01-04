// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

//# is for id

$(document).ready(function () {
    $('#tblData').DataTable();
});


//. is for class
$(function () {
    $(".datepicker").datepicker({
        dateFormat: "mm-dd-yy"
        });
    });

