const logDiv = document.getElementById('log');
const messageBox = document.getElementById('messageBox');
const statusSpan = document.getElementById('status');

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
    const data = event.data;
    if (data.startsWith('STATUS:')) {
        statusSpan.textContent = data.substring(7);
    } else {
        appendLog('< ' + data);
    }
});
