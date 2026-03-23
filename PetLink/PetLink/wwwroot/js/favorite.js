$(document).ready(function () {
    var petId = @Model.Id;

    // Verificar estado inicial
    $.ajax({
        url: '@Url.Action("Check", "Favorites")',
        type: 'GET',
        data: { animalListingId: petId },
        success: function (isFavorited) {
            if (isFavorited) {
                $('#favoriteIcon').removeClass('bi-heart').addClass('bi-heart-fill text-danger');
                $('#favoriteBtn').addClass('active');
            }
        }
    });

    // Clique no coração
    $('#favoriteBtn').click(function (e) {
        e.preventDefault();

        var icon = $('#favoriteIcon');

        $.ajax({
            url: '@Url.Action("Toggle", "Favorites")',
            type: 'POST',
            data: { animalListingId: petId },
            success: function (response) {
                if (response.success) {
                    if (response.isFavorited) {
                        icon.removeClass('bi-heart').addClass('bi-heart-fill text-danger');
                        showNotification('Added to favorites!');
                    } else {
                        icon.removeClass('bi-heart-fill text-danger').addClass('bi-heart');
                        showNotification('Removed from favorites!');
                    }
                }
            }
        });
    });

    function showNotification(message) {
        // Remove notificações anteriores
        $('.toast-notification').remove();

        var toast = $(`<div class="toast-notification">${message}</div>`);
        $('body').append(toast);
        setTimeout(() => toast.fadeOut(300, function () { $(this).remove(); }), 2000);
    }
});