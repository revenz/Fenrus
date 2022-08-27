function openTerminal(type, uid){
    let div = document.createElement('div');
    div.setAttribute('id', 'terminal');
    document.body.appendChild(div);
    setTimeout(() => {
        document.body.classList.add('terminal');

        const fitAddon = new FitAddon.FitAddon();
        var term = new Terminal({ 
            cursorBlink: true, 
            fontFamily: 'Courier New',
            fontSize: 16,            
            convertEol: true,
            fontFamily: "'Fira Mono', monospace"
        });
        term.loadAddon(fitAddon);
        term.open(div); 
        fitAddon.fit();

        let divClose = document.createElement('div');
        divClose.className = 'close close-terminal';
        div.appendChild(divClose);
        
        term.write('Welcome to the Fenrus Terminal\r\n');   
        term.focus();
        let line = ''; 
        let socket;
        let server, username, password;
        const MODE_SERVER = 0;
        const MODE_USERNAME = 1;
        const MODE_PASSWORD = 2;
        const MODE_TERMINAL = 3;
        let mode = !!type ? MODE_TERMINAL : MODE_SERVER; 
        let genericSsh = mode === MODE_SERVER;
        let promptForUser = false;
        let promptForPassword = false;
        let authError = false;

        let rows = term.rows;
        let cols = term.cols;

        term.onKey(function (ev) {
            let key = ev.key;
            if(mode < 3){
                let keyCode = key.charCodeAt(0);
                if (keyCode === 13) // enter
                {
                    if(mode === 0 || mode === 1){
                        if(mode === 0)
                            server = line;
                        else
                            username = line;
                        changeMode(mode + 1);
                        return;
                    }
                    password = line;
                    changeMode(mode + 1);
                    connect([server, username, password]);
                    return;
                }
                else if(keyCode === 127){ //backspace
                    if(line.length > 0){
                        line = line.substring(0, line.length - 1);
                        if(mode !== 2)
                            term.write("\b \b");
                    }
                }
                else 
                {
                    line += key;
                    if(mode !== 2)
                        term.write(key);
                }
            }
            else
            {
                socket.emit('data', key);
            }
        });

        const inputVariablePrefix = '\x1b[1;32m';
        const inputVariableSuffix = '\x1b[37m';
        const changeMode = (newMode) => {
            if(newMode === 0)
                term.write(`\r\n${inputVariablePrefix}Server${inputVariableSuffix}: `);
            else if(newMode === 1)
                term.write(`\r\n${inputVariablePrefix}Username${inputVariableSuffix}: `);
            else if(newMode === 2)
                term.write(`\r\n${inputVariablePrefix}Password${inputVariableSuffix}: `);
            mode = newMode;
            line = '';
        }

        const resizeEvent = (event) => {
            fitAddon.fit();
            let newRows = term.rows;
            let newCols = term.cols;

            if(newRows == rows && newCols == cols)
                return;                
            rows = newRows;
            cols = newCols;
            console.log('resize', rows, cols);            
            socket.emit('resize', { rows: rows, cols: cols});
        }

        const connect = function(args){
            term.write('\r\nConnecting...\r\n');
            let https = document.location.protocol === 'https:';
            socket = io((https ? 'wss' : 'ws') + '://' + document.location.host, {
                rejectUnauthorized: false,
                transports:['polling', 'websocket']
            });
            socket.on("connect_error", (err) => {
                socket.close();
                console.log('connect_failed');
                term.write(`\r\nFailed to connect to server\r\n${err}\r\n`);
                mode = 0;
                term.write('Server: ');
            });
            socket.on('connect_failed', function(){
                console.log('connect_failed');
            });
            if(Array.isArray(args) === false)
                args = [args];
            if(type == 2)
                socket.emit('docker', [term.rows, term.cols].concat(args));
            else
                socket.emit('ssh', [term.rows, term.cols].concat(args));
            socket.on('connect', function() {});

            // Backend -> Browser
            socket.on('data', function(data) {
                if(mode === 3){
                    term.write(data);
                    authError = false;
                }
            });
            socket.on('terminal-closed', () => {
                console.log('got terminal-closed');
                if(authError && (promptForPassword || promptForUser))
                    return;
                if(mode === 3)
                    term.write('\r\closed\r\n');
                socket.close();
                closeTerminal();
            });
            socket.on('fenrus-error', (error) => {
                term.write('\r\n' + error + '\r\n');
                socket.close();
                closeTerminal(5000);
            });
            socket.on('request-user', (args) => {
                server = args[0];
                promptForUser = true;
                term.write(`\r\nServer: ${server}\r\n`);
                changeMode(1)
            });
            socket.on('request-pwd', (args) => {
                server = args[0];
                username = args[1];
                promptForPassword = true;
                term.write(`\r\nEnter the password for: ${username}\r\n`);
                changeMode(2)
            });
            socket.on('autherror', (error) => {
                console.log('auth error', error, genericSsh, promptForPassword);
                authError = true;
                if(genericSsh){
                    term.write('\r\nFailed to authenticate: ' + error + '\r\n');
                    changeMode(1);
                }
                else if(promptForUser) {
                    term.write('\r\n' + error + '\r\n');
                    changeMode(1);
                }
                else if(promptForPassword){
                    term.write('\r\n' + error + '\r\n');
                    changeMode(2);
                }
                else
                {
                    term.write('\r\n' + error + '\r\n');
                    socket.close();
                    closeTerminal(5000);
                }
            });

            socket.on('disconnect', function() {
                if(mode !== 3)
                    return;
                term.write('\r\n*** Disconnected ***\r\n');
                mode = 4;
                closeTerminal(5000);
            });

            window.addEventListener('resize', resizeEvent);
        }
        divClose.addEventListener('click', () => {
            socket?.close();
            closeTerminal();
        });

        const closeTerminal = function(timeout) {
            window.removeEventListener('resize', resizeEvent);
            document.body.classList.remove('terminal');
            const closeActual = () => {                
                div.className = 'closing';
                setTimeout(() => {
                    div.remove();
                }, 500);
            }
            if(timeout)
                setTimeout(closeActual, timeout)
            else
                closeActual();
        }

        if(mode !== 3)
            changeMode(mode);
            
        if(uid)
        {
            fetch('/terminal/' + uid).then((res) => res.text()).then(text => {
                if(text)
                    connect([text]);
                else
                    connect([uid]);
            })
            //connect([uid]);
        }

    }, 750);
}