let drop;

let renderQuill = function (textBox, data) {
    let quill;
    if (!textBox.quill) {
        quill = new Quill(textBox, {
            theme: 'snow',
            placeholder: 'Description...',
            modules: {
                toolbar: [
                    ['bold', 'italic', 'underline'],
                    [{ 'header': 1 }, { 'header': 2 }],
                    [{ 'list': 'ordered' }, { 'list': 'bullet' }],
                    ['blockquote', 'code-block'],
                    [{ 'color': [] }],
                    ['clean']
                ]
            }
        });

        textBox.quill = quill; 
    } else {
        quill = textBox.quill;
    }

    const ticketDescriptionInputValue = textBox.nextElementSibling;

    if (data !== null || data !== "") {
        quill.root.innerHTML = data;
        ticketDescriptionInputValue.value = quill.root.innerHTML;
    }

    quill.on('text-change', function () {
        ticketDescriptionInputValue.value = quill.root.innerHTML;
    });
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
    if (popup.id === "confirmDeletePopup") {
        popup.style.display = "none";
        return;
    }
    try {
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
    } catch (e) {
        console.error(e);
    }
    
}

let getTicketStaus =  async function (ticketId) {
    let currentUrl = window.location.href;
    let params = new URLSearchParams(currentUrl.substring(currentUrl.indexOf('?')));
    let projectId = params.get('projectId');
    let ticketStatus = -1;

    await fetch(`/Projects/GetTicketStatus?projectId=${projectId}&ticketId=${ticketId}`, {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
        },
    })
    .then(response => response.json())
    .then(data => {
        if (data.success === true) {
            ticketStatus = data.ticketStatus;
        }
    })
    .catch(error => {
        console.error(error.message);
    });

    return ticketStatus;
}

