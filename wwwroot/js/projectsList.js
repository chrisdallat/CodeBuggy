const addNewProjectForm = document.getElementById('addNewProjectForm');
const addExistingProjectForm = document.getElementById('addExistingProjectForm');
const buttons = document.getElementById('ProjectButtons');
const goBackbutton = document.getElementById('goBackButton');
var popupTitle = document.getElementById('PopupTitle');


var defaultShowPopUp = function() {
    addNewProjectForm.style.display = 'none';
    addExistingProjectForm.style.display = 'none';
    ProjectButtons.style.display = 'block';
    goBackbutton.style.display = 'none';
    PopupTitle.innerHTML = "Add Project";
}

var togglePopup = function () {
    const popup = document.getElementById('popupOverlay');
    popup.style.display = popup.style.display === 'flex' ? 'none' : 'flex';

    if (popup.style.display === 'none') {
        let errorMessage = document.getElementById("errorMessage");
        if (errorMessage) {
            errorMessage.remove();
        }
        defaultShowPopUp();
    }
}

var toggleDeletePopup = function(button, projectAccessCode) {
    const popup = document.getElementById('deletePopup');
    popup.style.display = popup.style.display === 'flex' ? 'none' : 'flex';

    if (popup.style.display === 'none') {
        let errorMessage = document.getElementById("errorMessage");
        if (errorMessage) {
            errorMessage.remove();
        }
    }
}

var showExistingProject = function () {
    addExistingProjectForm.style.display = 'block';
    addNewProjectForm.style.display = 'none';
    ProjectButtons.style.display = 'none';
    goBackbutton.style.display = 'block';
    PopupTitle.innerHTML = "Add Exisiting Project";
}

var showCreateProject = function () {
    addExistingProjectForm.style.display = 'none';
    addNewProjectForm.style.display = 'block';
    ProjectButtons.style.display = 'none';
    goBackbutton.style.display = 'block';
    PopupTitle.innerHTML = "Add New Project";
}

var goBack = function () {
    let errorMessage = document.getElementById("errorMessage");
    if (errorMessage) {
        errorMessage.remove();
    }
    defaultShowPopUp();
}

var handleServerMessage = function (form, formData) {
    fetch(form.action, {
        method: 'POST',
        body: formData
    })
    .then(response => response.json())
    .then(data => {
        console.log(data);
        if (data.success === false) {
                let errorMessage = document.getElementById('errorMessage');
            if (!errorMessage) {
                errorMessage = document.createElement('span');
                errorMessage.id = "errorMessage";
                form.insertAdjacentElement('afterend', errorMessage);
            }
            errorMessage.innerHTML = data.message;
        }
        else {
            //window.location.href = '/Projects/ProjectsList?page=1';
        }
    })
    .catch(error => {
        console.error(error.message);
    });
}

document.addEventListener("DOMContentLoaded", function () {

    addNewProjectForm.addEventListener('submit', function (e) {
        e.preventDefault();
        handleServerMessage(this, new FormData(this));
    });

    addExistingProjectForm.addEventListener('submit', function (e) {
        e.preventDefault();
        handleServerMessage(this, new FormData(this));
    });

    document.getElementById('deleteProject').addEventListener('submit', function (e) {
        e.preventDefault();
        handleServerMessage(this, new FormData(this));
    });
});

var toggleBlur = function (span) {
    span.classList.toggle('blurred-text');

    setTimeout(function () {
        span.classList.toggle('blurred-text');
    }, 5000);
};

var copyText = function(button, accessCode) {
    let textToCopy = accessCode;

    let copiedAccessCode = button.nextElementSibling;
    if (copiedAccessCode) {
        copiedAccessCode.style.display = 'inline';
        setTimeout(function () {
            copiedAccessCode.style.display = 'none';
        }, 3000);
    }
    let tempTextarea = document.createElement('textarea');
    tempTextarea.value = textToCopy;

    document.body.appendChild(tempTextarea);

    tempTextarea.select();
    tempTextarea.setSelectionRange(0, 99999);

    document.execCommand('copy');

    document.body.removeChild(tempTextarea);
}