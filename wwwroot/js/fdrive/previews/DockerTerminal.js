/**
 * A Docker terminal pane
 */
class DockerTerminalPane extends _TerminalPane
{
    /**
     * Opens a Docker Terminal
     * @param name the name of the docker link, this is show in the title bar
     * @param url the URL for the SSH terminal
     * @param icon the icon image url to show in the title bar
     */
    open(name, url, icon){
        if(url === this.url)
            return; // already opened
        if(this.url && this.url !== url)
            this.close();
        else
            this.createDomElements();

        this.url = url;
        let parts = url.split(':');
        this.uid = parts[0];
        this.name = parts[1];
        this.command = parts.length > 2 ? parts[2] : '';
        
        this.eleTerminalFavicon.src = icon;
        super.open();        
        this.eleTerminalAddress.value = name;
        this.fenrusTerminal = new DockerTerminal({container: this.eleTerminal, uid: this.uid, name: this.name, command: this.command});
        this.fenrusTerminal.onDisconnected(() => {
            this.close();
        });
    }

}