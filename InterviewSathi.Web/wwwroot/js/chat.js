// Create SignalR connection
var connection = new signalR.HubConnectionBuilder()
    .withUrl("/hubs/chat")
    .withAutomaticReconnect([0, 1000, 5000, null])
    .build();

// Define functions to handle server-side events
//connection.on("ReceivePrivateMessage", function (senderId, senderName, receiverId, message, chatId, receiverName) {
//    console.log(`[Private Message to ${receiverName}] ${senderName} says: ${message}`);
//    showNotification(`New Private Message from ${senderName}`, message);
//});

connection.on("ReceiveSignal", function (userFrom, signal) {
    console.log(`Received signal from ${userFrom}: ${signal}`);
    // Handle the signal as needed (e.g., start video call)
});

connection.on("ReceiveCall", function (callerName) {
    console.log(`Incoming call from ${callerName}`);
    showNotification(`Incoming call from ${callerName}`, "Click to answer");
    // Handle the incoming call as needed
});

// Start the connection
connection.start().then(() => {
    console.log("SignalR connected");
}).catch(err => console.error(err));

//// Function to send a private message
//function sendPrivateMessage() {
//    let ddlSelUser = document.getElementById('ddlSelUser');
//    let ddlSelUserName = document.getElementById('ddlSelUserName');
//    let txtPrivateMessage = document.getElementById('txtPrivateMessage');

//    let receiverId = ddlSelUser.value;
//    let receiverName = ddlSelUserName.value;
//    let message = txtPrivateMessage.value;

//    connection.invoke("SendPrivateMessage", receiverId, message, receiverName)
//        .then(() => {
//            console.log(`Sent private message to ${receiverId}: ${message}`);
//        })
//        .catch(err => console.error(err));

//    txtPrivateMessage.value = '';
//}

// Function to start a video call


// Function to show browser notifications
function showNotification(title, message) {
    if (Notification.permission === "granted") {
        // Create and show the notification
        var notification = new Notification(title, { body: message });
    } else if (Notification.permission !== "denied") {
        // Request permission to show notifications
        Notification.requestPermission().then(permission => {
            if (permission === "granted") {
                // Create and show the notification
                var notification = new Notification(title, { body: message });
            }
        });
    }
}

connection.on("ReceivePrivateMessage", function (senderId, senderName, receiverId, message, chatId, receiverName) {
    addMessage(`[Private Message to ${receiverName} ]${senderName} says ${message}`);
})

function sendPrivateMessage() {
    let inputMsg = document.getElementById('txtPrivateMessage');
    let ddlSelUser = document.getElementById('ddlSelUser');
    let ddlSelUserName = document.getElementById('ddlSelUserName');

    let receiverId = ddlSelUser.value;
    let receiverName = ddlSelUserName.value;
    var message = inputMsg.value;

    connection.send("SendPrivateMessage", receiverId, message, receiverName);
    inputMsg.value = '';
}

function addMessage(msg) {
    if (msg == null && msg == '') {
        return;
    }
    let ui = document.getElementById('messagesList');
    let li = document.createElement("li");
    li.innerHTML = msg;
    ui.appendChild(li);
}

////connection.start();

////const hubUrl = "/hubs/chat";
////const connection = new signalR.HubConnectionBuilder().withUrl(hubUrl).build();
//var localVideo = document.getElementById("localVideo");
//var remoteVideo = document.getElementById("remoteVideo");

//let peerConnection;

async function startCall() {

    let ddlSelUser = document.getElementById('ddlSelUser').value;
    connection.invoke("SendSignal", ddlSelUser, "startVideoCall")
        .then(() => {
            console.log(`Sent video call signal to ${ddlSelUser}`);
            // Handle the start of the video call as needed
        })
        .catch(err => console.error(err));

    try {
        // Get local media stream
        let stream = await navigator.mediaDevices.getUserMedia({ video: true, audio: true });
        localVideo.srcObject = stream;



        // Join the room (replace 'room1' with your desired room name)
        await connection.invoke("JoinRoom", "room1");

        // Initialize peer connection
        peerConnection = new RTCPeerConnection();

        // Add local stream to peer connection
        stream.getTracks().forEach(track => peerConnection.addTrack(track, stream));

        // Handle incoming signals
        connection.on("ReceiveSignal", (userFrom, userFromSignal) => {
            let signal = JSON.parse(userFromSignal);
            if (signal.iceCandidate) {
                peerConnection.addIceCandidate(new RTCIceCandidate(signal.iceCandidate));
            } else if (signal.sdp) {
                peerConnection.setRemoteDescription(new RTCSessionDescription(signal.sdp));
                if (signal.sdp.type === "offer") {
                    peerConnection.createAnswer().then(answer => {
                        return peerConnection.setLocalDescription(answer);
                    }).then(() => {
                        connection.invoke("SendSignal", userFrom, JSON.stringify({ "sdp": peerConnection.localDescription }));
                    });
                }
            }
        });

        // Show notification for incoming call
        //connection.on("ReceiveCall", (callerName) => {
        //    showNotification(`Incoming call from ${callerName}`);
        //});
        // Send initial offer with user ID
        let userToId = document.getElementById('ddlSelUser').value; // Replace with the actual user ID
        let offer = await peerConnection.createOffer();
        await peerConnection.setLocalDescription(offer);
        connection.invoke("SendSignal", userToId, JSON.stringify({ "sdp": offer }));
    } catch (error) {
        console.error("Error starting call:", error);
    }
}

