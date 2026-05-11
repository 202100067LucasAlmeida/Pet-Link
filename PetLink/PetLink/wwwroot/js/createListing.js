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

function validateProofs() {
    const healthChecks = document.querySelectorAll('.health-check');
    let isValid = true;

    healthChecks.forEach(checkbox => {
        if (checkbox.checked) {
            const proofDivId = checkbox.getAttribute('data-proof');
            const proofDiv = document.getElementById(proofDivId);
            const proofInput = proofDiv.querySelector('.proof-file');
            const proofErrorSpan = proofDiv.querySelector('.text-danger');

            if (!proofInput.files || proofInput.files.length === 0) {
                isValid = false;
                proofInput.classList.add('is-invalid');
                if (!proofErrorSpan || !proofErrorSpan.innerText.includes('Proof is required')) {
                    const errorSpan = proofErrorSpan || document.createElement('span');
                    errorSpan.className = 'text-danger small mt-1';
                    errorSpan.style.display = 'block';
                    errorSpan.innerText = 'Proof document is required';
                    if (!proofErrorSpan) proofDiv.appendChild(errorSpan);
                }
            } else {
                proofInput.classList.remove('is-invalid');
                if (proofErrorSpan && proofErrorSpan.innerText.includes('Proof is required')) {
                    proofErrorSpan.style.display = 'none';
                }
            }
        }
    });

    return isValid;
}

// Toggle da submissão de documento para cada checkbox ativa
function toggleProofUpload(checkbox) {
    const proofDivId = checkbox.getAttribute('data-proof');
    const proofDiv = document.getElementById(proofDivId);

    if (checkbox.checked) {
        proofDiv.style.display = 'block';
        // Add required attribute to file input
        const proofInput = proofDiv.querySelector('.proof-file');
        if (proofInput) proofInput.required = true;
    } else {
        proofDiv.style.display = 'none';
        // Remove required attribute and clear file input
        const proofInput = proofDiv.querySelector('.proof-file');
        if (proofInput) {
            proofInput.required = false;
            proofInput.value = ''; // Clear the file input
            proofInput.classList.remove('is-invalid');
            // Remove any error messages
            const errorSpan = proofDiv.querySelector('.text-danger');
            if (errorSpan && errorSpan.innerText.includes('Proof is required')) {
                errorSpan.style.display = 'none';
            }
        }
    }
}


document.querySelector('input[name="AgeMonths"]').addEventListener('input', function () {
    validateAge();
});

document.getElementById('mainPhoto').addEventListener('change', function () {
    validatePhoto();
});

document.querySelectorAll('.health-check').forEach(checkbox => {
    checkbox.addEventListener('change', function () {
        validateHealth();
        toggleProofUpload(this);
        validateProofs(); // Re-validate proofs when checkboxes change
    });
});

document.addEventListener('change', function (e) {
    if (e.target && e.target.classList && e.target.classList.contains('proof-file')) {
        validateProofs();
    }
});

// Submissão do formulário
document.getElementById('createListingForm').addEventListener('submit', function (e) {
    const isAgeValid = validateAge();
    const isPhotoValid = validatePhoto();
    const isHealthValid = validateHealth();
    const isProofsValid = validateProofs();

    if (!isAgeValid || !isPhotoValid || !isHealthValid || !isProofsValid) {
        e.preventDefault();

        if (!isAgeValid) {
            document.querySelector('input[name="AgeMonths"]').focus();
            document.querySelector('input[name="AgeMonths"]').classList.add('is-invalid');
        } else if (!isPhotoValid) {
            document.getElementById('mainPhotoArea').scrollIntoView({ behavior: 'smooth', block: 'center' });
        } else if (!isHealthValid) {
            document.getElementById('healthError').scrollIntoView({ behavior: 'smooth', block: 'center' });
        } else if (!isProofsValid) {
            document.querySelector('.proof-file.is-invalid').scrollIntoView({ behavior: 'smooth', block: 'center' });
        }
    }
});

// Initial validation on page load
setTimeout(() => {
    validateAge();
    validatePhoto();
    validateHealth();

    // Initialize proof upload visibility based on checked checkboxes
    document.querySelectorAll('.health-check').forEach(checkbox => {
        if (checkbox.checked) {
            toggleProofUpload(checkbox);
        }
    });
    validateProofs();
}, 100);