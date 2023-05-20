function openDockerLog(uid){
    let div = document.createElement('div');
    div.setAttribute('id', 'terminal');
    document.body.appendChild(div);
    setTimeout(() => {
        document.body.classList.add('terminal');

        const fitAddon = new FitAddon.FitAddon();
        var term = new _FenrusTerminal({ 
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
            socket.emit('resize', { rows: rows, cols: cols});
        }
        window.addEventListener('resize', resizeEvent);
        
        const connect = function(args){
            term.write('\r\nConnecting...\r\n');
            let https = document.location.protocol === 'https:';
            let cUrl = (https ? 'wss' : 'ws') + '://' + document.location.host + `/terminal/log/${uid}?rows=${rows}&cols=${cols}`;
            console.log('cUrl:' , cUrl);
            socket = new WebSocket(cUrl);

            socket.emit = (ev, msg) => {
                socket.send(msg);
            }

            socket.onopen = function(event) {
                console.log('socket open');
            }
            socket.onclose = function(event) {
                console.log('terminal log close');
                socket.close();
                closeTerminal();
            }

            socket.onerror = function(event) {
                console.log('socket error', event);
            }

            socket.onmessage = function(event) {
                if(event.data) {
                    console.log(event.data);
                    term.write(event.data);
                }
            }
        }
        divClose.addEventListener('click', () => {
            console.log('socket closed!');
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
            console.log('close log terminal');
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
        connect([uid]);

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