function pickupCall() {
    let localVideo = document.getElementById("localVideo");
    let remoteVideo = document.getElementById("remoteVideo");

    // Create a new RTCPeerConnection
    let peerConnection = new RTCPeerConnection();

    // Add local stream to peer connection (assuming you have obtained local stream earlier)
    navigator.mediaDevices.getUserMedia({ video: true, audio: true })
        .then(localStream => {
            localVideo.srcObject = localStream;
            localStream.getTracks().forEach(track => peerConnection.addTrack(track, localStream));
        })
        .catch(error => console.error("Error getting local stream:", error));

    // Set up event handlers for ICE candidates and remote stream
    peerConnection.onicecandidate = event => {
        if (event.candidate) {
            // Send the ICE candidate to the other peer
            connection.invoke("SendSignal", ddlSelUser.value, JSON.stringify({ "iceCandidate": event.candidate }));
        }
    };

    peerConnection.ontrack = event => {
        // Display the remote stream in the remote video element
        remoteVideo.srcObject = event.streams[0];
    };

    // Create an SDP offer
    peerConnection.createOffer()
        .then(offer => peerConnection.setLocalDescription(offer))
        .then(() => {
            // Send the SDP offer to the other peer
            connection.invoke("SendSignal", ddlSelUser.value, JSON.stringify({ "sdp": peerConnection.localDescription }));
        })
        .catch(error => console.error("Error creating offer:", error));

    // Set up a listener for incoming signals
    connection.on("ReceiveSignal", (userFrom, userFromSignal) => {
        const signal = JSON.parse(userFromSignal);

        if (signal.iceCandidate) {
            // Add the incoming ICE candidate
            peerConnection.addIceCandidate(new RTCIceCandidate(signal.iceCandidate))
                .catch(error => console.error("Error adding ICE candidate:", error));
        } else if (signal.sdp) {
            // Set the remote SDP description
            peerConnection.setRemoteDescription(new RTCSessionDescription(signal.sdp))
                .then(() => {
                    if (signal.sdp.type === "offer") {
                        // Create an SDP answer
                        return peerConnection.createAnswer();
                    }
                })
                .then(answer => peerConnection.setLocalDescription(answer))
                .then(() => {
                    // Send the SDP answer to the other peer
                    connection.invoke("SendSignal", userFrom, JSON.stringify({ "sdp": peerConnection.localDescription }));
                })
                .catch(error => console.error("Error handling SDP:", error));
        }
    });
}


//function showNotification(message) {
//    const notificationElement = document.getElementById('notification');
//    notificationElement.innerText = message;

//    // Show the notification
//    notificationElement.style.display = 'block';

//    // Automatically hide the notification after a few seconds (adjust as needed)
//    setTimeout(() => {
//        notificationElement.style.display = 'none';
//    }, 5000); // 5 seconds
//}

//// Connect to SignalR hub
//connection.start().then(() => {
//    console.log("SignalR connected");
//}).catch(err => console.error(err));


////var connection = new signalR.HubConnectionBuilder().withUrl("/hubs/chat").build();

//////Disable the send button until connection is established.
////document.getElementById("sendButton").disabled = true;

////connection.on("ReceiveMessage", function (user, message) {
////    var li = document.createElement("li");
////    document.getElementById("messagesList").appendChild(li);
////    // We can assign user-supplied strings to an element's textContent because it
////    // is not interpreted as markup. If you're assigning in any other way, you 
////    // should be aware of possible script injection concerns.
////    li.textContent = `${user} says ${message}`;
////});

////connection.start().then(function () {
////    document.getElementById("sendButton").disabled = false;
////}).catch(function (err) {
////    return console.error(err.toString());
////});

////document.getElementById("sendButton").addEventListener("click", function (event) {
////    var user = document.getElementById("userInput").value;
////    var message = document.getElementById("messageInput").value;
////    connection.invoke("SendMessage", user, message).catch(function (err) {
////        return console.error(err.toString());
////    });
////    event.preventDefault();
////});