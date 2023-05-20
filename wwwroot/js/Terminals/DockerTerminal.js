/**
 * A Docker terminal
 */
class DockerTerminal extends _FenrusTerminal {
    
    /**
     * Constructs an instance of a Docker terminal
     * @param container the container to render the terminal into
     * @param uid the UID of the docker container
     * @param name the name of the docker container
     * @param command the command to run to open the terminal inside the docker container
     */
    constructor({container, uid, name, command})
    {
        super(container);

        this.uid = uid;    
        this.name = name;
        this.command = command;
        this.connect();
    }

    /**
     * Gets the URL route to connect the terminal to
     * @returns {string} the URL route to connect the terminal to
     */
    async getConnectionUrl(){
        return `docker/${this.uid}/${encodeURI(this.name)}?rows=${this.rows}&cols=${this.cols}&command=${encodeURIComponent(this.command)}`;
    }    
}