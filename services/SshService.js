const e = require('express');
const Settings = require('../models/Settings');

class SshService 
{
    socket;
    constructor(socket)
    {
        this.socket = socket;
    }

    init(args)
    {
        var SSHClient = require('ssh2').Client;

        let server, username, password;
        console.log('args', args);
        if (Array.isArray(args) === false)
        {
            console.log('debug 0');
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
            console.log('debug 1');
            server = args[0];
            username = args[1];
            password = args[2];
        }
        console.log('server: ', server);
        console.log('username: ', username);
        var conn = new SSHClient();
        conn.on('ready', () => {
            this.socket.emit('data', '\r\n*** SSH CONNECTION ESTABLISHED ***\r\n');
            conn.shell((err, stream) => {
                if (err)
                    return this.socket.emit('data', '\r\n*** SSH SHELL ERROR: ' + err.message + ' ***\r\n');
                this.socket.on('data', (data) =>
                {
                    stream.write(data);
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
}

module.exports = SshService;
