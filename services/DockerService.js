const Settings = require('../models/Settings');
const docker = require('dockerode');

class DockerService 
{
    socket;
    docker;
    app;
    stream;
    timeOutTimer;
    AUTO_TIMEOUT = 10 * 60 * 1000;

    constructor(socket, app, system)
    {
        this.socket = socket;
        this.app = app;
        let dockerInstance = system.Docker?.filter(x => x.Uid === app.DockerUid);
        if(dockerInstance?.length)
            dockerInstance = dockerInstance[0];
        if(dockerInstance?.Address){
            this.docker = new docker({
                host: dockerInstance.Address,
                port: dockerInstance.Port || 2375
            });
        }
        else {            
            this.docker = new docker();
        }
    }

    async getContainer(name) {
        return await new Promise((resolve, reject) => {
            name = name.toLowerCase();
            this.docker.listContainers({all: true}, (err, containers) => {
                if(err)
                    console.error('err', err);
                for(let i=0; i < containers?.length; i++) {
                    let c = containers[i];
                    for(let j=0;j<c.Names?.length; j++){
                        let cName = c.Names[j].toLowerCase();
                        if(cName.startsWith('/'))
                            cName = cName.substring(1);
                        console.log(`### comparing container name '${cName}' to '${name}'`);
                        if(cName === name){
                            resolve(c);
                            return;
                        }
                    }
                }
                console.log('### failed to locate any matching containers');
                resolve(null);
            });
        });
    }

    async init(rows, cols)
    {
        const containerByName = await this.getContainer(this.app.DockerContainer);        
        if(!containerByName){
            this.socket.emit('fenrus-error', 'Failed to find container');
            console.log('##### failed to locate container by name: ', this.app.DockerContainer);
            return;
        }
        const container = this.docker.getContainer(containerByName.Id);
        if(!container){
            this.socket.emit('fenrus-error', 'Failed to find container');
            console.log('##### failed to locate container: ', this.app.DockerContainer, container);
            return;
        }

        let cmd = {
            Cmd: [this.app.DockerCommand || '/bin/bash'],
            AttachStdout: true,
            AttachStderr: true,
            AttachStdin: true,
            Tty: true,
            Env: ['LINES=' + rows, 'COLUMNS='+ cols]
        };
        this.socket.on('resize', (data) => {
            container.resize({h: data.rows, w: data.cols}, () => {
            });
        });
        container.exec(cmd, (err, exec) => {
            let options = {
                Tty: true,
                stream: true,
                stdin: true,
                stdout: true,
                stderr: true,
                hijack: true
            };

            container.wait((err, data) => {
                this.socket.emit('terminal-closed', '');
            });

            if (err) {
                console.log('### this.docker err: ', err);
                return;
            }
            exec.start(options, (err, stream) => {
                this.stream = stream;
                this.resetTimeout();
                let name = containerByName.Names;
                if(Array.isArray(name) && name.length > 0)
                    name = name[0];
                if(name?.startsWith('/'))
                    name = name.substring(1);
                if(!name)
                    name = this.app.DockerContainer;
                this.socket.emit('data', `\r\n*** Connected to ${name} ***\r\n\r\n`);
                stream.on('data', (chunk) => {
                    let data = chunk.toString();
                    this.socket.emit('data', data);
                });
                this.socket.on('resize', (data) => {
                    container.resize({h: data.rows, w: data.cols});
                });
                stream.on('end', () => {
                    this.socket.emit('terminal-closed', '');
                });

                this.socket.on('data', (data) => {
                    if (typeof data !== 'object')
                    {
                        stream.write(data);
                        this.resetTimeout();
                    }
                });
            });
        });
    }

    resetTimeout(){
        if(this.timeOutTimer)
            clearTimeout(this, this.timeOutTimer);
        this.timeOutTimer = setTimeout(() => this.closeSocket(), this.AUTO_TIMEOUT);
    }

    closeSocket()
    {
        console.log('### AUTOMATICALLY TIMEOUT OUT DOCKER CONNECTION');
        try{
            if(this.socket){
                this.socket.emit('terminal-closed', '');
            }
            if(this.stream){
                this.stream.end();
            }
        }catch(err) {
            console.log('### error: ', err);
        }
    }
}

module.exports = DockerService;
