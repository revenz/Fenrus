const e = require('express');
const Settings = require('../models/Settings');

class SshService 
{
    socket;
    conn;
    timeOutTimer;
    AUTO_TIMEOUT = 10 * 60 * 1000;

    constructor(socket)
    {
        this.socket = socket;
    }

    init(args, rows, cols)
    {
        var SSHClient = require('ssh2').Client;

        let server, username, password;
        if (Array.isArray(args) === false)
        {
            let app = args;
            if (!app?.SshServer)
            {
                this.socket.emit('data', '\r\n*** SSH NOT CONFIGURED FOR THIS APPLICATION  ***\r\n');
                return;
            }
            server = app.SshServer;
            username = app.SshUsername;
            password = app.SshPassword;
        }
        else
        {
            server = args[0];
            username = args[1];
            password = args[2];
        }
        console.log('server: ', server);
        console.log('username: ', username);
        var conn = new SSHClient();
        this.conn = conn;
        conn.on('ready', () => {
            this.socket.emit('data', '\r\n*** SSH CONNECTION ESTABLISHED ***\r\n');
            conn.shell({rows: rows, cols: cols}, (err, stream) => {
                if (err)
                    return this.socket.emit('data', '\r\n*** SSH SHELL ERROR: ' + err.message + ' ***\r\n');
                this.socket.on('data', (data) =>
                {
                    stream.write(data);
                });
                this.socket.on('resize', (data) => {
                    stream.setWindow(data.rows, data.cols);
                });
                stream.on('data', (d) =>
                {
                    this.socket.emit('data', d.toString('binary'));
                }).on('close', () =>
                {
                    conn.end();
                });
            });
        }).on('close', () => {
            console.log('##### close');
            this.socket.emit('data', '\r\n*** SSH CONNECTION CLOSED ***\r\n');
            this.socket.emit('terminal-closed', '');
            conn.end();
        }).on('error', (err) => {
            console.log('##### error', err);
            if(err?.toString().indexOf('authentication') > 0){
                // auth erro
                console.log('### auth error', err);
                this.socket.emit('autherror', err.toString());
            }
            else
            {
                this.socket.emit('data', '\r\n*** SSH CONNECTION ERROR: ' + err.message + ' ***\r\n');
            }
        }).connect({
            host: server,
            port: 22,
            username: username,
            password: password
        });
    }

    resetTimeout(){
        if(this.timeOutTimer)
            clearTimeout(this, this.timeOutTimer);
        this.timeOutTimer = setTimeout(() => this.closeSocket(), this.AUTO_TIMEOUT);
    }

    closeSocket(){
        if(!this.conn)
            return;
        this.conn.end();
        if(this.socket)
            this.socket.close();
    }
}

module.exports = SshService;