var showTicket = async function (ticket) {
    const popup = document.getElementById("editTicketPopup");
    let popupTitle = popup.querySelector("#popupTitle");
    let ticketTitle = popup.querySelector("#ticketTitleInput").querySelector("input");
    let ticketPriorityDropdown = popup.querySelector("#ticketPriority");
    let ticketStatusDropdown = popup.querySelector("#ticketStatus");
    let ticketDescription = popup.querySelector("#ticketDescriptionInput");
    let ticketId = popup.querySelector("#changeTicketId");

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
    ticketId.value = ticket.Id;

    let i = 0;
    Array.from(ticketPriorityDropdown.querySelector("select")).forEach(option => {
        if (i === ticket.Priority) {
            ticketPriorityDropdown.querySelector('label[name="Input.TicketPriorityValue"]').textContent = option.value;
            option.selected = true;
        }
        i++;
    });

    i = 0;

    let ticketStatus = await getTicketStaus(ticket.Id);
    if (ticketStatus === -1) {
        ticketStatus = ticket.Status;
    }

    Array.from(ticketStatusDropdown.querySelector("select")).forEach(option => {
        if (i === ticketStatus) {
            ticketStatusDropdown.querySelector('label[name="Input.TicketStatusValue"]').textContent = option.value;
            option.selected = true;
        }
        i++;
    });

    let currentURL = window.location.href;
    let newURL = currentURL + "&ticket=" + ticket.StringId

    // KHALIL please don't forget to change that so it can load a popup with this ticket ID

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
            if (data.success === true) {
                console.log("Changed ticket status");
            }
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
            break;
        case "inProgressColumn":
            status = "InProgress";
            changeTicketStatus(ticketId, status);
            break;
        case "reviewColumn":
            status = "Review";
            changeTicketStatus(ticketId, status);
            break;
        case "doneColumn":
            status = "Done";
            changeTicketStatus(ticketId, status);
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

let handleServerMessageDeleteTicket = function(projectId, ticketId) {
    fetch(`/Projects/DeleteTicket?projectId=${projectId}&ticketId=${ticketId}`, {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
        },
    })
    .then(response => response.json())
    .then(data => {
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

// function fetchNotifications(projectId) {
//     fetch(`/Projects/GetNotifications?projectId=${projectId}`, {
//         method: 'POST',
//         headers: {
//             'Content-Type': 'application/json',
//         },
//     })
//     .then(response => response.json())
//     .then(data => {
//         console.log("Received data:", data);
//         var NotificationsHTML = `
//             <div id="infoLog-container" style="border: 1px solid #ccc; padding: 10px; border-radius: 5px; max-width: calc(100% - 20px); margin: 20px auto; font-family: 'Arial', sans-serif;">
//                 <div id="messages-container" style="max-height: 300px; overflow-y: auto; padding: 10px; border: 1px solid #eee; border-radius: 5px; background-color: #fff;">
//                     <ul id="messages" style="list-style: none; padding: 0; margin: 0;">
//                         ${data.map(notification => `<li>${notification.Timestamp}: ${notification.Message}</li>`).join('')}
//                     </ul>
//                 </div>
//                 <div style="display: flex; margin-top: 10px;">
//                     <input type="text" id="messageInput" style="flex-grow: 1; padding: 8px; margin-right: 5px; border: 1px solid #ccc; border-radius: 3px;" />
//                     <button id="sendButton" style="padding: 8px; background-color: #4CAF50; color: white; border: none; border-radius: 3px; cursor: pointer;">Send</button>
//                 </div>
//             </div>
//         `;

//         var placeholderContainer = document.getElementById("NotificationsPlaceholder");

//         placeholderContainer.innerHTML = NotificationsHTML;
//     })
//     .catch(error => console.error('Error fetching info log data:', error));
// }


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

    const editTicketForm = document.getElementById('editTicketForm');

    editTicketForm.addEventListener('submit', function (e) {
        e.preventDefault();
        handleServerMessage(this, new FormData(this));
    });

    const deleteButton = editTicketForm.querySelector("#deleteButton");

    deleteButton.addEventListener('click', function (e) {
        e.preventDefault();
        let ticketId;
        let projectId;
        let formData = new FormData(editTicketForm);

        for (var entry of formData.entries()) {
            console.log(entry[0], entry[1]);
            if (entry[0] === "ticketId") {
                ticketId = entry[1];
            }
            else if (entry[0] === "projectId") {
                projectId = entry[1];
            }
        }

        let confirmDeletePopup = document.querySelector("#confirmDeletePopup");
        confirmDeletePopup.querySelector(".popup").style.width = "600px";
        let popupTitle = confirmDeletePopup.querySelector("#popupTitle");
        popupTitle.innerHTML = "Confirm Deletion";
        let confirmProjectId = confirmDeletePopup.querySelector("input[name='projectId']");
        let confirmProjectTicketId = confirmDeletePopup.querySelector("input[name='ticketId']");

        confirmProjectId.value = projectId;
        confirmProjectTicketId.value = ticketId;

        confirmDeletePopup.style.display = "flex";

        let confirmButton = confirmDeletePopup.querySelector("#confirmDeleteButton");
        let cancelButton = confirmDeletePopup.querySelector("#cancelDeleteButton");
        confirmButton.addEventListener('click', function (e) {
            e.preventDefault();
            handleServerMessageDeleteTicket(projectId, ticketId);
        });

        cancelButton.addEventListener('click', function (e) {
            e.preventDefault();
            confirmDeletePopup.style.display = "none";
        });

    });

    const prioritySelects = document.querySelectorAll('select[name="Input.TicketPriorityValue"]');

    prioritySelects.forEach(function (selectElement) {

        const labelElement = selectElement.parentNode.querySelector('label[name="Input.TicketPriorityValue"]');

        selectElement.addEventListener('change', function () {
            updateLabel(selectElement, labelElement);
        });
    });

    const statusSelects = document.querySelectorAll('select[name="Input.TicketStatusValue"]');

    statusSelects.forEach(function (selectElement) {
        const labelElement = selectElement.parentNode.querySelector('label[name="Input.TicketStatusValue"]');
        selectElement.addEventListener('change', function () {
            updateLabel(selectElement, labelElement);
        });
    });
});

let fetchAndDisplayNotifications = function(projectId) {
    fetch(`/Projects/GetNotifications?projectId=${projectId}`, {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
        },
    })
    .then(response => response.json())
    .then(data => {
        console.log(data);
        var notificationsContainer = document.getElementById("notificationsContainer");
        if (notificationsContainer) {
            notificationsContainer.innerHTML = generateNotificationHTML(data);
            notificationsContainer.scrollTop = notificationsContainer.scrollHeight;
        }
        return data;
    })
    .catch(error => console.error('Error fetching notifications:', error));
}

let generateNotificationHTML = function(notifications) {
    return `<ul>${notifications.map(notification => `<li>${formatTimestamp(notification.timestamp)}: ${notification.message || 'No Message'}</li>`).join('')}</ul>`;
}

function formatTimestamp(timestamp) {
    const options = { day: '2-digit', month: '2-digit', year: 'numeric', hour: '2-digit', minute: '2-digit', hour12: false };
    const formattedDate = new Date(timestamp).toLocaleString('en-GB', options);
    return formattedDate;
}


