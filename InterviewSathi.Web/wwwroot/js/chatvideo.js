
"use strict";
//build connection
var connection = new signalR.HubConnectionBuilder()
    .withUrl("/hubs/chat")
    .withAutomaticReconnect([0, 1000, 5000, null])
    .build();

//variables
const configuration = { iceServers: [{ urls: 'stun:stun.l.google.com:19302' }] };
let peerConnection;
let remoteConnection;
let offer;
let answer;
let localStream;
let bufferedIceCandidates = [];
let offerer;
let answerer;


let isVideoEnabled = true;
let isAudioEnabled = true;
let isScreenSharing = false;


//receive private messages
connection.on("ReceivePrivateMessage", function (senderId, senderName, receiverId, message, chatId, receiverName) {
    if (senderId === loggedUser && receiverId === userId) {
        addMessage(message, true, senderId, receiverId, chatId);
    }
    else if (senderId === userId && receiverId === loggedUser) {
        addMessage(message, false, senderId, receiverId, chatId);
    }
    else {
        playNotificationSound();

        // Showing a browser notification
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


//receive offer
connection.on("ReceiveVideoOffer", function (message, senderId, receiverId, senderName) {
    const offerObject = JSON.parse(message);

    const { type, sdp } = offerObject;

    // Creating an RTCSessionDescription object
    offer = new RTCSessionDescription({
        type: type,
        sdp: sdp
    });

    console.log("You are receiving a offer:", offer.sdp);

    // showing all the buttons for the logged and pagged user only
    if (senderId === userId && receiverId === loggedUser) {
        document.getElementById('joinVideoButton').style.display = 'inline-block';
        document.getElementById('hangUpButton').style.display = 'inline-block';

        document.getElementById('startVideoButton').style.display = 'none';
    }
    else {
        playNotificationSound();
        document.getElementById('userIdInput').value = senderId;
        document.getElementById('callNotification').textContent = "You are getting a call from " + senderName;
        document.getElementById('goToUserButton').style.display = 'block';
        setTimeout(function () {
            // Clearing all the input field and removing the notification if user doesnt want to take the call
            document.getElementById('userIdInput').value = "";
            document.getElementById('callNotification').textContent = "";
            document.getElementById('goToUserButton').style.display = 'none';
        }, 30000).then(connection.invoke("SendMessagetosender", "User currently busy or offline try again later!!"));
    }

});


//receive answer
connection.on("ReceiveVideoAnswer", function (message) {
    const offerObject = JSON.parse(message);

    // Assuming offerObject has 'type' and 'sdp' properties
    const { type, sdp } = offerObject;

    // Creating an RTCSessionDescription object
    answer = new RTCSessionDescription({
        type: type,
        sdp: sdp
    });
    console.log("You are receiving a answer:", answer.sdp);
    peerConnection.setRemoteDescription(answer)
        .then(console.log("Done!!"));
    document.getElementById('remoteView').srcObject = peerConnection.getRemoteStreams()[0];
    document.getElementById('sidebar').style.display = 'none';
    document.getElementById('joinVideoButton').style.display = 'none';
    document.getElementById('startVideoButton').style.display = 'none';
    document.getElementById('hangUpButton').style.display = 'inline-block';

});

//receive ice
connection.on("ReceiveIceCandidate", function (message, senderId, receiverId) {
    const iceCandidate = new RTCIceCandidate(JSON.parse(message));
    console.log(iceCandidate);

    peerConnection.addIceCandidate(iceCandidate)
        .then(() => {
            console.log("ICE candidate added successfully for the local peer");
        })
        .catch((error) => {
            console.error("Error adding ICE candidate for the local peer:", error);
        });
});


connection.on("ReceiveIceCandidateFromLocal", function (message, senderId, receiverId) {
    const iceCandidate = new RTCIceCandidate(JSON.parse(message));
    console.log(iceCandidate);
    console.log("ReceiveIceCandidateFromLocal");

    if (remoteConnection && remoteConnection.signalingState === "stable") {
        remoteConnection.addIceCandidate(iceCandidate)
            .then(() => {
                console.log("ICE candidate added successfully for the remote peer");
            })
            .catch((error) => {
                console.error("Error adding ICE candidate for the remote peer:", error);
            });
    } else {
        console.warn("Buffering ICE candidate for the remote peer.");
        bufferedIceCandidates.push(iceCandidate);
    }
    console.log("Buffered ICE candidates:", bufferedIceCandidates);
});


function addMessage(msg, isSent, senderId, recipientId, chatId) {
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

connection.on("ReceiveScreenOffer", function (num, senderId, receiverId) {
    hangup();
});



connection.start()




//send message
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


function playNotificationSound() {
    const notificationSound = document.getElementById("notificationSound");
    if (notificationSound) {
        notificationSound.play();
    }
}

// Function to initialize Peer Connection
const initializePeerConnection = () => {
    peerConnection = new RTCPeerConnection(configuration);

    peerConnection.onicecandidate = handleIceCandidate;
    peerConnection.ontrack = handleRemoteTrack;

};

// Function to handle ICE candidate events
const handleIceCandidate = (event) => {
    if (event.candidate) {
        // Sending the ICE candidate to the remote peer using SignalR
        const receiverId = document.getElementById('ddlSelUser').value;
        const iceCandidateMessage = JSON.stringify(event.candidate);

        console.log('Sending ICE candidate to receiverId:', receiverId, 'Message:', iceCandidateMessage);

        connection.invoke("SendIceCandidateToRemote", receiverId, iceCandidateMessage)
            .catch(error => {
                console.error("Error in SendIceCandidate:", error);
            });
    }
};


const handleRemoteTrack = (event) => {
    let videoRemote = document.getElementById('remoteView');
    videoRemote.style.display = 'inline-block';
    const remoteVideo = videoRemote;
    if (remoteVideo.srcObject !== event.streams[0]) {
        remoteVideo.srcObject = event.streams[0];
    }
};

// Function to create and send offer
const createOffer = async () => {
    try {
        initializePeerConnection();

        offerer = loggedUser;
        localStream = await navigator.mediaDevices.getUserMedia({ video: true, audio: true });
        localStream.getTracks().forEach(track => {
            peerConnection.addTrack(track, localStream);
        });
        let peerVideo = document.getElementById('selfView');
        peerVideo.style.display = 'inline-block';
        peerVideo.srcObject = localStream;

        console.log('Local stream tracks:', localStream.getTracks());

        // Create offer
        const offer = await peerConnection.createOffer();
        await peerConnection.setLocalDescription(offer);

        // Send the offer to the remote peer using SignalR
        const receiverId = document.getElementById('ddlSelUser').value;
        const offerMessage = JSON.stringify(peerConnection.localDescription);

        await connection.invoke("SendVideoOffer", receiverId, offerMessage);
        console.log("SendVideoOffer invoked successfully");

        document.getElementById('hangUpButton').style.display = 'inline-block';
        document.getElementById('videoControl').style.display = 'inline-block';
        document.getElementById('audioControl').style.display = 'inline-block';
        document.getElementById('screenControl').style.display = 'inline-block';

        document.getElementById('toggleFullscreenButton').style.display = 'inline-block';
        document.getElementById('startVideoButton').style.display = 'none';
        console.log("startvideobutton clicked");
        document.getElementById('sidebar').style.display = 'none';

    } catch (error) {
        console.error("Error in createOffer:", error);
    }

};

function handleOffer() {
    remoteConnection = new RTCPeerConnection(configuration);
    answerer = loggedUser;
    // Set up local video stream
    navigator.mediaDevices.getUserMedia({ video: true, audio: true })
        .then(stream => {
            localStream = stream;
            let peerVideo = document.getElementById('selfView');
            peerVideo.style.display = 'block';
            peerVideo.srcObject = localStream;
            document.getElementById('startVideoButton').style.display = 'none';
            console.log("Attempting to hide startVideoButton...");

            document.getElementById('video-col').style.display = 'block';
            localStream.getTracks().forEach(track => {
                remoteConnection.addTrack(track, localStream);
            });

            remoteConnection.setRemoteDescription(offer);

            if (remoteConnection.signalingState === "stable" && bufferedIceCandidates.length > 0) {
                bufferedIceCandidates.forEach(iceCandidate => {
                    remoteConnection.addIceCandidate(iceCandidate).then(console.log("Ice added!!"));
                });
                console.log("Buffered ICE candidates before:", bufferedIceCandidates);

                bufferedIceCandidates.length = 0;
                console.log("Buffered ICE candidates after:", bufferedIceCandidates);

            }
        })
        .then(() => {
            return remoteConnection.createAnswer();
        })
        .then(answer => remoteConnection.setLocalDescription(answer))
        .then(() => {
            const receiverId = document.getElementById('ddlSelUser').value;
            connection.invoke("SendVideoAnswer", receiverId, JSON.stringify(remoteConnection.localDescription));
        })
        .then(() => {
            // Handle success
            console.log("SendVideoAnswer invoked successfully");
        })
        .catch(error => {
            // Handle errors
            console.error("Error in handleOffer:", error);
        });

    // Setting up ICE candidate handling
    remoteConnection.onicecandidate = e => {
        if (e.candidate) {
            const receiverId = document.getElementById('ddlSelUser').value;
            connection.invoke("SendIceCandidate", receiverId, JSON.stringify(e.candidate))
                .then(() => {
                    // Handle success
                    console.log("SendIceCandidate invoked successfully");
                })
                .catch(error => {
                    // Handle errors
                    console.error("Error in SendIceCandidate:", error);
                });
        }
    };

    // Setting up remote stream handling
    remoteConnection.ontrack = event => {
        const remoteStream = event.streams[0];
        let remoteVideo = document.getElementById('remoteView');
        remoteVideo.style.display = 'inline-block';
        remoteVideo.srcObject = remoteStream;

        document.getElementById('hangUpButton').style.display = 'inline-block';
        document.getElementById('videoControl').style.display = 'inline-block';
        document.getElementById('audioControl').style.display = 'inline-block';
        document.getElementById('screenControl').style.display = 'inline-block';

        document.getElementById('joinVideoButton').style.display = 'none';

        document.getElementById('sidebar').style.display = 'none';
    };

}

// Function to handle the Hang Up button click
const hangUp = () => {
    if (localStream) {
        localStream.getTracks().forEach(track => track.stop());
        localStream = null;
    }

    if (remoteConnection) {
        remoteConnection.close();
        remoteConnection = null;
    }

    if (peerConnection) {
        peerConnection.close();
        peerConnection = null;
    }

    // Clearing the video elements
    document.getElementById('selfView').srcObject = null;
    document.getElementById('remoteView').srcObject = null;

    // Hiding the Hang Up button
    document.getElementById('selfView').style.display = 'none';
    document.getElementById('remoteView').style.display = 'none';
    document.getElementById('videoControl').style.display = 'none';
    document.getElementById('audioControl').style.display = 'none';
    document.getElementById('hangUpButton').style.display = 'none';
    document.getElementById('joinVideoButton').style.display = 'none';
    document.getElementById('screenControl').style.display = 'none';
    document.getElementById('startVideoButton').style.display = 'block';
    document.getElementById('startVideoButton').style.transition = 'opacity 0.5s ease -in -out';
    document.getElementById('sidebar').style.display = 'block';

    $('#hangUpModal').modal('show');
};



function confirmRatings() {
    $('#hangUpModal').modal('hide');
};

// Add logging statements to functions
async function toggleVideo() {
    try {
        isVideoEnabled = !isVideoEnabled;

        if (localStream) {
            localStream.getVideoTracks().forEach(track => track.enabled = isVideoEnabled);
            console.log(`Video ${isVideoEnabled ? 'enabled' : 'disabled'}`);

            var videoButton = document.getElementById("videoControl");
            if (!isVideoEnabled) {
                videoButton.classList.remove("btn-outline-dark");
                videoButton.classList.add("btn-dark");
            } else {
                videoButton.classList.remove("btn-dark");
                videoButton.classList.add("btn-outline-dark");
            }
        }
    } catch (error) {
        console.error('Error toggling video:', error);
    }
}

async function toggleAudio() {
    try {
        isAudioEnabled = !isAudioEnabled;

        if (localStream) {
            localStream.getAudioTracks().forEach(track => track.enabled = isAudioEnabled);
            console.log(`Audio ${isAudioEnabled ? 'enabled' : 'disabled'}`);

            var audioButton = document.getElementById("audioControl");
            if (!isAudioEnabled) {
                audioButton.classList.remove("btn-outline-dark");
                audioButton.classList.add("btn-dark");
            } else {
                audioButton.classList.remove("btn-dark");
                audioButton.classList.add("btn-outline-dark");
            }

        }
    } catch (error) {
        console.error('Error toggling audio:', error);
    }
}

async function toggleScreenSharing() {
    try {
        if (isScreenSharing) {
            localStream.getTracks().forEach(track => track.stop());

            // Get user media for video
            localStream = await navigator.mediaDevices.getUserMedia({ video: true, audio: true });
            isScreenSharing = false;
            console.log('Screen sharing stopped');
        } else {
            // Stop the current tracks (video or screen share)
            localStream.getTracks().forEach(track => track.stop());

            // Get display media for screen sharing
            localStream = await navigator.mediaDevices.getDisplayMedia({ video: true, audio: true });
            isScreenSharing = true;
            console.log('Screen sharing started');
        }

        // Update local video element
        selfView.srcObject = localStream;

        if (answerer === loggedUser) {
            remoteConnection.getSenders().forEach(sender => {
                localStream.getTracks().forEach(track => {
                    console.log('Replacing track:', track);
                    sender.replaceTrack(track).then(
                        () => console.log('Track replaced successfully'),
                        error => console.error('Error replacing track:', error)
                    );
                });
            });
        }
        else if (offerer === loggedUser) {
            peerConnection.getSenders().forEach(sender => {
                localStream.getTracks().forEach(track => {
                    console.log('Replacing track:', track);
                    sender.replaceTrack(track).then(
                        () => console.log('Track replaced successfully'),
                        error => console.error('Error replacing track:', error)
                    );
                });
            });
        }
    } catch (error) {
        console.error('Error toggling screen sharing:', error);
    }
}