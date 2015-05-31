$(document).ready(function () {

    $('#content-wrapper').on('click', '#books-list li', function () {
        $.ajax({
            url: "/Home/Order",
            type: "post",
            data: JSON.stringify({
                id: $(this).attr('id')
            }),
            dataType: "html",
            contentType: "application/json",
            success: function (content) {
                $('#content-wrapper').html(content);
            },
            error: function () {
                alert("Error requesting order");
            }
        });
    });

    $("#content-wrapper").on('change', '#books', function () {
        var selected = $("#books")[0].selectedIndex + 1;
        var value = $("#books option:nth-child(" + selected + ")").val();

        $("#book_id").attr("value", value);
    });

});