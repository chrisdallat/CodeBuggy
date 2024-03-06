var drop;

var renderQuill = function (textBox, data) {
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

var getTicketStatus =  async function (ticketId) {
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

var loadComments = async function (projectId, ticketId) {

    let comments = undefined;
    await fetch(`/Projects/LoadComments?projectId=${projectId}&ticketId=${ticketId}`, {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
        },
    })
        .then(response => response.json())
        .then(data => {
            console.log(data);
            if (data.success === true) {
                comments = data.commentsData;
            }
        })
        .catch(error => {
            console.error(error.message);
        });

    return comments;
}

var showTicket = async function (ticket, assignedToUser) {
    const popup = document.getElementById("editTicketPopup");
    let popupTitle = popup.querySelector("#popupTitle");
    let ticketTitle = popup.querySelector("#ticketTitleInput").querySelector("input");
    let ticketPriorityDropdown = popup.querySelector("#ticketPriority");
    let ticketStatusDropdown = popup.querySelector("#ticketStatus");
    let ticketDescription = popup.querySelector("#ticketDescriptionInput");
    let ticketId = popup.querySelector("#changeTicketId");
    let projectId = popup.querySelector("input[name='projectId']").value;
    let assignMeButton = popup.querySelector("#assignToMeButton");
    let assigneeName = popup.querySelector("#assigneeName");
    let reporterName = popup.querySelector("#reporterName");

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

    let ticketStatus = await getTicketStatus(ticket.Id);
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

    assignedToUser === true ? assignMeButton.style.display = 'none' : assignMeButton.style.display = 'block';

    assigneeName.innerHTML = ticket.Assignee;
    reporterName.innerHTML = ticket.Reporter;

    let currentURL = window.location.href;
    let newURL = currentURL + "&ticket=" + ticket.StringId

    // KHALIL please don't forget to change that so it can load a popup with this ticket ID

    let comments = await loadComments(projectId, ticket.Id);
    let commentsBox = popup.querySelector("#existingCommentsBox");
    commentsBox.innerHTML = '';
    console.log(comments);
    if (comments !== undefined) {
        comments.forEach(comment => {
            // Create separator line
            const separator = document.createElement('hr');
            separator.classList.add('comment-separator');
            commentsBox.appendChild(separator);

            // Create comment container
            const commentContainer = document.createElement('div');
            commentContainer.classList.add('comment-container');

            // Create username element
            const userName = document.createElement('strong');
            userName.textContent = comment.username;

            // Format timestamp
            const timestampDate = new Date(comment.timestamp);
            const formattedDate = `${(timestampDate.getMonth() + 1).toString().padStart(2, '0')}/${timestampDate.getDate().toString().padStart(2, '0')}/${timestampDate.getFullYear()}`;
            const formattedTime = `${timestampDate.getHours().toString().padStart(2, '0')}:${timestampDate.getMinutes().toString().padStart(2, '0')}`;

            // Create timestamp element
            const timestamp = document.createElement('span');
            timestamp.textContent = `${formattedDate} ${formattedTime}`;
            timestamp.classList.add('timestamp');

            // Append username and timestamp to comment container
            commentContainer.appendChild(userName);
            commentContainer.appendChild(timestamp);

            // Create comment content
            const commentContent = document.createElement('div');
            commentContent.classList.add('comment-content');

             //Initialize Quill
            const quill = new Quill(commentContent, {
                theme: 'snow',
                modules: {
                    syntax: true, // Enable syntax highlighting module
                    toolbar: false, // Disable toolbar
                },
            });

            commentContent.classList.remove('ql-container');

            // Insert comment text into Quill editor
            quill.root.innerHTML = comment.text;

            // Append comment container to comments box
            commentsBox.appendChild(commentContainer);
            commentsBox.appendChild(commentContent);
        });
    }

    popup.style.display = 'flex';
}

var changeTicketStatus = async function (ticketId, newStatus) {
    let currentUrl = window.location.href;
    let params = new URLSearchParams(currentUrl.substring(currentUrl.indexOf('?')));
    let projectId = params.get('projectId');


    await fetch(`/Projects/ChangeTicketStatus?projectId=${projectId}&ticketId=${ticketId}&status=${newStatus}`, {
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

var allowDrop = function (event) {
    event.preventDefault();
}

var findNearestColumnId = function(element) {
    while (element) {
        if (element.classList.contains("column")) {
            return element;
        }
        element = element.parentElement;
    }
    return null; 
}

var dropAction = async function (event, ticket) {

    let dropzone = findNearestColumnId(event.target);
    let ticketId;
    ticketId = parseInt(ticket.firstChild.textContent, 10);

    if (isNaN(ticketId)) {
        return;
    }

    let status = undefined;
    switch (dropzone.id) {
        case "todoColumn":
            status = "ToDo";
            break;
        case "inProgressColumn":
            status = "InProgress";
            break;
        case "reviewColumn":
            status = "Review";
            break;
        case "doneColumn":
            status = "Done";
            break;
        default:
            break;
    }

    if (status === undefined) {
        console.error("Couldn't move ticket");
        return;
    }

    let dropzoneColumnContainer = dropzone.querySelector('.tickets-container');
    if (dropzoneColumnContainer) {
        dropzoneColumnContainer.appendChild(ticket.cloneNode(true));
        ticket.parentElement.removeChild(ticket);
    } else {
        console.error("Parent column not found");
    }

    await changeTicketStatus(ticketId, status);

}

var drag = function(dragEvent, ticket) {
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
            console.log(data)
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

var handleServerMessageDeleteTicket = function(projectId, ticketId) {

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

var updateLabel = function(selectElement, labelElement) {
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

    const editTicketForm = document.getElementById('editTicketForm');

    editTicketForm.addEventListener('submit', function (e) {
        e.preventDefault();
        handleServerMessage(this, new FormData(this));
    });

    const addCommentButton = editTicketForm.querySelector('#addCommentButton');

    addCommentButton.addEventListener('click', function (e) {
        e.preventDefault();
        let projectId = editTicketForm.querySelector("input[name='projectId']").value;
        let ticketId = editTicketForm.querySelector("input[name='ticketId']").value;
        addComment(projectId, ticketId, addCommentButton);
    })

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

var addCommentToTicket = async function (projectId, ticketId, text) {
    if (text === null || text === '<p><br></p>') return;

    await fetch(`/Projects/AddCommentToTicket?projectId=${projectId}&ticketId=${ticketId}&comment=${encodeURIComponent(text)}`, {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
        },
    })
    .then(response => response.json())
    .then(data => {
        console.log(data);
        if (data.success === true) {
            console.log("Changed ticket status");
        }
    })
    .catch(error => {
        console.error(error.message);
    });
}

var addComment = function (projectId, ticketId, button) {

    console.log(projectId, ticketId);
    let parent = button.parentNode;
    let addCommentTextBox = parent.querySelector("#addCommentTextBox");

    button.style.display = 'none';

    let buttonsContainer = parent.querySelector('#buttonsContainer');
    buttonsContainer.style.display = 'flex';

    let saveCommentButton = parent.querySelector("#saveCommentButton");
    let cancelCommentButton = parent.querySelector("#cancelCommentButton");

    saveCommentButton.addEventListener('click', async function (e) {
        e.preventDefault();
        let commentInput = parent.querySelector("input[name='commentInput']")
        commentInput.value = addCommentTextBox.quill.root.innerHTML;
        await addCommentToTicket(projectId, ticketId, commentInput.value);
    })

    cancelCommentButton.addEventListener('click', function (e) {
        e.preventDefault();
        addCommentTextBox.style.display = 'none';
        addCommentTextBox.previousSibling.style.display = 'none';
        buttonsContainer.style.display = 'none';
        button.style.display = 'flex';
    })

    if (!addCommentTextBox.quill) {
        const quill = new Quill(addCommentTextBox, {
            theme: 'snow',
            placeholder: 'Add comment...',
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
        addCommentTextBox.quill = quill;
    } else {
        addCommentTextBox.style.display = 'block';
        addCommentTextBox.previousSibling.style.display = 'block';
    }
;    

}

var fetchAndDisplayNotifications = function(projectId) {
    fetch(`/Projects/GetNotifications?projectId=${projectId}`, {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
        },
    })
    .then(response => response.json())
    .then(data => {
        var notificationContainer = document.getElementById("notificationContainer");
        if (notificationContainer) {
            notificationContainer.innerHTML = generateNotificationHTML(data);
            notificationContainer.scrollTop = notificationContainer.scrollHeight;
        }
        return data;
    })
    .catch(error => console.error('Error fetching notifications:', error));
}

var generateNotificationHTML = function(notifications) {
    return `<div class="notification-list">${notifications.map(notification => `<div class="notification-item">${formatTimestamp(notification.timestamp)}: ${notification.message || 'No Message'}</div>`).join('')}</div>`;
}

function formatTimestamp(timestamp) {
    const options = { day: '2-digit', month: '2-digit', year: 'numeric', hour: '2-digit', minute: '2-digit', hour12: false };
    const formattedDate = new Date(timestamp).toLocaleString('en-GB', options);
    return formattedDate;
}


