/**
 * A pane for a terminal, abstract class
 */
class _TerminalPane extends FenrusPreview {
    
    /**
     * Constructs a new instance of a terminal pane
     */
    constructor() {
        if (new.target === _TerminalPane) {
            throw new Error('Cannot instantiate abstract class');
        }
        super();
    }

    /**
     * Creates teh dom elements needed for this pane
     */
    createDomElements()
    {
        if(this.container)
            return;
        this.container = document.createElement('div');
        this.container.innerHTML = '<div class="terminal-container app-target-container">' +
            '  <div class="header">' +
            '    <div class="address-bar">' +
            '      <img />' +
            '      <input type="text" readonly>' +
            '    </div>' +
            '    <div class="controls">' +
            '      <button class="close-button"><i class="fa-solid fa-xmark"></i></button>' +
            '    </div>' +
            '  </div>' +
            '  <div class="inner-container">' +
            '    <div></div>' +
            '  </div>' +
            '</div>';
        this.container.setAttribute('id', 'fdrive-apps-terminal');
        this.eleTerminal = this.container.querySelector('.inner-container div');
        this.eleTerminalAddress = this.container.querySelector('.address-bar input[type=text]');
        this.eleTerminalFavicon = this.container.querySelector('.address-bar img');
        this.container.querySelector('.close-button').addEventListener('click', () => this.close());
        document.querySelector('.dashboard-main').appendChild(this.container);
    }

    /**
     * Closes the terminal
     */
    close(){
        if(this.terminal)
        {
            this.terminal.onDisconnected(null);
            this.terminal.close();
        }
        this.terminal = null;
        this.url = null;
        super.close();
    }
}