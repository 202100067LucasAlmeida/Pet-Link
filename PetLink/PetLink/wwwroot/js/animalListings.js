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
    });

    // Handle age radio button clicks for visual feedback
    $('input[name="age"]').on('change', function () {
        // Remove bold from all labels
        $('.form-check-label').removeClass('fw-bold text-primary');

        // Add bold to selected label
        $(this).next('.form-check-label').addClass('fw-bold text-primary');
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
});

// Global function for removing filters
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
            // Update visual state for age filters
            $('.form-check-label').removeClass('fw-bold text-primary');
            $('input[name="age"]:checked').each(function () {
                $(this).next('.form-check-label').addClass('fw-bold text-primary');
            });
            break;
        case 'Location':
            $('input[name="location"]').val('');
            break;
        case 'Range':
            $('#rangeInput').val('0');
            $('#rangeValue').text('0 km');
            break;
    }

    // CRITICAL FIX: Preserve age filter visual state after range removal
    // Re-apply age filter visual styling to ensure it's maintained
    $('input[name="age"]:checked').each(function () {
        $(this).next('.form-check-label').addClass('fw-bold text-primary');
    });

    // Submit the form to refresh the results
    $('#filterForm').submit();
}