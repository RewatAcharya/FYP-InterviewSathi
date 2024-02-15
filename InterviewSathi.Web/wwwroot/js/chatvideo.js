"use strict";

var connection = new signalR.HubConnectionBuilder()
    .withUrl("/hubs/chat")
    .withAutomaticReconnect([0, 1000, 5000, null])
    .build();

var localVideo = document.getElementById("localVideo");
var remoteVideo = document.getElementById("remoteVideo");
var peerConnection;

connection.on("ReceivePrivateMessage", function (senderId, senderName, receiverId, message, chatId, receiverName) {
    addMessage(`[Private Message to ${receiverName}] ${senderName} says ${message}`);
});

function addMessage(msg) {
    if (!msg) {
        return;
    }
    let ui = document.getElementById('messagesList');
    let li = document.createElement("li");
    li.innerHTML = msg;
    ui.appendChild(li);
}

connection.on("ReceiveSignal", function (userFrom, userFromSignal) {
    console.log(`Received signal from ${userFrom}: ${userFromSignal}`);

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
    let message = txtPrivateMessage.value;

    connection.invoke("SendPrivateMessage", receiverId, message, receiverName)
        .then(() => {
            console.log(`Sent private message to ${receiverId}: ${message}`);
        })
        .catch(err => console.error(err));

    txtPrivateMessage.value = '';
}

function startCall() {
    let ddlSelUser = document.getElementById('ddlSelUser').value;

    // Create a new RTCPeerConnection
    peerConnection = new RTCPeerConnection();

    // Set up event handlers for ICE candidates and remote stream
    peerConnection.onicecandidate = event => {
        if (event.candidate) {
            // Send the ICE candidate to the other peer
            connection.invoke("SendSignal", ddlSelUser, JSON.stringify({ "iceCandidate": event.candidate }));
        }
    };

    // Set up event handler for receiving the remote stream
    peerConnection.ontrack = event => {
        // Display the remote stream in the remote video element
        remoteVideo.srcObject = event.streams[0];
    };

    // Obtain local media stream
    navigator.mediaDevices.getUserMedia({ video: true, audio: true })
        .then(localStream => {
            localVideo.srcObject = localStream;
            localStream.getTracks().forEach(track => peerConnection.addTrack(track, localStream));

            // Set up a listener for incoming signals
            connection.on("ReceiveSignal", (userFrom, userFromSignal) => {
                console.log(`Received signal from ${userFrom}: ${userFromSignal}`);

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

            // Create an SDP offer
            return peerConnection.createOffer();
        })
        .then(offer => peerConnection.setLocalDescription(offer))
        .then(() => {
            // Send the SDP offer to the other peer
            connection.invoke("SendSignal", ddlSelUser, JSON.stringify({ "sdp": peerConnection.localDescription }));
        })
        .catch(error => {
            console.error("Error getting local stream or creating offer:", error);
        });
}

function pickupCall() {
    let ddlSelUser = document.getElementById('ddlSelUser').value;
    let ddlSelUserName = document.getElementById('ddlSelUserName').value;

    // Display the incoming call modal
    showCallModal(ddlSelUserName);
    // Create a new RTCPeerConnection
    peerConnection = new RTCPeerConnection();

    // Set up event handlers for ICE candidates and remote stream
    peerConnection.onicecandidate = event => {
        if (event.candidate) {
            // Send the ICE candidate to the other peer
            connection.invoke("SendSignal", ddlSelUser, JSON.stringify({ "iceCandidate": event.candidate }));
        }
    };

    // Set up event handler for receiving the remote stream
    peerConnection.ontrack = event => {
        // Display the remote stream in the remote video element
        remoteVideo.srcObject = event.streams[0];
    };

    // Obtain local media stream
    navigator.mediaDevices.getUserMedia({ video: true, audio: true })
        .then(localStream => {
            localVideo.srcObject = localStream;
            localStream.getTracks().forEach(track => peerConnection.addTrack(track, localStream));

            // Set up a listener for incoming signals
            connection.on("ReceiveSignal", (userFrom, userFromSignal) => {
                console.log(`Received signal from ${userFrom}: ${userFromSignal}`);

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

            // Create an SDP answer
            return peerConnection.createAnswer();
        })
        .then(answer => peerConnection.setLocalDescription(answer))
        .then(() => {
            // Send the SDP answer to the other peer
            connection.invoke("SendSignal", ddlSelUser, JSON.stringify({ "sdp": peerConnection.localDescription }));
        })
        .catch(error => {
            console.error("Error getting local stream or creating answer:", error);
        });
}


function hangUpCall() {
    // Close the peer connection and stop local media
    if (peerConnection) {
        peerConnection.close();
    }
    localVideo.srcObject.getTracks().forEach(track => track.stop());
    localVideo.srcObject = null;
    remoteVideo.srcObject = null;
}

function showNotification(title, message) {
    if (Notification.permission === "granted") {
        var notification = new Notification(title, { body: message });
    } else if (Notification.permission !== "denied") {
        Notification.requestPermission().then(permission => {
            if (permission === "granted") {
                var notification = new Notification(title, { body: message });
            }
        });
    }
}
