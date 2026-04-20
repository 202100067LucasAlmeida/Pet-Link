function getAccurateLocation() {
    if ("geolocation" in navigator) {

        document.getElementById('ipDetails').innerHTML = `
            <div class="mt-3 p-3 rounded-4" style="background-color: var(--petlink-grey-bg);">
                <div class="text-center">
                    <i class="bi bi-hourglass-split text-primary fs-4"></i>
                    <p class="text-muted small mt-2">Requesting location permission...</p>
                </div>
            </div>
        `;

        navigator.geolocation.getCurrentPosition(

            async function (position) {
                const lat = position.coords.latitude;
                const lng = position.coords.longitude;

                // Reverse geocode to get city name from coordinates
                try {
                    const reverseGeo = await fetch(`https://nominatim.openstreetmap.org/reverse?format=json&lat=${lat}&lon=${lng}`);
                    const addressData = await reverseGeo.json();

                    const city = addressData.address?.city ||
                        addressData.address?.town ||
                        addressData.address?.village ||
                        addressData.address?.municipality ||
                        "Location found";

                    document.getElementById('ipDetails').innerHTML = `
                        <div class="mt-3 p-3 rounded-4" style="background-color: var(--petlink-grey-bg);">
                            <h6 class="fw-bold text-primary-darkblue mb-2">
                                <i class="bi bi-geo-alt-fill text-success me-2"></i>Your Accurate Location
                            </h6>
                            <div class="small">
                                <p class="mb-1"><strong>Coordinates:</strong> ${lat.toFixed(6)}, ${lng.toFixed(6)}</p>
                                <p class="mb-1"><strong>City:</strong> ${city}</p>
                            </div>
                            <button onclick="updateLocationField('${city}', ${lat}, ${lng})" class="btn btn-sm btn-primary rounded-pill mt-3">
                                <i class="bi bi-check-lg me-1"></i>Use This Location
                            </button>
                        </div>
                    `;

                } catch (error) {
                    console.error('Reverse geocoding error:', error);
                    document.getElementById('ipDetails').innerHTML = `
                        <div class="mt-3 p-3 rounded-4" style="background-color: var(--petlink-grey-bg);">
                            <h6 class="fw-bold text-primary-darkblue mb-2">
                                <i class="bi bi-geo-alt-fill text-success me-2"></i>Your Accurate Location
                            </h6>
                            <div class="small">
                                <p class="mb-1"><strong>Coordinates:</strong> ${lat.toFixed(6)}, ${lng.toFixed(6)}</p>
                                <p class="mb-0 text-warning"><strong>Note:</strong> Could not retrieve city name, but coordinates are available</p>
                            </div>
                            <button onclick="updateLocationField('', ${lat}, ${lng})" class="btn btn-sm btn-primary rounded-pill mt-3">
                                <i class="bi bi-check-lg me-1"></i>Use These Coordinates
                            </button>
                        </div>
                    `;
                }
            },

            // Error callback
            function (error) {
                let errorMessage = '';
                switch (error.code) {
                    case error.PERMISSION_DENIED:
                        errorMessage = 'You denied location permission. Enable it in your browser settings.';
                        break;
                    case error.POSITION_UNAVAILABLE:
                        errorMessage = 'Location information is unavailable.';
                        break;
                    case error.TIMEOUT:
                        errorMessage = 'Location request timed out.';
                        break;
                    default:
                        errorMessage = 'An error occurred while getting your location.';
                        break;
                }
                document.getElementById('ipDetails').innerHTML = `
                    <div class="mt-3 p-3 rounded-4" style="background-color: var(--petlink-grey-bg);">
                        <div class="text-center">
                            <i class="bi bi-exclamation-triangle text-warning fs-4"></i>
                            <p class="text-muted small mt-2">${errorMessage}</p>
                        </div>
                    </div>
                `;
            },

            // Options for better accuracy
            {
                enableHighAccuracy: true,
                timeout: 10000,
                maximumAge: 0
            }
        );
    } else {
        document.getElementById('ipDetails').innerHTML = `
            <div class="mt-3 p-3 rounded-4" style="background-color: var(--petlink-grey-bg);">
                <div class="text-center">
                    <i class="bi bi-browser-edge text-warning fs-4"></i>
                    <p class="text-muted small mt-2">Geolocation not supported by your browser.</p>
                </div>
            </div>
        `;
    }
}
function updateLocationField(city, lat, lng) {
    const locationInput = document.querySelector('input[name="City"]');
    if (locationInput) {
        if (city) {
            locationInput.value = city;
        } else if (lat && lng) {
            locationInput.value = `${lat.toFixed(4)}, ${lng.toFixed(4)}`;
        }

        locationInput.style.borderColor = '#28a745';
        locationInput.style.backgroundColor = '#f0fff4';
        setTimeout(() => {
            locationInput.style.borderColor = '';
            locationInput.style.backgroundColor = '';
        }, 2000);
    }

    const latInput = document.querySelector('input[name="Lat"]');
    const lonInput = document.querySelector('input[name="Lon"]');

    if (latInput && lonInput && lat && lng) {
        latInput.value = lat;
        lonInput.value = lng;
    }

    const ipDetailsDiv = document.getElementById('ipDetails');
    const successMessage = document.createElement('div');
    successMessage.className = 'alert alert-success alert-dismissible fade show mt-2';
    successMessage.innerHTML = `
        <i class="bi bi-check-circle-fill me-2"></i>
        Location updated successfully! Don't forget to save your changes.
        <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
    `;
    ipDetailsDiv.appendChild(successMessage);

    setTimeout(() => {
        if (successMessage) successMessage.remove();
    }, 3000);
}