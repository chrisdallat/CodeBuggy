let drop;

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