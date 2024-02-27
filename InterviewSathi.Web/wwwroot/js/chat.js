"use strict";


var connection = new signalR.HubConnectionBuilder()
    .withUrl("/hubs/chat")
    .withAutomaticReconnect([0, 1000, 5000, null])
    .build();

const configuration = { iceServers: [{ urls: 'stun:stun.l.google.com:19302' }] };
let peerConnection;
let remoteConnection;
let offer;
let answer;
let localStream;
let bufferedIceCandidates = [];


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
                icon: "../images/notificationicon.png",
            });
        } else if (Notification.permission !== "denied") {
            Notification.requestPermission().then(function (permission) {
                if (permission === "granted") {
                    new Notification("New Message", {
                        body: `${senderName}: ${message}`,
                        icon: "../images/notificationicon.png",
                    });
                }
            });
        }
    }
});

connection.on("ReceiveVideoAnswer", function (message) {
    const offerObject = JSON.parse(message);

    // Assuming offerObject has 'type' and 'sdp' properties
    const { type, sdp } = offerObject;

    // Create an RTCSessionDescription object
    answer = new RTCSessionDescription({
        type: type,
        sdp: sdp
    });
    console.log("You are receiving a answer:", answer.sdp);
    peerConnection.setRemoteDescription(answer)
        .then(console.log("Done!!"));
    document.getElementById('remoteView').srcObject = peerConnection.getRemoteStreams()[0];

});

connection.on("ReceiveVideoOffer", function (message) {
    const offerObject = JSON.parse(message);

    // Assuming offerObject has 'type' and 'sdp' properties
    const { type, sdp } = offerObject;

    // Create an RTCSessionDescription object
    offer = new RTCSessionDescription({
        type: type,
        sdp: sdp
    });
    console.log("You are receiving a offer:", offer.sdp);

    document.getElementById('joinVideoButton').style.display = 'block';
    document.getElementById('startVideoButton').style.display = 'none';

});
connection.on("ReceiveIceCandidate", function (message, senderId, receiverId) {
    const iceCandidate = new RTCIceCandidate(JSON.parse(message));
    console.log(iceCandidate);

    peerConnection.addIceCandidate(iceCandidate)
        .then(() => {
            // Handle success for the local peer
            console.log("ICE candidate added successfully for the local peer");
        })
        .catch((error) => {
            // Handle errors for the local peer
            console.error("Error adding ICE candidate for the local peer:", error);
            // Additional error handling for the local peer (if needed)
        });
});


