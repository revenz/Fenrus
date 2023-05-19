class FenrusTerminal {
    
    inputVariablePrefix = '\x1b[1;32m';
    inputVariableSuffix = '\x1b[37m';
    MODE_SERVER = 0;
    MODE_USERNAME = 1;
    MODE_PASSWORD = 2;
    MODE_TERMINAL = 3;
    
    constructor({container, server, user, password}){
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

        this.line = '';
        this.socket = null;
        this.server = server || '';
        this.user = user || '';
        this.password = password || '';

        this.term.write('Welcome to the Fenrus Terminal\r\n');
        this.term.focus();
        this.promptForUser = false;
        this.promptForPassword = false;
        this.authError = false;

        this.rows = this.term.rows;
        this.cols = this.term.cols;

        this.term.onKey((ev) => this.onKey(ev));
        this.changeMode(!!this.server ? 
            !!this.user ? 
                !!this.password ? this.MODE_TERMINAL 
                    : this.MODE_PASSWORD 
                : this.MODE_USERNAME 
            : this.MODE_SERVER);
        if(this.mode ===  this.MODE_TERMINAL)
            this.connect();
    }
    
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
    
    onDisconnected(action) {
        this.onDisconnectedAction = action;
    }


    close() 
    {
        window.removeEventListener('resize', this.resizeEventHandler);
        this.term?.dispose();
        this.term = null;
        this.socket?.close();
        this.socket = null;
    }
    
    onKey(ev)
    {
        let key = ev.key;
        if(this.mode < 3){
            let keyCode = key.charCodeAt(0);
            if (keyCode === 13) // enter
            {
                if(this.mode === 0 || this.mode === 1){
                    if(this.mode === 0)
                        this.server = this.line;
                    else
                        this.user = this.line;
                    this.changeMode(this.mode + 1);
                    return;
                }
                this.password = this.line;
                this.changeMode(this.mode + 1);
                this.connect([this.server, this.user, this.password]);
            }
            else if(keyCode === 127){ //backspace
                if(this.line.length > 0){
                    this.line = this.line.substring(0, this.line.length - 1);
                    if(this.mode !== this.MODE_PASSWORD)
                        this.term.write("\b \b");
                }
            }
            else
            {
                this.line += key;
                if(this.mode !== this.MODE_PASSWORD)
                    this.term.write(key);
            }
        }
        else
        {
            this.socket.emit('data', key);
        }
    }

    changeMode(newMode) {
        if(newMode === 0)
            this.term.write(`\r\n${this.inputVariablePrefix}Server${this.inputVariableSuffix}: `);
        else if(newMode === 1)
            this.term.write(`\r\n${this.inputVariablePrefix}Username${this.inputVariableSuffix}: `);
        else if(newMode === 2)
            this.term.write(`\r\n${this.inputVariablePrefix}Password${this.inputVariableSuffix}: `);
        this.mode = newMode;
        this.line = '';
    }
    async encrypt(plainText)
    {
        let hexToBytes = (hex) => {
            const bytes = new Uint8Array(hex.length / 2);
            for (let i = 0; i < bytes.length; i++) {
                bytes[i] = parseInt(hex.substr(i * 2, 2), 16);
            }
            return bytes;
        }

        let arrayBufferToBase64 = (buffer)=> {
            let binary = '';
            const bytes = new Uint8Array(buffer);
            for (let i = 0; i < bytes.length; i++) {
                binary += String.fromCharCode(bytes[i]);
            }
            return btoa(binary);
        }
        
        const keyHex = 'E2C1B1E9C0812A8F2D52C0C7A1E90B8F';
        const ivHex = '65F1D7AC3903D2E964E3A2C1B1E9C081';

        // Convert the key and IV from hex to byte arrays
        const keyBytes = hexToBytes(keyHex);
        const ivBytes = hexToBytes(ivHex);

        // Convert the plaintext string to an ArrayBuffer
        const encoder = new TextEncoder();
        const data = encoder.encode(plainText);

        // Create a CryptoKey from the key bytes
        const key = await crypto.subtle.importKey(
            'raw',
            keyBytes,
            { name: 'AES-CBC', length: 256 },
            false,
            ['encrypt', 'decrypt']
        );

        // Encrypt the data using AES-CBC mode with the key and IV
        const encryptedData = await crypto.subtle.encrypt(
            { name: 'AES-CBC', iv: ivBytes },
            key,
            data
        );

        // Convert the encrypted data to a Base64-encoded string
        const encryptedString = arrayBufferToBase64(encryptedData);

        return encryptedString;
    }


    async connect()
    {
        this.term.write('\r\nConnecting...\r\n');
        let https = document.location.protocol === 'https:';
        let info = await this.encrypt(JSON.stringify({ server: this.server, user: this.user, password: this.password}));
        let cUrl = (https ? 'wss' : 'ws') + '://' + document.location.host + `/terminal/ssh?info=${encodeURIComponent(info)}&rows=${this.rows}&cols=${this.cols}`;
        this.socket = new WebSocket(cUrl);

        this.socket.emit = (ev, msg) => {
            this.socket.send(msg);
        }

        this.socket.onopen = (event)=> {
        }
        this.socket.onclose = (event)=> {
            if(this.authError && (this.promptForPassword || this.promptForUser))
                return;
            if(this.mode === 3)
                this.term?.write('\r\closed\r\n');
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
        this.changeMode(3);
    }
}