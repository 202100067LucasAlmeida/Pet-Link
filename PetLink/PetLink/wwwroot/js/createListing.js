// Main photo preview and validation
document.getElementById('mainPhoto').addEventListener('change', function (e) {
    const file = e.target.files[0];
    if (file) {
        const reader = new FileReader();
        reader.onload = function (event) {
            document.getElementById('mainPhotoImg').src = event.target.result;
            document.getElementById('mainPhotoPreview').style.display = 'block';
            document.getElementById('mainPhotoPlaceholder').style.display = 'none';
            document.getElementById('photoError').style.display = 'none';
            document.getElementById('mainPhotoArea').classList.remove('border-danger');
        }
        reader.readAsDataURL(file);
    }
});

function clearMainPhoto() {
    document.getElementById('mainPhoto').value = '';
    document.getElementById('mainPhotoPreview').style.display = 'none';
    document.getElementById('mainPhotoPlaceholder').style.display = 'block';
    document.getElementById('photoError').style.display = 'none';
}

// Real-time validation and feedback
function validateAge() {
    const ageInput = document.querySelector('input[name="AgeMonths"]');
    const ageValue = ageInput.value;
    const isValid = ageValue !== '' && parseInt(ageValue) >= 0;

    if (!isValid) {
        ageInput.classList.add('is-invalid');
    } else {
        ageInput.classList.remove('is-invalid');
    }
    return isValid;
}

function validatePhoto() {
    const mainPhoto = document.getElementById('mainPhoto');
    const isValid = mainPhoto.files && mainPhoto.files.length > 0;

    if (!isValid) {
        document.getElementById('photoError').style.display = 'block';
        document.getElementById('mainPhotoArea').classList.add('border-danger');
    } else {
        document.getElementById('photoError').style.display = 'none';
        document.getElementById('mainPhotoArea').classList.remove('border-danger');
    }
    return isValid;
}

function validateHealth() {
    const healthChecks = document.querySelectorAll('.health-check');
    let isValid = false;

    healthChecks.forEach(checkbox => {
        if (checkbox.checked) {
            isValid = true;
        }
    });

    const healthError = document.getElementById('healthError');
    if (!isValid) {
        healthError.style.display = 'block';
    } else {
        healthError.style.display = 'none';
    }
    return isValid;
}

// Add event listeners for real-time validation
document.querySelector('input[name="AgeMonths"]').addEventListener('input', function () {
    validateAge();
});

document.getElementById('mainPhoto').addEventListener('change', function () {
    validatePhoto();
});

document.querySelectorAll('.health-check').forEach(checkbox => {
    checkbox.addEventListener('change', function () {
        validateHealth();
    });
});

// Form submission validation
document.getElementById('createListingForm').addEventListener('submit', function (e) {
    const isAgeValid = validateAge();
    const isPhotoValid = validatePhoto();
    const isHealthValid = validateHealth();

    if (!isAgeValid || !isPhotoValid || !isHealthValid) {
        e.preventDefault();

        // Scroll to the first invalid field
        if (!isAgeValid) {
            document.querySelector('input[name="AgeMonths"]').focus();
            document.querySelector('input[name="AgeMonths"]').classList.add('is-invalid');
        } else if (!isPhotoValid) {
            document.getElementById('mainPhotoArea').scrollIntoView({ behavior: 'smooth', block: 'center' });
        } else if (!isHealthValid) {
            document.getElementById('healthError').scrollIntoView({ behavior: 'smooth', block: 'center' });
        }
    }
});

// Initial validation on page load
setTimeout(() => {
    validateAge();
    validatePhoto();
    validateHealth();
}, 100);