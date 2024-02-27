let localStream;
let isVideoEnabled = true;
let isAudioEnabled = true;
let isScreenSharing = false;

const selfView = document.getElementById('selfView');

async function startMedia() {
    try {
        localStream = await navigator.mediaDevices.getUserMedia({ video: isVideoEnabled, audio: isAudioEnabled });
        selfView.srcObject = localStream;

        // Ensure that the video is playing
        selfView.play().catch(error => console.error('Error playing video:', error));
    } catch (error) {
        console.error('Error accessing media devices:', error);
    }
}

async function toggleVideo() {
    isVideoEnabled = !isVideoEnabled;

    if (localStream) {
        localStream.getVideoTracks().forEach(track => track.enabled = isVideoEnabled);
    } else {
        await startMedia();
    }
}

async function toggleAudio() {
    isAudioEnabled = !isAudioEnabled;

    if (localStream) {
        localStream.getAudioTracks().forEach(track => track.enabled = isAudioEnabled);
    } else {
        await startMedia();
    }
}

async function toggleScreenSharing() {
    try {
        if (isScreenSharing) {
            localStream.getTracks().forEach(track => track.stop());
            isScreenSharing = false;
        } else {
            localStream = await navigator.mediaDevices.getDisplayMedia({ video: true, audio: true });
            isScreenSharing = true;
        }

        selfView.srcObject = localStream;
    } catch (error) {
        console.error('Error accessing media devices:', error);
    }
}
function toggleFullscreen() {
    if (selfView.requestFullscreen) {
        if (!document.fullscreenElement) {
            selfView.requestFullscreen();
        } else {
            document.exitFullscreen();
        }
    } else if (selfView.mozRequestFullScreen) { // Firefox
        if (!document.mozFullScreenElement) {
            selfView.mozRequestFullScreen();
        } else {
            document.mozCancelFullScreen();
        }
    } else if (selfView.webkitRequestFullscreen) { // Chrome, Safari and Opera
        if (!document.webkitFullscreenElement) {
            selfView.webkitRequestFullscreen();
        } else {
            document.webkitExitFullscreen();
        }
    } else if (selfView.msRequestFullscreen) { // IE/Edge
        if (!document.msFullscreenElement) {
            selfView.msRequestFullscreen();
        } else {
            document.msExitFullscreen();
        }
    }
}

// Start the user's video when the page loads
startMedia();



