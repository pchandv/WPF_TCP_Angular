const logDiv = document.getElementById('log');
const messageBox = document.getElementById('messageBox');

function appendLog(text) {
    const p = document.createElement('div');
    p.textContent = text;
    logDiv.appendChild(p);
    logDiv.scrollTop = logDiv.scrollHeight;
}

function sendMessage() {
    const text = messageBox.value;
    if (text) {
        window.chrome.webview.postMessage(text);
        appendLog('> ' + text);
        messageBox.value = '';
    }
}

window.chrome.webview.addEventListener('message', event => {
    appendLog('< ' + event.data);
});

// TODO: add UI improvements and connection status display
