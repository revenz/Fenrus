class FenrusSocket{
    
    socket;
    notificationHistory = {};
    notificationsSaveTimer;
    
    constructor() {
        const protocol = window.location.protocol;
        const hostname = window.location.hostname;
        const port = window.location.port;
        const wsProtocol = protocol === "https:" ? "wss:" : "ws:";
        const webSocketUrl = `${wsProtocol}//${hostname}:${port}/websocket`;
        
        this.socket = new WebSocket(webSocketUrl);
        this.socket.onmessage = (event) => this.onMessage(event);
        try
        {
            let local = localStorage.getItem('NOTIFICATIONS') || '';
            this.notificationHistory = JSON.parse(local) || {};
            let keys = Object.keys(this.notificationHistory);
            let cutOff = new Date().getTime() - (4 * 24 * 60 * 60 * 1000);
            for(let i=0;i<keys.length;i++){
                let key = keys[i];
                if(this.notificationHistory[key].date < cutOff) 
                    delete this.notificationHistory[key];
            }
        }
        catch(err) {
            this.notificationHistory = {};
        }

        window.addEventListener("beforeunload", () => {
            clearInterval(this.notificationsSaveTimer);
            localStorage.setItem('NOTIFICATIONS', JSON.stringify(this.notificationHistory));
            this.socket.close();
        });
    }
    
    onMessage(event) {
        console.log('websocket on message:' , event);
        const rgxDateTime = /^(\d{4})-(\d{2})-(\d{2})T(\d{2}):(\d{2}):(\d{2})Z$/;

        if(event.type === 'message'){
            let msg = JSON.parse(event.data);
            if(msg.type === 'notification'){
                let notification = JSON.parse(msg.data);
                if(notification.identifier){
                    if(this.notificationHistory[notification.identifier]) {
                        let history = this.notificationHistory[notification.identifier];
                        if (history.date > new Date().getTime() - (2 * 60 * 60 * 1000))
                            return;
                    }
                    this.notificationHistory[notification.identifier] = {
                        date: new Date().getTime()
                    };
                }
                if(rgxDateTime.test(notification.title))
                    notification.title = new Date(notification.title).toLocaleTimeString();
                new Toast(notification.type, notification.title, notification.message, notification.duration * 1000);
                
                clearInterval(this.notificationsSaveTimer);
                this.notificationsSaveTimer = setInterval(() => {
                    localStorage.setItem('NOTIFICATIONS', JSON.stringify(this.notificationHistory));
                }, 3000);
            }
            else if(msg.type === 'email')
            {
                if(typeof(msg.data) === 'string')
                    msg.data = JSON.parse(msg.data);
                console.log('email message!', msg.data);
                let customEvent = new CustomEvent('fenrusEmail',
                    { detail: msg.data }
                );
                document.dispatchEvent(customEvent);
            }
        } 
    }
}

document.addEventListener("DOMContentLoaded", () => {
    if(document.querySelector('.dashboard')) {
        let socket = new FenrusSocket();
    }
});
