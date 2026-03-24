// animalListings.js

$(document).ready(function () {
    // Update range value display
    $('#rangeInput').on('input', function () {
        $('#rangeValue').text($(this).val() + ' km');
    });

    // Handle animal type button clicks for immediate visual feedback
    $('.animal-type-btn').on('click', function (e) {
        e.preventDefault();

        var btn = $(this);
        var radioId = btn.attr('for');
        var radio = $('#' + radioId);

        // Uncheck all other radios in the same group
        $('input[name="species"]').prop('checked', false);

        // Check this radio
        radio.prop('checked', true);

        // Update icons for all animal type buttons
        $('.animal-type-btn').each(function () {
            var currentBtn = $(this);
            var currentRadioId = currentBtn.attr('for');
            var currentRadio = $('#' + currentRadioId);
            var icon = currentBtn.find('i');

            if (currentRadio.prop('checked')) {
                icon.removeClass('bi-list-task').addClass('bi-check-circle-fill');
                currentBtn.addClass('active');
            } else {
                icon.removeClass('bi-check-circle-fill').addClass('bi-list-task');
                currentBtn.removeClass('active');
            }
        });

        // Optional: Auto-submit if you want instant filtering
        // $('#filterForm').submit();
    });

    // Handle age radio button clicks for visual feedback
    $('input[name="age"]').on('change', function () {
        // Remove bold from all labels
        $('.form-check-label').removeClass('fw-bold text-primary');

        // Add bold to selected label
        $(this).next('.form-check-label').addClass('fw-bold text-primary');

        // Optional: Auto-submit if you want instant filtering
        // $('#filterForm').submit();
    });

    // Initial load - set visual state for pre-selected filters
    initializeVisualState();

    function initializeVisualState() {
        // Set initial state for species buttons
        $('input[name="species"]').each(function () {
            var radio = $(this);
            var btn = $('label[for="' + radio.attr('id') + '"]');
            var icon = btn.find('i');

            if (radio.prop('checked')) {
                icon.removeClass('bi-list-task').addClass('bi-check-circle-fill');
                btn.addClass('active');
            } else {
                icon.removeClass('bi-check-circle-fill').addClass('bi-list-task');
                btn.removeClass('active');
            }
        });

        // Set initial state for age radios
        $('input[name="age"]:checked').each(function () {
            $(this).next('.form-check-label').addClass('fw-bold text-primary');
        });
    }

    // Verifica estado inicial de cada coração
    $('.btn-heart').each(function () {
        var button = $(this);
        var petId = button.data('pet-id');
        var icon = button.find('i');

        $.ajax({
            url: '/Favorites/Check',
            type: 'GET',
            data: { animalListingId: petId },
            success: function (isFavorited) {
                if (isFavorited) {
                    icon.removeClass('bi-heart').addClass('bi-heart-fill text-danger');
                    button.addClass('active');
                }
            },
            error: function () {
                console.log("Error checking favorite status");
            }
        });
    });

    // Handler para cliques nos corações 
    $(document).on('click', '.btn-heart', function (e) {
        e.preventDefault();
        e.stopPropagation();

        var button = $(this);
        var petId = button.data('pet-id');
        var icon = button.find('i');

        console.log("Coração clicado! Pet ID:", petId);

        $.ajax({
            url: '/Favorites/Toggle',
            type: 'POST',
            data: { animalListingId: petId },
            success: function (response) {
                console.log("Resposta:", response);
                if (response.success) {
                    if (response.isFavorited) {
                        icon.removeClass('bi-heart').addClass('bi-heart-fill text-danger');
                        button.addClass('active');
                        showNotification('Added to favorites!');
                    } else {
                        icon.removeClass('bi-heart-fill text-danger').addClass('bi-heart');
                        button.removeClass('active');
                        showNotification('Removed from favorites!');
                    }
                }
            },
            error: function (xhr, status, error) {
                console.log("Erro:", error);
                showNotification('Error updating favorite', 'error');
            }
        });
    });

    function showNotification(message, type = 'success') {
        // Remove notificações anteriores
        $('.toast-notification').remove();

        var toast = $(`<div class="toast-notification">${message}</div>`);
        $('body').append(toast);
        setTimeout(() => toast.fadeOut(300, function () { $(this).remove(); }), 2000);
    }
});

// Global function for removing filters
function removeFilter(element) {
    var filterKey = $(element).data('filter-key');
    var filterValue = $(element).data('filter-value');

    // Clear the corresponding filter in the form
    switch (filterKey) {
        case 'Species':
            $('input[name="species"][value="' + filterValue + '"]').prop('checked', false);
            // Update visual state
            $('.animal-type-btn').each(function () {
                var btn = $(this);
                var radioId = btn.attr('for');
                var radio = $('#' + radioId);
                var icon = btn.find('i');

                if (radio.prop('checked')) {
                    icon.removeClass('bi-list-task').addClass('bi-check-circle-fill');
                    btn.addClass('active');
                } else {
                    icon.removeClass('bi-check-circle-fill').addClass('bi-list-task');
                    btn.removeClass('active');
                }
            });
            break;
        case 'Age':
            $('input[name="age"][value="' + filterValue + '"]').prop('checked', false);
            // Reset, not as an adult for default if no age selected
            if ($('input[name="age"]:checked').length === 0) {
                $('#ageAdult').prop('checked', false);
            }
            // Update visual state
            $('.form-check-label').removeClass('fw-bold text-primary');
            $('input[name="age"]:checked').each(function () {
                $(this).next('.form-check-label').addClass('fw-bold text-primary');
            });
            break;
        case 'Location':
            $('input[name="location"]').val('');
            break;
        case 'Range':
            $('#rangeInput').val('50');
            $('#rangeValue').text('50 km');
            break;
    }

    // Submit the form to refresh the results
    $('#filterForm').submit();
}