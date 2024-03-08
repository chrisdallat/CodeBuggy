let addNewProjectForm;
let addExistingProjectForm;
let buttons;
let goBackbutton;
let popupTitle;


var defaultShowPopUp = function() {
    addNewProjectForm.style.display = 'none';
    addExistingProjectForm.style.display = 'none';
    buttons.style.display = 'block';
    goBackbutton.style.display = 'none';
    popupTitle.innerHTML = "Add Project";
}

var showPopup = function (popupId) {
    const popup = document.getElementById(popupId);
    popup.style.display = 'flex';
}

var closePopup = function (popupId) {
    const popup = document.getElementById(popupId);
    popup.style.display = 'none';
    let errorMessage = document.getElementById("errorMessage");
    if (errorMessage) {
        errorMessage.remove();
    }
    defaultShowPopUp();
}

var toggleDeletePopup = function () {
    const popup = document.getElementById('deleteProjectPopup');
    popup.style.display = popup.style.display === 'flex' ? 'none' : 'flex';

    if (popup.style.display === 'none') {
        let errorMessage = document.getElementById("errorMessage");
        if (errorMessage) {
            errorMessage.remove();
        }
    }
}

var toggleInviteEmailPopup = function () {
    console.log("ToggleEmail");
    const popup = document.getElementById('inviteEmailPopup');
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
    buttons.style.display = 'none';
    goBackbutton.style.display = 'block';
    popupTitle.innerHTML = "Add Exisiting Project";
}

var showCreateProject = function () {
    addExistingProjectForm.style.display = 'none';
    addNewProjectForm.style.display = 'block';
    buttons.style.display = 'none';
    goBackbutton.style.display = 'block';
    popupTitle.innerHTML = "Add New Project";
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
        console.log("data: " + data);
    if (data.success === false) {
            let errorMessage = document.getElementById('errorMessage');
        if (!errorMessage) {
            errorMessage = document.createElement('span');
            errorMessage.id = "errorMessage";
            form.insertAdjacentElement('afterend', errorMessage);
        }
        console.log("err: " + data);
        errorMessage.innerHTML = data.message;
    }
    else {
        window.location.reload();
    }
})
.catch(error => {
    console.error(error.message);
});
}

document.addEventListener("DOMContentLoaded", function () {

    addNewProjectForm = document.getElementById('addNewProjectForm');
    addExistingProjectForm = document.getElementById('addExistingProjectForm');
    buttons = document.getElementById('projectButtons');
    goBackbutton = document.getElementById('goBackButton');
    popupTitle = document.getElementById('popupTitle');

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

    document.getElementById('inviteEmail').addEventListener('submit', function (e) {
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