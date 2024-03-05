
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


//receive messages
connection.on("ReceivePrivateMessage", function (senderId, senderName, receiverId, message, chatId, receiverName) {
    if (senderId === loggedUser && receiverId === userId) {
        addMessage(`Sent: ${message}`, true, senderId, receiverId, chatId);
    }
    else if (senderId === userId && receiverId === loggedUser) {
        addMessage(`Received: ${message}`, false, senderId, receiverId, chatId);
    }
    else {
        playNotificationSound();

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


//receive offer
connection.on("ReceiveVideoOffer", function (message, senderId, receiverId, senderName) {
    const offerObject = JSON.parse(message);

    // offerObject has 'type' and 'sdp' properties
    const { type, sdp } = offerObject;

    // Creating an RTCSessionDescription object
    offer = new RTCSessionDescription({
        type: type,
        sdp: sdp
    });

    console.log("You are receiving a offer:", offer.sdp);

    // showing all the buttons for the logged and pagged user only
    if (senderId === userId && receiverId === loggedUser) {
        document.getElementById('joinVideoButton').style.display = 'block';
        document.getElementById('hangUpButton').style.display = 'block';
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


//receive offer
connection.on("ReceiveScreenOffer", function (message, senderId, receiverId) {
    const offerObject = JSON.parse(message);

    // offerObject has 'type' and 'sdp' properties
    const { type, sdp } = offerObject;

    // Creating an RTCSessionDescription object
    offer = new RTCSessionDescription({
        type: type,
        sdp: sdp
    });

    console.log("You are receiving a offer:", offer.sdp);

    remoteConnection.setRemoteDescription(offer);


});

//receive answer


//receive ice

//send message
//send offer
//send answer
//send ice

//toggle video
//toggle audio
//toggle screen
//full screen

//hangup call









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
    document.getElementById('joinVideoButton').style.display = 'none';
    document.getElementById('startVideoButton').style.display = 'none';
    document.getElementById('hangUpButton').style.display = 'block';

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

function playNotificationSound() {
    const notificationSound = document.getElementById("notificationSound");
    if (notificationSound) {
        notificationSound.play();
    }
}

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
    let videoRemote = document.getElementById('remoteView');
    videoRemote.style.display = 'block';
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
        // Assume you have the local video stream
        localStream = await navigator.mediaDevices.getUserMedia({ video: true, audio: true });
        // Add tracks to the Peer Connection
        localStream.getTracks().forEach(track => {
            peerConnection.addTrack(track, localStream);
        });
        let peerVideo = document.getElementById('selfView');
        peerVideo.style.display = 'block';
        peerVideo.srcObject = localStream;

        console.log('Local stream tracks:', localStream.getTracks());

        // Create offer
        const offer = await peerConnection.createOffer();
        await peerConnection.setLocalDescription(offer);

        // Send the offer to the remote peer using SignalR
        const receiverId = document.getElementById('ddlSelUser').value;
        const offerMessage = JSON.stringify(peerConnection.localDescription);

        //console.log('Sending offer to receiverId:', receiverId, 'Message:', offerMessage);

        await connection.invoke("SendVideoOffer", receiverId, offerMessage);
        console.log("SendVideoOffer invoked successfully");


        let videoControl = document.getElementById('videoControl');
        videoControl.style.display = 'block';
        let audioControl = document.getElementById('audioControl');
        audioControl.style.display = 'block';

    } catch (error) {
        console.error("Error in createOffer:", error);
        // Handle error if needed
    }

};

function handleOffer() {
    // Assuming you have the peerConnection object available
    remoteConnection = new RTCPeerConnection(configuration);
    answerer = loggedUser;
    // Set up local video stream
    navigator.mediaDevices.getUserMedia({ video: true, audio: true })
        .then(stream => {
            localStream = stream;
            let peerVideo = document.getElementById('selfView');
            peerVideo.style.display = 'block';
            peerVideo.srcObject = localStream;

            // Add audio and video tracks individually
            localStream.getTracks().forEach(track => {
                remoteConnection.addTrack(track, localStream);
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
        let remoteVideo = document.getElementById('remoteView');
        remoteVideo.style.display = 'block';
        remoteVideo.srcObject = remoteStream;
        document.getElementById('hangUpButton').style.display = 'block';
        document.getElementById('videoControl').style.display = 'block';
        document.getElementById('audioControl').style.display = 'block';
        document.getElementById('joinVideoButton').style.display = 'none';
    };

}

// Function to handle the Hang Up button click
const hangUp = () => {
    // Stop the local video stream
    if (localStream) {
        localStream.getTracks().forEach(track => track.stop());
        localStream = null;
    }

    // Stop the remote video stream (assuming it's in the 'remoteConnection' variable)
    if (remoteConnection) {
        remoteConnection.close();
        remoteConnection = null;
    }

    // Stop the remote video stream (assuming it's in the 'remoteConnection' variable)
    if (peerConnection) {
        peerConnection.close();
        peerConnection = null;
    }

    // Clear the video elements
    document.getElementById('selfView').srcObject = null;
    document.getElementById('remoteView').srcObject = null;

    // Hide the Hang Up button
    document.getElementById('selfView').style.display = 'none';
    document.getElementById('remoteView').style.display = 'none';
    document.getElementById('videoControl').style.display = 'none';
    document.getElementById('audioControl').style.display = 'none';
    document.getElementById('hangUpButton').style.display = 'none';
    document.getElementById('joinVideoButton').style.display = 'none';
    document.getElementById('startVideoButton').style.display = 'block';
};



// Add logging statements to functions

async function toggleVideo() {
    try {
        isVideoEnabled = !isVideoEnabled;

        if (localStream) {
            localStream.getVideoTracks().forEach(track => track.enabled = isVideoEnabled);
            console.log(`Video ${isVideoEnabled ? 'enabled' : 'disabled'}`);
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
        }
    } catch (error) {
        console.error('Error toggling audio:', error);
    }
}

async function toggleScreenSharing() {
    try {
        if (isScreenSharing) {
            // Stop the current tracks (video or screen share)
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

        //peerConnection.getSenders().forEach(sender => {
        //    localStream.getTracks().forEach(track => sender.replaceTrack(track));
        //});

        // Remove existing tracks and add the new ones to the connection
        //remoteConnection.getSenders().forEach(sender => {
        //    localStream.getTracks().forEach(track => sender.replaceTrack(track));
        //});
        // Remove existing tracks and add the new ones to the connection
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
        else if(offerer === loggedUser) {
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
        // Add the screen-sharing track to the peer connection
        //handleRemoteTrack();
    } catch (error) {
        console.error('Error toggling screen sharing:', error);
    }
}


function toggleFullscreen() {
    try {
        if (selfView.requestFullscreen) {
            if (!document.fullscreenElement) {
                selfView.requestFullscreen();
                console.log('Entered fullscreen');
            } else {
                document.exitFullscreen();
                console.log('Exited fullscreen');
            }
        } else if (selfView.mozRequestFullScreen) { // Firefox
            // Similar handling for other browsers
        }
    } catch (error) {
        console.error('Error toggling fullscreen:', error);
    }
}
