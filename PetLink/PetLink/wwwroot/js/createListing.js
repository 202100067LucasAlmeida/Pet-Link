// verificação das vacinas e foto
document.getElementById('createListingForm').addEventListener('submit', function (e) {
    let isValid = true;

    // pelo menos umas das três o animal vai ter de ter
    const healthChecks = document.querySelectorAll('.health-check');
    let atLeastOneChecked = false;

    healthChecks.forEach(checkbox => {
        if (checkbox.checked) {
            atLeastOneChecked = true;
        }
    });

    const healthError = document.getElementById('healthError');
    if (!atLeastOneChecked) {
        healthError.style.display = 'block';
        isValid = false;
    } else {
        healthError.style.display = 'none';
    }

    // temos uma foto?
    const mainPhoto = document.getElementById('mainPhoto');
    if (!mainPhoto.files || mainPhoto.files.length === 0) {
        mainPhoto.classList.add('is-invalid');
        isValid = false;
    } else {
        mainPhoto.classList.remove('is-invalid');
    }

    // verificar a idade
    const ageInput = document.querySelector('input[name="AgeMonths"]');
    if (!ageInput.value || parseInt(ageInput.value) < 0) {
        ageInput.classList.add('is-invalid');
        isValid = false;
    } else {
        ageInput.classList.remove('is-invalid');
    }

    if (!isValid) {
        e.preventDefault();
        // um extra
        const firstError = document.querySelector('.is-invalid, #healthError');
        if (firstError) {
            firstError.scrollIntoView({ behavior: 'smooth', block: 'center' });
        }
    }
});

// atualizar quando já temos uma foto
document.getElementById('mainPhoto').addEventListener('change', function () {
    if (this.files && this.files.length > 0) {
        this.classList.remove('is-invalid');
    }
});

// atualizar qwuando já temos idade
document.querySelector('input[name="AgeMonths"]').addEventListener('input', function () {
    if (this.value && parseInt(this.value) >= 0) {
        this.classList.remove('is-invalid');
    }
});

// atualizar as vacinas
document.querySelectorAll('.health-check').forEach(checkbox => {
    checkbox.addEventListener('change', function () {
        let atLeastOneChecked = false;
        document.querySelectorAll('.health-check').forEach(cb => {
            if (cb.checked) atLeastOneChecked = true;
        });
        if (atLeastOneChecked) {
            document.getElementById('healthError').style.display = 'none';
        }
    });
});