/**
 * A SSH terminal pane
 */
class SshTerminalPane extends _TerminalPane
{
    /**
     * Opens a SSH Terminal 
     * @param url the URL for the SSH terminal
     * @param icon the icon image url to show in the title bar
     */
    open(url, icon){        
        if(url === this.url)
            return; // already opened
        if(this.url && this.url !== url)
            this.close();
        else
            this.createDomElements();
        
        this.url = url;
        this.eleTerminalFavicon.src = icon;
        super.open();
        let user = '';
        let pwd = ''
        let server = url;
        let atIndex = url.indexOf('@');
        if(atIndex > 0) {
            server = url.substring(atIndex + 1);
            user = url.substring(0, atIndex);
            let colonIndex = user.indexOf(':');
            if (colonIndex > 0) {
                pwd = user.substring(colonIndex + 1);
                user = user.substring(0, colonIndex);
            }
        }
        this.eleTerminalAddress.value = server;
        this.terminal = new SshTerminal({container: this.eleTerminal, server: server, user: user, password: pwd});
        this.terminal.onDisconnected(() => {
            this.close();
        });
    }

}