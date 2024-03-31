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

function hasDisabledOption(dropdown) {
    for (var i = 0; i < dropdown.options.length; i++) {
        if (dropdown.options[i].disabled) {
            return true;
        }
    }
    return false;
}

function addDisabledOption(dropdown, labelText) {
    if (dropdown.options.length > 0 && dropdown.options[0].disabled) {
        return;
    }
    var disabledOption = document.createElement('option');
    disabledOption.disabled = true;
    disabledOption.selected = true;
    disabledOption.value = "";
    disabledOption.textContent = labelText;

    if (dropdown.options.length > 0) {
        dropdown.insertBefore(disabledOption, dropdown.options[0]);
    } else {
        dropdown.appendChild(disabledOption);
    }
}

var showPopup = function (popupId) {
    const popup = document.getElementById(popupId);
    let ticketDescription = popup.querySelector("#ticketDescriptionInput");
    let ticketPriorityDropdown = popup.querySelector("#prioritySelect");
    let ticketStatusDropdown = popup.querySelector("#statusSelect");

    if (ticketDescription !== undefined) {
        renderQuill(ticketDescription, "");
    }
    
    addDisabledOption(ticketPriorityDropdown, "Priority"); 
    addDisabledOption(ticketStatusDropdown, "Status");

    popup.addEventListener('click', closePopupOutside = function (event) {
        if (!popup.querySelector('.popup').contains(event.target)) {
            closePopup(popupId);
            document.removeEventListener('click', closePopupOutside);
        }
    });

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
        let ticketPrioritySelect = ticketPriorityDropdown.querySelector('#prioritySelect');
        let ticketStatusSelect = ticketStatusDropdown.querySelector('#statusSelect');

        ticketTitle.value = "";
        ticketDescription.value = "";
        ticketPrioritySelect.value = ticketPrioritySelect.options[0].value;
        ticketStatusSelect.value = ticketStatusSelect.options[0].value;
        ticketPriorityDropdown.querySelector('label[name="Input.TicketPriorityValue"]').textContent = "Priority";
        ticketStatusDropdown.querySelector('label[name="Input.TicketStatusValue"]').textContent = "Status";

        popup.style.display = 'none';
        let errorMessage = document.getElementById("errorMessage");
        if (errorMessage) {
            errorMessage.remove();
        }

        var currentURL = new URL(window.location.href);
        if (currentURL.searchParams.has('ticket')) {
            currentURL.searchParams.delete('ticket');
            history.pushState({}, '', currentURL.href);
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

var GetComments = async function (projectId, ticketId) {

    let comments = undefined;
    await fetch(`/Projects/GetComments?projectId=${projectId}&ticketId=${ticketId}`, {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
        },
    })
        .then(response => response.json())
        .then(data => {
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

    assignMeButton.addEventListener('click', async function (e) {
        e.preventDefault();
        await assignTicketToUser(projectId, ticket.Id);
    })

    assigneeName.innerHTML = ticket.Assignee;
    reporterName.innerHTML = ticket.Reporter;

    var currentURL = new URL(window.location.href);
    if (currentURL.searchParams.has('ticket')) {
        currentURL.searchParams.set('ticket', ticket.StringId);
    } else {
        currentURL.searchParams.append('ticket', ticket.StringId);
    }

    history.pushState({}, '', currentURL.href);

    let comments = await GetComments(projectId, ticket.Id);
    let commentsBox = popup.querySelector("#existingCommentsBox");
    commentsBox.innerHTML = '';
    if (comments !== undefined) {
        comments.forEach(comment => {
            const separator = document.createElement('hr');
            separator.classList.add('comment-separator');
            commentsBox.appendChild(separator);

            const commentContainer = document.createElement('div');
            commentContainer.classList.add('comment-container');

            const userName = document.createElement('strong');
            userName.textContent = comment.username;

            const timestampDate = new Date(comment.timestamp);
            const formattedDate = `${(timestampDate.getMonth() + 1).toString().padStart(2, '0')}/${timestampDate.getDate().toString().padStart(2, '0')}/${timestampDate.getFullYear()}`;
            const formattedTime = `${timestampDate.getHours().toString().padStart(2, '0')}:${timestampDate.getMinutes().toString().padStart(2, '0')}`;

            const timestamp = document.createElement('span');
            timestamp.textContent = `${formattedDate} ${formattedTime}`;
            timestamp.classList.add('timestamp');

            commentContainer.appendChild(userName);
            commentContainer.appendChild(timestamp);

            const commentContent = document.createElement('div');
            commentContent.classList.add('comment-content');

            const quill = new Quill(commentContent, {
                theme: 'snow',
                modules: {
                    syntax: true, // Enable syntax highlighting module
                    toolbar: false, // Disable toolbar
                },
            });

            commentContent.classList.remove('ql-container');

            quill.root.innerHTML = comment.text;

            commentsBox.appendChild(commentContainer);
            commentsBox.appendChild(commentContent);
        });
    }

    popup.addEventListener('click', closePopupOutside = function (event) {
        if (!popup.querySelector('.popup').contains(event.target)) {
            closePopup("editTicketPopup");
            document.removeEventListener('click', closePopupOutside);
        }
    });
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
            //    console.log("Changed ticket status");
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
        case "backlogColumn":
            status = "Backlog";
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

var handleAddTicketForm = function (form, formData) {
    let errorMessage = document.getElementById('errorMessage');
    if (!errorMessage) {
        errorMessage = document.createElement('span');
        errorMessage.id = "errorMessage";
        form.insertAdjacentElement('afterend', errorMessage);
    }

    let ticketPrioritySelected = false;
    let ticketStatusSelected = false;
    for (var entry of formData.entries()) {
        if (entry[0] === "Input.TicketPriorityValue") {
            ticketPrioritySelected = true;
        }
        if (entry[0] === "Input.TicketStatusValue") {
            ticketStatusSelected = true;
        }
    }

    if (ticketPrioritySelected === false || ticketStatusSelected === false) {
        errorMessage.innerHTML = "Ticket Status and Priority must be selected";
        return;
    }
    handleServerMessage(form, formData);
}

var handleServerMessage = function (form, formData) {
    let errorMessage = document.getElementById('errorMessage');
    if (!errorMessage) {
        errorMessage = document.createElement('span');
        errorMessage.id = "errorMessage";
        form.insertAdjacentElement('afterend', errorMessage);
    }
    
    fetch(form.action, {
        method: 'POST',
        body: formData
    })

    .then(response => response.json())
    .then(data => {
        if (data.success === false) {
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
        handleAddTicketForm(this, new FormData(this));
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
        if (data.success === true) {
            window.location.reload();
        }
    })
    .catch(error => {
        console.error(error.message);
    });
}

var addComment = function (projectId, ticketId, button) {
    
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
        const filteredNotifications = filterNotifications(data);
        var notificationHTML = generateNotificationHTML(filteredNotifications);
        displayNotificationPopup(notificationHTML);
    })
    .catch(error => console.error('Error fetching notifications:', error));
}

var generateNotificationHTML = function(notifications) {
    if (notifications.length === 0) {
        return `
        <div class="notification-list-container">
            <div class="notification-list" style="font-style: italic; color: #999;">NO NOTIFICATIONS CURRENTLY</div>
        </div>`;
    } else {
        return `
    <div style="display: flex; justify-content: center; align-items: center; font-style: italic; color: #999;">Most Recent Activity</div>
    <div class="notification-list-container">
       <div class="notification-list">${notifications.map(notification => `<div class="notification-item"><span style="font-style: italic;">${formatTimestamp(notification.timestamp)}</span>: ${notification.message || 'No Message'}</div>`).join('')}</div>
    </div>`;
    }
}

function formatTimestamp(timestamp) {
    const options = { day: '2-digit', month: '2-digit', year: 'numeric', hour: '2-digit', minute: '2-digit', hour12: false };
    const formattedDate = new Date(timestamp).toLocaleString('en-GB', options);
    return formattedDate;
}

function filterNotifications(notifications) {
    const maxNotifications = 30;

    notifications.sort((a, b) => new Date(b.timestamp) - new Date(a.timestamp));

    const filteredNotifications = notifications.filter((_, index) => {
        return index <= maxNotifications;
    });
    return filteredNotifications;
}

var displayNotificationPopup = function(content) {
    var notificationPopup = document.createElement('div');
    notificationPopup.className = 'notification-popup-parent';

    var notificationContent = document.createElement('div');
    notificationContent.className = 'notification-popup-content';
    notificationContent.innerHTML = content;

    notificationPopup.appendChild(notificationContent);
    document.body.appendChild(notificationPopup);

    var notificationContainer = document.querySelector('.notification-popup-container');
    if (notificationContainer) {
        notificationContainer.scrollTop = notificationContainer.scrollHeight;
    }

    notificationPopup.addEventListener('click', function(event) {
        if (!notificationContent.contains(event.target)) {
            closeNotificationPopup();
        }
    });
}

var closeNotificationPopup = function() {
    var popup = document.querySelector('.notification-popup-parent');
    if (popup) {
        document.body.removeChild(popup);
    }
}

$(document).ready(function() {
    $("#searchInput").on("input", function() {
        var searchQuery = $(this).val();
        if (searchQuery.length >= 3) {
            var projectId = $("#projectId").val();
            updateSearchResults(projectId, searchQuery);
        } else {
            clearSearchResults();
        }
    });

    $(document.body).on('click', function(event) {
        var target = $(event.target);
        if (!target.closest('#searchResultsDropdown').length) {
            clearSearchResults();
        }
    });
});

function updateSearchResults(projectId, query) {
    $.ajax({
        url: '/Projects/Search',
        data: { projectId: projectId, query: query },
        success: function(data) {
            displaySearchResults(data);
        },
        error: function(error) {
            console.error("Error fetching search results:", error);
        }
    });
}

function displaySearchResults(results) {
    var dropdown = $("#searchResultsDropdown");
    dropdown.empty();

    if (results.length === 0) {
        dropdown.append("<div>No results found</div>");
    } else {
        $.each(results, function(index, result) {
            var html = result.description ? result.description.match(/<p>(.*?)<\/p>/)[1] : null;
            var truncatedDescription = html ? (html.length > 70 ? html.substring(0, 70) + '...' : html) : 'N/A';
            var listItem = $('<div class="search-result-item">' + 
                             '<div><strong>ID:</strong> ' + result.stringId + '</div>' + 
                             '<div><strong>Title:</strong> ' + result.title + '</div>' + 
                             '<div><strong>Description:</strong> ' + truncatedDescription.trimEnd() + '...'  + '</div>' + 
                             '</div>');
            listItem.click(function() {
                openTicketPopup(result.id);

            });
            dropdown.append(listItem);
        });
    }
    dropdown.show();
}

function clearSearchResults() {
    $("#searchResultsDropdown").empty().hide();
}

var openTicketPopup  = async function(ticketId) {
    let currentUrl = window.location.href;
    let params = new URLSearchParams(currentUrl.substring(currentUrl.indexOf('?')));
    let projectId = params.get('projectId');

   await fetch(`/Projects/GetTicketInfo?projectId=${projectId}&searchTicketId=${ticketId}`, {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
        },
    })
    .then(response => response.json())
       .then(data => {
        if (data.success == true) {
            let getTicket = async function () {
                closeNotificationPopup();
                await showTicket(JSON.parse(data.ticketJson), data.assignedToUser);
                clearSearchResults();
            }
            getTicket();
        }
    })
    .catch(error => {
        console.error('Error fetching ticket', error);
    })
}

var assignTicketToUser = async function (projectId, ticketId) {
    await fetch(`/Projects/AssignTicketToUser?projectId=${projectId}&ticketId=${ticketId}`, {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
        },
    })
    .then(response => response.json())
    .then(data => {
        if (data.success !== true) {
            showErrorTooltip(data.message);
        }
        else {
            window.location.reload();
        }
    })
    .catch(error => {
        console.error('Error fetching ticket', error);
    })

}
var showErrorTooltip = function(errorMessage) {
    var errorTooltip = document.querySelector('#errorTooltip');
    errorTooltip.textContent = errorMessage;

    var tooltipX = (window.innerWidth - errorTooltip.offsetWidth) / 2;
    var tooltipY = 10; 

    errorTooltip.style.left = tooltipX + 'px';
    errorTooltip.style.top = tooltipY + 'px';

    errorTooltip.style.display = 'block';

    let tooltipTimeout = setTimeout(function () {
        errorTooltip.style.display = 'none';
    }, 3000);

    document.addEventListener('mousedown', function () {
        if (!errorTooltip.contains(event.target)) {
            errorTooltip.style.display = 'none';
            document.removeEventListener('mousedown');
            clearTimeout(tooltipTimeout);
        }
    });
}

var toggleUserTickets = function () {
    // let button = document.querySelector("#toggleUserTickets");
    var currentURL = new URL(window.location.href);
    if (currentURL.searchParams.has('userFilter')) {
        let filter = currentURL.searchParams.get('userFilter');
        if (filter == 'true')
        {
            currentURL.searchParams.set('userFilter', false);
        }
        else 
        {
            currentURL.searchParams.set('userFilter', true);
        }
    }
    else {
        currentURL.searchParams.set('userFilter', true);
    }
    window.location.href = currentURL;
}

var toggleBackLog = function () {
    let backlogColumn = document.querySelector("#backlogColumn");
    let doneColumn = document.querySelector("#doneColumn");
    let button = document.querySelector("#toggleBacklogButton");

    if (backlogColumn.style.display === 'none') {
        backlogColumn.style.display = '';
        doneColumn.style.display = 'none';
        button.style.background = '#1861ac'

    }
    else {
        backlogColumn.style.display = 'none';
        doneColumn.style.display = '';
        button.style.background = '';
    }
}