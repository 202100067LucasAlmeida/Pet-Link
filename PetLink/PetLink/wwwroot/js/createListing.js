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
    // Documents are optional - always return true
    // Just show helpful messages but don't block submission
    const healthChecks = document.querySelectorAll('.health-check');

    healthChecks.forEach(checkbox => {
        if (checkbox.checked) {
            const proofDivId = checkbox.getAttribute('data-proof');
            const proofDiv = document.getElementById(proofDivId);
            const proofInput = proofDiv.querySelector('.proof-file');

            let proofErrorSpan = proofDiv.querySelector('.text-danger');
            if (!proofErrorSpan) {
                proofErrorSpan = document.createElement('div');
                proofErrorSpan.className = 'text-info small mt-1';
                proofDiv.appendChild(proofErrorSpan);
            }

            if (!proofInput.files || proofInput.files.length === 0) {
                proofErrorSpan.style.display = 'block';
                proofErrorSpan.innerHTML = '<i class="bi bi-info-circle"></i> Tip: Uploading proof documents helps admins verify your listing';
                proofErrorSpan.classList.remove('text-danger');
                proofErrorSpan.classList.add('text-info');
            } else {
                proofErrorSpan.style.display = 'none';
            }
        }
    });

    return true; // Always return true - documents are optional
}

// Toggle da submissão de documento para cada checkbox ativa
function toggleProofUpload(checkbox) {
    const proofDivId = checkbox.getAttribute('data-proof');
    const proofDiv = document.getElementById(proofDivId);

    if (checkbox.checked) {
        proofDiv.style.display = 'block';
        // Add required attribute to file input
        const proofInput = proofDiv.querySelector('.proof-file');
        if (proofInput) {
            proofInput.required = true;
            // Add visual indicator that document is required
            const label = proofDiv.querySelector('.form-label');
            if (label && !label.innerHTML.includes('*')) {
                label.innerHTML = label.innerHTML + '<span class="text-danger ms-1">*</span>';
            }
        }
    } else {
        proofDiv.style.display = 'none';
        // Remove required attribute and clear file input
        const proofInput = proofDiv.querySelector('.proof-file');
        if (proofInput) {
            proofInput.required = false;
            proofInput.value = ''; // Clear the file input
            proofInput.classList.remove('is-invalid');
            // Remove asterisk from label
            const label = proofDiv.querySelector('.form-label');
            if (label) {
                label.innerHTML = label.innerHTML.replace('<span class="text-danger ms-1">*</span>', '');
            }
            // Remove any error messages
            const errorSpan = proofDiv.querySelector('.text-danger');
            if (errorSpan) {
                errorSpan.style.display = 'none';
                errorSpan.innerHTML = '';
            }
        }
    }
}

// Add file preview functionality for proof documents
function addFilePreviewListeners() {
    const proofInputs = document.querySelectorAll('.proof-file');

    proofInputs.forEach(input => {
        input.addEventListener('change', function (e) {
            const proofDiv = this.closest('[id$="ProofDiv"]');
            const files = Array.from(this.files);

            // Create or update preview container
            let previewContainer = proofDiv.querySelector('.file-preview-container');
            if (!previewContainer) {
                previewContainer = document.createElement('div');
                previewContainer.className = 'file-preview-container mt-2';
                proofDiv.appendChild(previewContainer);
            }

            // Clear previous preview
            previewContainer.innerHTML = '';

            if (files.length > 0) {
                const badge = document.createElement('div');
                badge.className = 'alert alert-info alert-sm mb-0';
                badge.innerHTML = `<i class="bi bi-paperclip"></i> ${files.length} file(s) selected: ${files.map(f => f.name).join(', ')}`;
                previewContainer.appendChild(badge);
            }

            // Re-validate proofs when files are selected
            validateProofs();
        });
    });
}

// Call this function after DOM is ready
document.addEventListener('DOMContentLoaded', function () {
    addFilePreviewListeners();
});




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
    // Don't validate proofs as required, just show warnings
    validateProofs(); // This will show warnings but won't block submission

    if (!isAgeValid || !isPhotoValid || !isHealthValid) {
        e.preventDefault();

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

// Check if there's a validation error for main photo on page load
document.addEventListener('DOMContentLoaded', function () {
    addFilePreviewListeners();

    // Check if main photo error exists from server validation
    const mainPhotoError = document.getElementById('photoError');
    const mainPhotoInput = document.getElementById('mainPhoto');

    if (mainPhotoError && mainPhotoError.style.display !== 'none') {
        // Show a helpful message
        const errorMessage = document.createElement('div');
        errorMessage.className = 'alert alert-warning alert-sm mt-2';
        errorMessage.innerHTML = '<i class="bi bi-exclamation-triangle"></i> Please select a main photo again due to validation errors.';
        document.getElementById('mainPhotoArea').appendChild(errorMessage);
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