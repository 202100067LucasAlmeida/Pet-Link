// site.js - PetLink 

$(document).ready(function () {

    // --- 1. INTERFACE: MOSTRAR/ESCONDER PASSWORD ---
    function setupPasswordToggle(buttonId, inputId, iconId) {
        $(buttonId).on('click', function () {
            var pwdInput = $(inputId);
            var icon = $(iconId);
            if (pwdInput.attr('type') === 'password') {
                pwdInput.attr('type', 'text');
                icon.removeClass('bi-eye').addClass('bi-eye-slash');
            } else {
                pwdInput.attr('type', 'password');
                icon.removeClass('bi-eye-slash').addClass('bi-eye');
            }
        });
    }

    setupPasswordToggle('#togglePassword', '#password', '#togglePasswordIcon');
    setupPasswordToggle('#toggleConfirmPassword', '#confirmPassword', '#toggleConfirmPasswordIcon');

    // --- 2. VALIDAÇÃO EM TEMPO REAL (PASSWORD) ---
    $('#password').on('input', function () {
        var password = $(this).val();
        $.ajax({
            url: '/Profile/ValidatePassword',
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

    // Validação ao sair do campo (blur)
    $('#fullName, #email, #phone, #password, #confirmPassword').on('blur', function () {
        validateField($(this).attr('id'));
    });

    // --- 3. SUBMISSÃO DO FORMULÁRIO (AJAX) ---
    $('#signupForm').on('submit', function (e) {
        e.preventDefault();
        clearErrors();

        var isValid = true;

        // Validações de Cliente (Rápidas)
        var fullName = $('#fullName').val().trim();
        if (!fullName) { showFieldError('fullName', 'Full Name is required.'); isValid = false; }

        var email = $('#email').val().trim();
        if (!email || !email.includes('@')) { showFieldError('email', 'A valid email is required.'); isValid = false; }

        var finalPhone = window.phoneInput ? window.phoneInput.getNumber() : $('#phone').val();
        if (!finalPhone) { showFieldError('phone', 'Phone Number is required.'); isValid = false; }

        var password = $('#password').val();
        if (password.length < 6) { showFieldError('password', 'Password must meet all requirements.'); isValid = false; }

        if (password !== $('#confirmPassword').val()) { showFieldError('confirmPassword', 'Passwords do not match.'); isValid = false; }

        if (!$('#terms').is(':checked')) { showFieldError('terms', 'You must agree to the terms.'); isValid = false; }

        if (!isValid) {
            showGeneralError('Please fix the errors below.');
            return;
        }

        // Estado de carregamento no botão
        var submitBtn = $('#submitBtn');
        var originalBtnText = submitBtn.text();
        submitBtn.html('<span class="spinner-border spinner-border-sm me-2"></span>Creating...').prop('disabled', true);

        var formData = {
            FullName: fullName,
            Email: email,
            Phone: finalPhone,
            Password: password,
            ConfirmPassword: $('#confirmPassword').val(),
            UserType: $('input[name="userType"]:checked').val()
        };

        $.ajax({
            url: '/Profile/ValidateSignUp',
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify(formData),
            success: function (response) {
                if (response.success) {
                    // Feedback de Sucesso Inline (Barra Verde)
                    submitBtn.html('<i class="bi bi-check2"></i> Created').addClass('btn-success').prop('disabled', true);
                    showGeneralSuccess('Welcome! Your account has been successfully created.');
                    $('#signupForm')[0].reset();
                } else {
                    submitBtn.html(originalBtnText).prop('disabled', false);
                    displayErrors(response.errors);
                    showGeneralError('Verification failed. Please check the fields.');
                }
            },
            error: function () {
                submitBtn.html(originalBtnText).prop('disabled', false);
                showGeneralError('Connection error. Please try again later.');
            }
        });
    });
});

$('#loginForm').on('submit', function (e) {
    e.preventDefault();
    clearErrors();

    var email = $('#email').val().trim();
    var password = $('#password').val();
    var rememberMe = $('#rememberMe').is(':checked');

    // Validação básica local
    if (!email) { showFieldError('email', 'Email is required.'); return; }
    if (!password) { showFieldError('password', 'Password is required.'); return; }

    var submitBtn = $('#submitBtn');
    var originalText = submitBtn.text();
    submitBtn.html('<span class="spinner-border spinner-border-sm"></span> Logging in...').prop('disabled', true);

    $.ajax({
        url: '/Profile/LoginForm',
        type: 'POST',
        data: {
            email: email,
            password: password,
            rememberMe: rememberMe
        },
        success: function (response) {
            if (response.success) {
                // Se o login deu certo, manda para a Home
                window.location.href = '/Home/Index';
            } else {
                submitBtn.html(originalText).prop('disabled', false);
                showGeneralError(response.message);
            }
        },
        error: function () {
            submitBtn.html(originalText).prop('disabled', false);
            showGeneralError('An error occurred. Please try again.');
        }
    });
});

// --- 4. FUNÇÕES AUXILIARES DE VALIDAÇÃO ---

function updatePasswordRequirement(req, isValid) {
    var icon = $('#' + req + 'Icon');
    var text = $('#' + req + 'Text');
    if (isValid) {
        icon.removeClass('bi-circle').addClass('bi-check-circle-fill text-success');
        text.css('color', '#28a745');
    } else {
        icon.removeClass('bi-check-circle-fill text-success').addClass('bi-circle');
        text.css('color', '');
    }
}

function validateField(fieldId) {
    var val = $('#' + fieldId).val();
    if (fieldId === 'fullName' && val && !/^[\p{L}\s\-']+$/u.test(val)) {
        showFieldError('fullName', 'Invalid characters in name.');
    } else if (fieldId === 'email' && val && (!val.includes('@') || !/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(val))) {
        showFieldError('email', 'Enter a valid email.');
    } else {
        hideFieldError(fieldId);
    }
}

function displayErrors(errors) {
    for (var field in errors) { showFieldError(field, errors[field]); }
}

function showFieldError(id, msg) {
    $('#' + id).addClass('is-invalid');
    $('#' + id + 'Error').text(msg).show();
}

function hideFieldError(id) {
    $('#' + id).removeClass('is-invalid');
    $('#' + id + 'Error').hide();
}

function clearErrors() {
    $('.is-invalid').removeClass('is-invalid');
    $('.invalid-feedback').hide();
    $('#errorContainer, #successContainer').hide();
}

function showGeneralError(msg) {
    $('#errorMessage').text(msg);
    $('#errorContainer').show();
}

function showGeneralSuccess(msg) {
    $('#successMessage').text(msg);
    $('#successContainer').show();
    $('#errorContainer').hide();
    $('html, body').animate({ scrollTop: 0 }, 'fast');
}


//ações da side bar, pra personalizar o css
document.addEventListener('DOMContentLoaded', function () {
    const currentUrl = window.location.pathname.toLowerCase();
    const navItems = document.querySelectorAll('.sidebar-profile .list-group-item');

    navItems.forEach(item => {
        // Remove active class from all items first
        item.classList.remove('active');

        // Get the href attribute (which is generated from asp-controller/asp-action)
        const href = item.getAttribute('href');

        // Check if this item's href matches current URL
        // Skip items with href="#"
        if (href && href !== '#' && href !== '') {
            // For exact match or if current URL ends with the href
            if (currentUrl === href.toLowerCase() ||
                currentUrl.endsWith(href.toLowerCase()) ||
                // Check if it's the home/index page
                (href.toLowerCase() === '/' && currentUrl === '/') ||
                (href.toLowerCase() === '' && currentUrl === '/')) {
                item.classList.add('active');
            }

            // For controller/action matching (more flexible)
            const controllerAction = href.toLowerCase().split('?')[0]; // Remove query strings
            if (currentUrl.includes(controllerAction) && controllerAction !== '/') {
                item.classList.add('active');
            }
        }
    });
});