connection.on("ReceiveIceCandidateFromLocal", function (message, senderId, receiverId) {
    const iceCandidate = new RTCIceCandidate(JSON.parse(message));
    console.log(iceCandidate);
    console.log("ReceiveIceCandidateFromLocal");

    if (remoteConnection && remoteConnection.signalingState === "stable") {
        remoteConnection.addIceCandidate(iceCandidate)
            .then(() => {
                // Handle success for the remote peer
                console.log("ICE candidate added successfully for the remote peer");
            })
            .catch((error) => {
                // Handle errors for the remote peer
                console.error("Error adding ICE candidate for the remote peer:", error);
                // Additional error handling for the remote peer (if needed)
            });
    } else {
        console.warn("Buffering ICE candidate for the remote peer.");
        bufferedIceCandidates.push(iceCandidate);
    }
    console.log("Buffered ICE candidates:", bufferedIceCandidates);
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




//let peerConnection;

// Function to initialize Peer Connection
const initializePeerConnection = () => {
    peerConnection = new RTCPeerConnection(configuration);

    // Set up event handlers, if needed
    peerConnection.onicecandidate = handleIceCandidate;
    peerConnection.ontrack = handleRemoteTrack;

};

// Function to handle ICE candidate events
const handleIceCandidate = (event) => {
    if (event.candidate) {
        // Send the ICE candidate to the remote peer using SignalR
        const receiverId = document.getElementById('ddlSelUser').value;
        const iceCandidateMessage = JSON.stringify(event.candidate);

        console.log('Sending ICE candidate to receiverId:', receiverId, 'Message:', iceCandidateMessage);

        connection.invoke("SendIceCandidateToRemote", receiverId, iceCandidateMessage)
            .catch(error => {
                console.error("Error in SendIceCandidate:", error);
                // Handle error if needed
            });
    }
};


const handleRemoteTrack = (event) => {
    const remoteVideo = document.getElementById('remoteView');
    if (remoteVideo.srcObject !== event.streams[0]) {
        remoteVideo.srcObject = event.streams[0];
    }
};

// Function to create and send offer
const createOffer = async () => {
    try {
        initializePeerConnection();

        // Assume you have the local video stream
        const stream = await navigator.mediaDevices.getUserMedia({ video: true, audio: true });
        // Add tracks to the Peer Connection
        stream.getTracks().forEach(track => {
            peerConnection.addTrack(track, stream);
        });
        document.getElementById('selfView').srcObject = stream;

        // Create offer
        const offer = await peerConnection.createOffer();
        await peerConnection.setLocalDescription(offer);

        // Send the offer to the remote peer using SignalR
        const receiverId = document.getElementById('ddlSelUser').value;
        const offerMessage = JSON.stringify(peerConnection.localDescription);

        //console.log('Sending offer to receiverId:', receiverId, 'Message:', offerMessage);

        await connection.invoke("SendVideoOffer", receiverId, offerMessage);
        console.log("SendVideoOffer invoked successfully");
    } catch (error) {
        console.error("Error in createOffer:", error);
        // Handle error if needed
    }
};

function handleOffer() {
    // Assuming you have the peerConnection object available
    remoteConnection = new RTCPeerConnection(configuration);

    // Set up local video stream
    navigator.mediaDevices.getUserMedia({ video: true, audio: true })
        .then(stream => {
            document.getElementById('selfView').srcObject = stream;

            // Add audio and video tracks individually
            stream.getTracks().forEach(track => {
                remoteConnection.addTrack(track, stream);
            });

            remoteConnection.setRemoteDescription(offer);

            // Process buffered ICE candidates if any
            if (remoteConnection.signalingState === "stable" && bufferedIceCandidates.length > 0) {
                bufferedIceCandidates.forEach(iceCandidate => {
                    // Send the buffered ICE candidates to the peer using SignalR
                    remoteConnection.addIceCandidate(iceCandidate).then(console.log("Ice added!!"));
                });
                console.log("Buffered ICE candidates before:", bufferedIceCandidates);

                // Clear the buffer after processing
                bufferedIceCandidates.length = 0;
                console.log("Buffered ICE candidates after:", bufferedIceCandidates);

            }
        })
        .then(() => {
            // Create and set local answer
            return remoteConnection.createAnswer();
        })
        .then(answer => remoteConnection.setLocalDescription(answer))
        .then(() => {
            // Send the answer to the remote peer using SignalR
            const receiverId = document.getElementById('ddlSelUser').value;
            connection.invoke("SendVideoAnswer", receiverId, JSON.stringify(remoteConnection.localDescription));
        })
        .then(() => {
            // Handle success
            console.log("SendVideoAnswer invoked successfully");
            // Additional success handling (if needed)
        })
        .catch(error => {
            // Handle errors
            console.error("Error in handleOffer:", error);
            // Additional error handling (if needed)
        });

    // Set up ICE candidate handling
    remoteConnection.onicecandidate = e => {
        if (e.candidate) {
            // Send the ICE candidate to the remote peer using SignalR
            const receiverId = document.getElementById('ddlSelUser').value;
            connection.invoke("SendIceCandidate", receiverId, JSON.stringify(e.candidate))
                .then(() => {
                    // Handle success
                    console.log("SendIceCandidate invoked successfully");
                    // Additional success handling (if needed)
                })
                .catch(error => {
                    // Handle errors
                    console.error("Error in SendIceCandidate:", error);
                    // Additional error handling (if needed)
                });
        }
    };

    // Set up remote stream handling
    remoteConnection.ontrack = event => {
        const remoteStream = event.streams[0];
        document.getElementById('remoteView').srcObject = remoteStream;
    };
}