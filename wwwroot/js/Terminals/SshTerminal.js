/**
 * A SSH terminal
 */
class SshTerminal extends _FenrusTerminal {
    
    MODE_SERVER = 0;
    MODE_USERNAME = 1;
    MODE_PASSWORD = 2;
    MODE_TERMINAL = 3;

    /**
     * Constructs an instance of a SSH terminal
     * @param container the container to render the terminal into
     * @param server the server to connect to
     * @param user [Optional] the username of the SSH terminal
     * @param password [Optional] the password ofthe SSH terminal
     */
    constructor({container, server, user, password}){
        super(container);

        this.socket = null;
        this.server = server || '';
        this.user = user || '';
        this.password = password || '';
        this.promptForUser = false;
        this.promptForPassword = false;
        this.authError = false;
        
        this.changeMode(!!this.server ? 
            !!this.user ? 
                !!this.password ? this.MODE_TERMINAL 
                    : this.MODE_PASSWORD 
                : this.MODE_USERNAME 
            : this.MODE_SERVER);
        if(this.mode ===  this.MODE_TERMINAL)
            this.connect();
    }

    /**
     * Called when a key is pressed
     * @param ev the event
     */
    onKey(ev)
    {
        if(this.mode !== 3)
            return super.onKey(ev);
        let key = ev.key; 
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

    /**
     * Changes the input mode
     * @param newMode the new mode
     */
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

    /**
     * Encrypts data to send to the server
     * @param plainText the text to encrypt
     * @returns {Promise<string>} a promise with the encrypted text as the result
     */
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

    /**
     * Gets the URL route to connect the terminal to
     * @returns {string} the URL route to connect the terminal to
     */
    async getConnectionUrl(){
        let info = await this.encrypt(JSON.stringify({ server: this.server, user: this.user, password: this.password}));
        return `ssh?info=${encodeURIComponent(info)}&rows=${this.rows}&cols=${this.cols}`;
    }

    /**
     * Called after the terminal has connected
     */
    onConnected(){
        this.changeMode(3);
    }
    
}