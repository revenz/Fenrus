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
        if (typeof(args[0]) === 'object')
        {
            let app = args[0];
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
        var conn = new SSHClient();
        conn.on('ready', () => {
            this.socket.emit('data', '\r\n*** SSH CONNECTION ESTABLISHED ***\r\n');
            conn.shell((err, stream) => {
                if (err)
                    return this.socket.emit('data', '\r\n*** SSH SHELL ERROR: ' + err.message + ' ***\r\n');
                this.socket.on('data', function (data)
                {
                    stream.write(data);
                });
                this.stream.on('data', function (d)
                {
                    this.socket.emit('data', d.toString('binary'));
                }).on('close', function ()
                {
                    conn.end();
                });
            });
        }).on('close', () => {
            this.socket.emit('data', '\r\n*** SSH CONNECTION CLOSED ***\r\n');
            this.socket.emit('ssh-closed', '');
            conn.end();
        }).on('error', (err) => {
            this.socket.emit('data', '\r\n*** SSH CONNECTION ERROR: ' + err.message + ' ***\r\n');
        }).connect({
            host: server,
            port: 22,
            username: username,
            password: password
        });
    }
}

module.exports = SshService;
