let drop;


var togglePopup = function () {
    const popup = document.getElementById('createTicketPopup');
    popup.style.display = popup.style.display === 'flex' ? 'none' : 'flex';
    if (popup.style.display === 'none') {
        let errorMessage = document.getElementById("errorMessage");
        if (errorMessage) {
            errorMessage.remove();
        }
    }
}

let allowDrop = function (event) {
    event.preventDefault();
}

let dropAction = function (event, ticket) {

    let dropzone = event.target;

    let dropzoneColumnContainer = dropzone.closest('.column').getElementsByClassName('tickets-container')[0];
    if (dropzoneColumnContainer) {

        // Append a clone of the ticket to the new column
        dropzoneColumnContainer.appendChild(ticket.cloneNode(true));

        // Remove ticket from original column
        ticket.parentElement.removeChild(ticket);

    } else {
        console.error("Parent column not found");
    }

    let status;
    switch (dropzone.id) {
        case "todoColumn":
            status = "ToDo";
            console.log(status);
            break;
        case "inProgressColumn":
            status = "InProgress";
            console.log(status);
            break;
        case "doneColumn":
            status = "Done";
            console.log(status);
            break;
        default:
            break;
    }
}

let drag = function(dragEvent, ticket) {

    // This gets called when event drop gets dispatched
    drop = function (dropEvent) {
        dropEvent.preventDefault();
        try {
            dropAction(dropEvent, ticket);

        } catch (e) {
            console.error("Error: Could not find target");
        }
    }
}

var handleServerMessage = function (form, formData) {

    for (var entry of formData.entries()) {
        console.log(entry[0], entry[1]);
    }
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
            window.location.reload();
        }
    })
    .catch(error => {
        console.error(error.message);
    });

}
document.addEventListener('DOMContentLoaded', function () {

    populatePrioritiesDropdown();
    populateStatusDropdown();
    var dropdowns = document.querySelectorAll('.form-control');
    dropdowns.forEach(function (dropdown) {
        dropdown.addEventListener('click', function () {
            this.parentNode.classList.toggle('dropdown-open');
        });
    });

    const addTicketForm = document.getElementById('addTicketForm');
    addTicketForm.addEventListener('submit', function (e) {
        e.preventDefault();
        handleServerMessage(this, new FormData(this));
    });

});
