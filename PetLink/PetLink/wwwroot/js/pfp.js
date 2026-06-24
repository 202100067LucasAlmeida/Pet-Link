function previewImage(event) {
    var reader = new FileReader();
    reader.onload = function () {
        var output = document.getElementById('profilePreview');
        var icon = document.getElementById('defaultAvatarIcon');
        var removeFlag = document.getElementById('removePhotoFlag');

        output.src = reader.result;
        output.classList.remove('d-none');
        output.style.display = 'block';
        icon.classList.add('d-none');
        removeFlag.value = "false";
    }
    if (event.target.files[0]) {
        reader.readAsDataURL(event.target.files[0]);
    }
}

function handleRemovePhoto() {
    document.getElementById('profilePictureInput').value = "";
    var output = document.getElementById('profilePreview');
    var icon = document.getElementById('defaultAvatarIcon');
    output.src = "";
    output.classList.add('d-none');
    icon.classList.remove('d-none');
    document.getElementById('removePhotoFlag').value = "true";
}