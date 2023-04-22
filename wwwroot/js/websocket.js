class FenrusSocket{
    
    socket;
    constructor() {
        const protocol = window.location.protocol;
        const hostname = window.location.hostname;
        const port = window.location.port;
        const wsProtocol = protocol === "https:" ? "wss:" : "ws:";
        const webSocketUrl = `${wsProtocol}//${hostname}:${port}/websocket`;
        
        this.socket = new WebSocket(webSocketUrl);
        this.socket.onmessage = this.onMessage;

        window.addEventListener("beforeunload", () => {
            this.socket.close();
        });
    }
    
    onMessage(event) {
        if(event.type === 'message'){
            let msg = JSON.parse(event.data);
            if(msg.type === 'notification'){
                let notification = JSON.parse(msg.data);
                console.log('notification received', notification);
                new Toast(notification.type, notification.message, notification.duration * 1000);
            }            
        }
    }
}

document.addEventListener("DOMContentLoaded", () => {
    if(document.querySelector('.dashboard')) {
        let socket = new FenrusSocket();
    }
});
