/**
 * A terminal wrapper base class used to open a websocket connection for a docker or ssh termianal
 */
class _FenrusTerminal {
    
    inputVariablePrefix = '\x1b[1;32m';
    inputVariableSuffix = '\x1b[37m';

    /**
     * Constructs a new instance
     * @param container the container dom element to render the termianl into
     */
    constructor(container){
        if (new.target === _FenrusTerminal) {
            throw new Error('Cannot instantiate abstract class');
        }
        
        this.container = container;
        this.container.innerHTML = '';
        this.resizeEventHandler = (event) => this.resizeEvent(event);
        window.addEventListener('resize', this.resizeEventHandler);

        this.fitAddon = new FitAddon.FitAddon();
        this.term = new Terminal({
            cursorBlink: true,
            fontSize: 16,
            convertEol: true,
            fontFamily: "'Fira Mono', monospace"
        });
        this.term.loadAddon(this.fitAddon);
        this.term.open(this.container);
        this.fitAddon.fit();

        this.socket = null;

        this.term.write('Welcome to the Fenrus Terminal\r\n');
        this.term.focus();

        this.rows = this.term.rows;
        this.cols = this.term.cols;

        this.term.onKey((ev) => this.onKey(ev));
    }

    /**
     * Event that is fired when the window is resized
     * @param event the resized event
     */
    resizeEvent(event)
    {
        this.fitAddon.fit();
        let newRows = this.term.rows;
        let newCols = this.term.cols;

        if(newRows === this.rows && newCols === this.cols)
            return;
        this.rows = newRows;
        this.cols = newCols;
        this.socket?.emit('resize', { rows: this.rows, cols: this.cols});
    }

    /**
     * Sets the action to execute when the terminal is disconnected
     * @param action the action/function to execute when the terminal is disconnected
     */
    onDisconnected(action) {
        this.onDisconnectedAction = action;
    }

    /**
     * Closes the terminal
     */
    close() 
    {
        window.removeEventListener('resize', this.resizeEventHandler);
        this.term?.dispose();
        this.term = null;
        this.socket?.close();
        this.socket = null;
    }

    /**
     * Event fired when a key is pressed
     * @param ev the event
     */
    onKey(ev)
    {
        let key = ev.key;
        this.socket.emit('data', key);
    }

    /**
     * Called when the connection has completed
     * Intended to be used in a subclass for additional setup after connecting
     */
    onConnected(){ }

    /**
     * Gets the URL route to connect the terminal to
     * @returns {string} the URL route to connect the terminal to
     */
    getConnectionUrl() { return ''; }

    /**
     * Connects the terminal
     * @returns {Promise<void>} a task for the connection to complete
     */
    async connect()
    {
        this.term.write('\r\nConnecting...\r\n');
        let https = document.location.protocol === 'https:';
        let route = this.getConnectionUrl();
        if(route instanceof Promise)
            route = await route;
        
        let cUrl = (https ? 'wss' : 'ws') + '://' + document.location.host + `/terminal/` + route;
        this.socket = new WebSocket(cUrl);

        this.socket.emit = (ev, msg) => {
            this.socket.send(msg);
        }

        this.socket.onopen = (event)=> {
        }
        this.socket.onclose = (event)=> {
            // if(this.authError && (this.promptForPassword || this.promptForUser))
            //     return;
            // if(this.mode === 3)
            //     this.term?.write('\r\closed\r\n');
            if(this.socket) 
            {
                this.socket.close();
                this.scoket = null;
            }
            this.close();
            if(this.onDisconnectedAction && typeof(this.onDisconnectedAction) === 'function')
                this.onDisconnectedAction();
        }

        this.socket.onerror = (event) => {
        }

        this.socket.onmessage = (event)=> {
            if(event.data) {
                this.term.write(event.data);
            }
        }
        this.onConnected();
    }
}