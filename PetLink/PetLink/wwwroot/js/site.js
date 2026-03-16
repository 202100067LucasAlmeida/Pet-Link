// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

$(document).ready(function () {
    // Real-time password validation
    $('#password').on('input', function () {
        var password = $(this).val();

        $.ajax({
            url: '@Url.Action("ValidatePassword", "Profile")',
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify({ password: password }),
            success: function (requirements) {
                updatePasswordRequirement('length', requirements.length);
                updatePasswordRequirement('lowercase', requirements.lowercase);
                updatePasswordRequirement('uppercase', requirements.uppercase);
                updatePasswordRequirement('number', requirements.number);
                updatePasswordRequirement('symbol', requirements.symbol);
            }
        });
    });

    // Form submission with AJAX validation
    $('#signupForm').on('submit', function (e) {
        e.preventDefault();

        var formData = {
            FullName: $('#fullName').val(),
            Email: $('#email').val(),
            Phone: $('#phone').val(),
            Password: $('#password').val(),
            ConfirmPassword: $('#confirmPassword').val(),
            UserType: $('input[name="userType"]:checked').val()
        };

        // Clear previous errors
        clearErrors();

        $.ajax({
            url: '@Url.Action("ValidateSignUp", "Profile")',
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify(formData),
            success: function (response) {
                if (response.success) {
                    // If validation passes, submit the form normally
                    $('#signupForm')[0].submit();
                } else {
                    // Display field-specific errors
                    displayErrors(response.errors);
                }
            },
            error: function () {
                showGeneralError('An error occurred. Please try again.');
            }
        });
    });

    // Real-time field validation on blur
    $('#fullName, #email, #phone, #password, #confirmPassword').on('blur', function () {
        validateField($(this).attr('id'));
    });
});

function updatePasswordRequirement(requirement, isValid) {
    var icon = $('#' + requirement + 'Icon');
    var text = $('#' + requirement + 'Text');

    if (isValid) {
        icon.removeClass('bi bi-circle').addClass('bi bi-check-circle-fill text-success');
        text.css('color', '#28a745');
    } else {
        icon.removeClass('bi bi-check-circle-fill text-success').addClass('bi bi-circle');
        text.css('color', '');
    }
}

function validateField(fieldId) {
    var field = $('#' + fieldId);
    var value = field.val();

    // Simple client-side validation before server call
    if (fieldId === 'fullName' && value && !/^[\p{L}\s\-']+$/u.test(value)) {
        showFieldError('fullName', 'O nome não deve ter números nem símbolos.');
    } else {
        hideFieldError(fieldId);
    }
}

function displayErrors(errors) {
    for (var field in errors) {
        showFieldError(field, errors[field]);
    }
}

function showFieldError(fieldId, message) {
    var field = $('#' + fieldId);
    field.addClass('is-invalid');
    $('#' + fieldId + 'Error').text(message).show();
}

function hideFieldError(fieldId) {
    var field = $('#' + fieldId);
    field.removeClass('is-invalid');
    $('#' + fieldId + 'Error').text('').hide();
}

function clearErrors() {
    $('.is-invalid').removeClass('is-invalid');
    $('.invalid-feedback').text('').hide();
    $('#errorContainer').hide();
}

function showGeneralError(message) {
    $('#errorMessage').text(message);
    $('#errorContainer').show();
}
