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
                if(!paused){
                    data = data.replace(/([INFO|WARN|DBUG|ERRR|ERROR|WARNING|DEBUG|CRIT|CRTICAL])/g, xtermColor("$1", 'green'));
                    data = data.replace(/([\d]{4}-[\d]{2}-[\d]{2}() [\d]{1,2}:[\d]{2}(:[\d]{2}(.[\d]+)?)?)?)/g, xtermColor("$1", 'blue'));
                    data = data.replace('->', xtermColor("$1", 'yellow'));
                    data = data.replace(/([GET|POST|PUT|DELETE|OPTIONS])/g, xtermColor("$1", 'magenta'));

                    term.write(data);
                }
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

function xtermColor(line, color) {

    const colors = {
        black:0,
        red: 1,
        green: 2,
        yellow: 3,
        blue: 4,
        magenta: 5,
        cyan: 6,
        white: 7
    }
    /** 
     * 
     * Foreground
    30 Black
    31 Red
    32 Green
    33 Yellow
    34 Blue
    35 Magenta
    36 Cyan
    37 White
    Background:

    40 Black
    41 Red
    42 Green
    43 Yellow
    44 Blue
    45 Magenta
    46 Cyan

    47 White

    0 Reset all
    1 Bold

     */
    return '\x1b[1;' + (30 + colors[color.toLowerCase()]) + 'm' + line + '\x1b[0m';
}