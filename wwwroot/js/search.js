/**
 * Class that handles the user searching
 */
class Searcher {

    /**
     * Constructs a new instance of the Seacher
     */
    constructor(){
        this.eleSearch = document.getElementById('search-text');
        this.divLaunchingApp = document.getElementById('launching-app');
        this.eleSerachIcon = document.getElementById('search-icon');
        if(document.getElementById('search-container'))
        {
            this.SearchEngines = JSON.parse(document.getElementById('search-engines').value);
            this.SearchEngineDefault = this.SearchEngines.filter(x => x.IsDefault)[0];
            this.SearchEngine = null;
        }else {
            this.SearchEngines = [];
        }
        this.eleSearch.addEventListener('keydown', (event) => this.onKeyDown(event));
    }

    /**
     * Event that is fired when the search input key is down
     * @param event the event
     */
    onKeyDown(event)
    {
        let original = event.target.value;
        setTimeout(()=> {
            let searchText = event.target.value;

            if(event.code === 'Enter') {
                this.performSearch();
                return;
            }

            if(original === ' ' && event.code === 'Backspace')
            {
                if(!searchText && this.SearchEngine){
                    // clear the search engine
                    this.SearchEngine = null;
                    this.eleSerachIcon.setAttribute('src', this.SearchEngineDefault.Icon);
                    return;
                }
            }
            if(this.SearchEngine)
                return; // already using a search engine
            if(/^[a-zA-Z]+ /.test(searchText)){
                // matches how a search engine shortcut is used
                let shortcut = searchText.slice(0, -1).toLowerCase();
                let se = this.SearchEngines.filter(x => x.Shortcut === shortcut);
                if(!se?.length)
                    return;
                this.SearchEngine = se[0];
                this.eleSerachIcon.setAttribute('src', this.SearchEngine.Icon);
                event.target.value = ' ';
            }
        }, 10);
    }


    /**
     * Performs the actual search
     */
    performSearch()
    {
        let searchText = this.eleSearch.value.trim();
        if(!searchText)
            return;


        let forceSearch = false;
        if(/^\?/.test(searchText)){
            forceSearch = true;
            searchText = searchText.substring(1);
            if(!searchText) return;
        }
        
        if(!forceSearch && searchText.indexOf(' ') > 0)
            forceSearch = true;

        if(!forceSearch) {
            if (/^(\b25[0-5]|\b2[0-4][0-9]|\b[01]?[0-9][0-9]?)(\.(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)){3}($|\/)/.test(searchText)) {
                // ip address url, lets just go right there
                this.openUrl('http://' + searchText, searchText);
                return;
            }
            if (/^lh:[\d]+/i.test(searchText)) {
                // localhost shortcut
                let url = 'http://localhost:' + searchText.substring(3);
                this.openUrl(url, url);
                return;
            }
            if (/^[a-z][a-z\-]*(:[\d]+)?\//i.test(searchText)) {
                // hostname /
                this.openUrl('http://' + searchText, searchText);
                return;
            }
            if (/^[a-z][a-z\-]*:[\d]+/i.test(searchText)) {
                // hostname with port
                this.openUrl('http://' + searchText, searchText);
                return;
            }
            if (/^((?!-))(xn--)?[a-z0-9][a-z0-9-_]{0,61}[a-z0-9]{0,1}\.(xn--)?([a-z0-9\-]{1,61}|[a-z0-9-]{1,30}\.[a-z]{2,})$/.test(searchText)) {
                // domainname 
                this.openUrl('http://' + searchText, searchText);
                return;
            }
            if (/^((?!-))(xn--)?[a-z0-9][a-z0-9-_]{0,61}[a-z0-9]{0,1}\.(xn--)?([a-z0-9\-]{1,61}|[a-z0-9-]{1,30}\.[a-z]{2,})(\/.*)$/.test(searchText)) {
                // domainname with path 
                this.openUrl('http://' + searchText, searchText);
                return;
            }
            if (/^(http:\/\/|https:\/\/).*$/.test(searchText)) {
                // full url
                this.openUrl(searchText, searchText);
                return;
            }
        }

        let url = this.SearchEngine?.Url || this.SearchEngineDefault?.Url || 'https://duckduckgo.com/?q=%s';
        url = url.replace('%s', encodeURIComponent(searchText));
        window.location = url;
        this.openUrl(url, 'Searching ' +  searchText);
    }

    /**
     * Opens a url and shows the launching message
     * @param urL the url to open
     * @param text the text to show when launching the search
     */
    openUrl(urL, text){
        this.divLaunchingApp.querySelector('.title').textContent = text;
        this.divLaunchingApp.querySelector('img').src = this.eleSerachIcon.src;
        this.divLaunchingApp.style.display = 'unset';
        window.location = url;
    }
}
document.addEventListener('DOMContentLoaded', () => {
    new Searcher();
});