"use strict";

var connection = new signalR.HubConnectionBuilder()
    .withUrl("/hubs/chat")
    .withAutomaticReconnect([0, 1000, 5000, null])
    .build();


connection.on("ReceivePrivateMessage", function (senderId, senderName, receiverId, message, chatId, receiverName) {
    if (senderId === loggedUser && receiverId === userId) {
        addMessage(`Sent: ${message}`, true, senderId, receiverId);
    }
    else if (senderId === userId && receiverId === loggedUser) {
        addMessage(`Received: ${message}`, false, senderId, receiverId);
    }
    else {
        // Show a browser notification
        if (Notification.permission === "granted") {
            new Notification("New Message", {
                body: `${senderName}: ${message}`,
                icon: "path/to/notification-icon.png", // Replace with the path to your notification icon
            });
        } else if (Notification.permission !== "denied") {
            Notification.requestPermission().then(function (permission) {
                if (permission === "granted") {
                    new Notification("New Message", {
                        body: `${senderName}: ${message}`,
                        icon: "path/to/notification-icon.png", // Replace with the path to your notification icon
                    });
                }
            });
        }
    }
});

function addMessage(msg, isSent, senderId, recipientId) {
    if (!msg || !senderId || !recipientId) {
        return;
    }
    let ui = document.getElementById('messagesList');
    if (isSent || recipientId === loggedUser) {
        let li = document.createElement("li");
        li.textContent = msg;

        li.classList.add(isSent ? 'sent' : 'received');

        ui.appendChild(li);
    }
}

connection.start()
    .then(() => {
        console.log("SignalR connected");
    })
    .catch(err => console.error(err));

function sendPrivateMessage() {
    let ddlSelUser = document.getElementById('ddlSelUser');
    let ddlSelUserName = document.getElementById('ddlSelUserName');
    let txtPrivateMessage = document.getElementById('txtPrivateMessage');

    let receiverId = ddlSelUser.value;
    let receiverName = ddlSelUserName.value;
    let message = txtPrivateMessage.value.trim();

    if (message) {
        connection.invoke("SendPrivateMessage", receiverId, message, receiverName)

        txtPrivateMessage.value = '';
    }
}

