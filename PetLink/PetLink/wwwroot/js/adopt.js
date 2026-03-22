document.addEventListener('DOMContentLoaded', function () {
    console.log("adopt.js loaded successfully");

    // Get elements
    const adoptBtn = document.getElementById('adoptBtn');
    const modal = document.getElementById('adoptModal');
    const confirmBtn = document.querySelector('.adopt-confirm-btn');
    const cancelBtn = document.querySelector('.adopt-cancel-btn');
    const checkboxes = document.querySelectorAll('.adopt-checkbox');
    const successNotification = document.getElementById('adoptSuccessNotification');

    // Check if adopt button exists
    if (!adoptBtn) {
        console.error("Adopt button not found!");
        return;
    }

    // Handle adopt button click
    adoptBtn.addEventListener('click', function (e) {
        e.preventDefault();
        console.log("Adopt button clicked");

        // Reset checkboxes
        checkboxes.forEach(checkbox => {
            checkbox.checked = false;
        });

        // Disable confirm button
        if (confirmBtn) {
            confirmBtn.disabled = true;
        }

        // Show modal
        if (modal) {
            modal.style.display = 'flex';
        }
    });

    // Handle checkbox changes
    if (checkboxes.length > 0) {
        checkboxes.forEach(checkbox => {
            checkbox.addEventListener('change', function () {
                const allChecked = Array.from(checkboxes).every(cb => cb.checked);
                if (confirmBtn) {
                    confirmBtn.disabled = !allChecked;
                }
            });
        });
    }

    // Handle confirm button
    if (confirmBtn) {
        confirmBtn.addEventListener('click', function () {
            if (!confirmBtn.disabled) {
                closeModal();
                showSuccess();
            }
        });
    }

    // Handle cancel button
    if (cancelBtn) {
        cancelBtn.addEventListener('click', function () {
            closeModal();
        });
    }

    // Close modal when clicking outside
    if (modal) {
        modal.addEventListener('click', function (e) {
            if (e.target === modal) {
                closeModal();
            }
        });
    }

    function closeModal() {
        if (modal) {
            modal.style.display = 'none';
        }
    }

    function showSuccess() {
        if (successNotification) {
            successNotification.style.display = 'block';
            setTimeout(function () {
                successNotification.style.display = 'none';
            }, 3000);
        }
    }
});