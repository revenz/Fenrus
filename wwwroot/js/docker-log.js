function openDockerLog(uid){
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
        divClose.className = 'terminal-btn close close-terminal';
        div.appendChild(divClose);

        let divPause = document.createElement('div');
        divPause.className = 'terminal-btn pause pause-terminal';
        div.appendChild(divPause);
        
        let socket;
        let paused = false;
        let rows = term.rows;
        let cols = term.cols;


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
            let https = document.location.protocol === 'https:';
            socket = io((https ? 'wss' : 'ws') + '://' + document.location.host, {
                rejectUnauthorized: false,
                transports:['polling', 'websocket']
            });
            socket.on("connect_error", (err) => {
                socket.close();
                console.log('connect_failed');
                term.write(`\r\nFailed to connect to server\r\n${err}\r\n`);
            });
            socket.on('connect_failed', function(){
                console.log('connect_failed');
            });
            socket.emit('docker-log', [term.rows, term.cols].concat(args));;
            socket.on('connect', function() {});

            // Backend -> Browser
            socket.on('data', function(data) {
                if(!paused)
                    term.write(data);
            });
            socket.on('terminal-closed', () => {                
                term.write('\r\closed\r\n');
                socket.close();
                closeTerminal();
            });

            socket.on('disconnect', function() {
                term.write('\r\n*** Disconnected ***\r\n');
                closeTerminal(5000);
            });

            window.addEventListener('resize', resizeEvent);
        }
        divClose.addEventListener('click', () => {
            socket?.close();
            closeTerminal();
        });
        divPause.addEventListener('click', () => {
            divPause.classList.remove('paused');
            paused = !paused;
            if(paused)
                divPause.classList.add('paused');
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
            
        fetch('/terminal/' + uid).then((res) => res.text()).then(text => {
            if(text)
                connect([text]);
            else
                connect([uid]);
        });

    }, 750);
}