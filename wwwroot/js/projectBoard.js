let drop;

let renderQuill = function (textBox, data) {
    if (textBox.querySelector('.ql-editor') === null) {
        const quill = new Quill(textBox, {
            theme: 'snow',
            placeholder: 'Description...',
            modules: {
                toolbar: [
                    ['bold', 'italic', 'underline'],
                    [{ 'header': 1 }, { 'header': 2 }],
                    [{ 'list': 'ordered' }, { 'list': 'bullet' }],
                    ['blockquote', 'code-block'], // Include 'code-block' as a toolbar item
                    [{ 'color': [] }],
                    ['clean']
                ]
            }
        });

        const ticketDescriptionInputValue = textBox.nextElementSibling;

        if (data !== null || data !== "") {
            quill.root.innerHTML = data;
            ticketDescriptionInputValue.value = quill.root.innerHTML;
        }

        quill.on('text-change', function () {
            ticketDescriptionInputValue.value = quill.root.innerHTML;
        })
    }
   
}
var showPopup = function (popupId) {
    const popup = document.getElementById(popupId);
    let ticketDescription = popup.querySelector("#ticketDescriptionInput");

    if (ticketDescription !== undefined) {
        renderQuill(ticketDescription, "");
    }

    popup.style.display = 'flex';
}

var closePopup = function (popupId) {
    const popup = document.getElementById(popupId);
    let ticketTitle = popup.querySelector("#ticketTitleInput").querySelector("input");
    let ticketPriorityDropdown = popup.querySelector("#ticketPriority");
    let ticketStatusDropdown = popup.querySelector("#ticketStatus");
    let ticketDescription = popup.querySelector("#ticketDescriptionInput");

    ticketTitle.value = "";
    ticketDescription.value = "";
    ticketPriorityDropdown.querySelector('label[name="Input.TicketPriorityValue"]').textContent = "Priority";
    ticketStatusDropdown.querySelector('label[name="Input.TicketStatusValue"]').textContent = "Status";

    popup.style.display = 'none';
    let errorMessage = document.getElementById("errorMessage");
    if (errorMessage) {
        errorMessage.remove();
    }
}

var showTicket = function (ticket) {
    const popup = document.getElementById("editTicketPopup");
    let form = popup.querySelector("form");
    form.style.textAlign = "left";
    let popupTitle = popup.querySelector("#popupTitle");
    let ticketTitle = popup.querySelector("#ticketTitleInput").querySelector("input");
    let ticketPriorityDropdown = popup.querySelector("#ticketPriority");
    let ticketStatusDropdown = popup.querySelector("#ticketStatus");
    let ticketDescription = popup.querySelector("#ticketDescriptionInput");

    if (ticketDescription !== undefined) {
        if (ticket.Description !== undefined || ticket.Description !== "") {
            renderQuill(ticketDescription, ticket.Description);
        }
        else {
            renderQuill(ticketDescription, "");
        }
    }

    ticketTitle.value = ticket.Title;
    ticketDescription.value = ticket.Description;
    popupTitle.innerHTML = ticket.StringId;

    let i = 0;
    Array.from(ticketPriorityDropdown.querySelector("select")).forEach(option => {
        if (i === ticket.Priority) {
            ticketPriorityDropdown.querySelector('label[name="Input.TicketPriorityValue"]').textContent = option.value;
            option.selected = true;
        }
        i++;
    });

    i = 0;
    Array.from(ticketStatusDropdown.querySelector("select")).forEach(option => {
        if (i === ticket.Status) {
            ticketStatusDropdown.querySelector('label[name="Input.TicketStatusValue"]').textContent = option.value;
            option.selected = true;
        }
        i++;
    });

    popup.style.display = 'flex';
}

let changeTicketStatus = function (ticketId, newStatus) {
    let currentUrl = window.location.href;
    let params = new URLSearchParams(currentUrl.substring(currentUrl.indexOf('?')));
    let projectId = params.get('projectId');


    fetch(`/Projects/ChangeTicketStatus?projectId=${projectId}&ticketId=${ticketId}&status=${newStatus}`, {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
        },
    })
        .then(response => response.json())
        .then(data => {
            console.log(data);
            // Handle the response as needed
        })
        .catch(error => {
            console.error(error.message);
        });
}
let allowDrop = function (event) {
    event.preventDefault();
}

let dropAction = function (event, ticket) {

    let dropzone = event.target;
    let ticketId;
    ticketId = parseInt(ticket.firstChild.textContent, 10);

    if (isNaN(ticketId)) {
        return;
    }

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
            changeTicketStatus(ticketId, status);
            console.log(status);
            break;
        case "inProgressColumn":
            status = "InProgress";
            changeTicketStatus(ticketId, status);
            console.log(status);
            break;
        case "reviewColumn":
            status = "Review";
            changeTicketStatus(ticketId, status);
            console.log(status);
            break;
        case "doneColumn":
            status = "Done";
            changeTicketStatus(ticketId, status);
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

let updateLabel = function(selectElement, labelElement) {
    const selectedOption = selectElement.options[selectElement.selectedIndex];
    labelElement.textContent = selectedOption.textContent;
}

document.addEventListener('DOMContentLoaded', function () {

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

    // Get all priority select elements
    const prioritySelects = document.querySelectorAll('select[name="Input.TicketPriorityValue"]');

    // Loop through each select element
    prioritySelects.forEach(function (selectElement) {
        // Get the corresponding label element
        const labelElement = selectElement.parentNode.querySelector('label[name="Input.TicketPriorityValue"]');

        // Add event listener to the select element
        selectElement.addEventListener('change', function () {
            // Update label text content
            updateLabel(selectElement, labelElement);
        });
    });

    // Get all status select elements
    const statusSelects = document.querySelectorAll('select[name="Input.TicketStatusValue"]');

    // Loop through each status select element
    statusSelects.forEach(function (selectElement) {
        // Get the corresponding label element
        const labelElement = selectElement.parentNode.querySelector('label[name="Input.TicketStatusValue"]');

        // Add event listener to the status select element
        selectElement.addEventListener('change', function () {
            // Update status label text content
            updateLabel(selectElement, labelElement);
        });
    });

});


