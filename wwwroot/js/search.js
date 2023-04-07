if(document.getElementById('search-container'))
{
    SearchEngines = JSON.parse(document.getElementById('search-engines').value);
    SearchEngineDefault = SearchEngines.filter(x => x.IsDefault)[0];
    SearchEngineLogo = document.getElementById('search-icon')
    SearchEngine = null;
}
function SearchKeyDown(event)
{
    let original = event.target.value;
    setTimeout(()=> {
        let searchText = event.target.value;
    
        if(searchText[0] === '/'){
            // special to filter out item                    
            SearchFilter(searchText.slice(1));
            return;
        }else {
            SearchFilter('');
        }
        if(event.code === 'Enter') {
            SearchPerform();
            return;
        }
        
        if(original === ' ' && event.code === 'Backspace')
        {
            if(!searchText && SearchEngine){
                // clear the search engine
                SearchEngine = null;
                SearchEngineLogo.setAttribute('src', SearchEngineDefault.Icon);
                return;
            }
        }
        if(SearchEngine)
            return; // already using a search engine
        if(/^[a-zA-Z]+ /.test(searchText)){
            // matches how a search engine shortcut is used
            let shortcut = searchText.slice(0, -1).toLowerCase();
            let se = SearchEngines.filter(x => x.Shortcut === shortcut);
            if(!se?.length)
                 return;
            SearchEngine = se[0];
            SearchEngineLogo.setAttribute('src', SearchEngine.Icon);
            event.target.value = ' ';
        }
    }, 10);
}

function SearchFilter(filter)
{
    
}

function SearchPerform(){
    let searchText = document.getElementById('search-text').value.trim();
    if(!searchText)
        return;
    
    
    let forceSearch = false;
    if(/^\?/.test(searchText)){
        forceSearch = true;
        searchText = searchText.substring(1);
        if(!searchText) return;
    }

    if(!forceSearch) {
        if (/^(\b25[0-5]|\b2[0-4][0-9]|\b[01]?[0-9][0-9]?)(\.(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)){3}($|\/)/.test(searchText)) {
            // ip address url, lets just go right there
            window.location = 'http://' + searchText;
            return;
        }
        if (/^lh:[\d]+/i.test(searchText)) {
            // localhost shortcut
            window.location = 'http://localhost:' + searchText.substring(3);
            return;
        }
        if (/^[a-z][a-z\-]*(:[\d]+)?\//i.test(searchText)) {
            // hostname /
            window.location = 'http://' + searchText;
            return;
        }
        if (/^[a-z][a-z\-]*:[\d]+/i.test(searchText)) {
            // hostname with port
            window.location = 'http://' + searchText;
            return;
        }
        if (/^((?!-))(xn--)?[a-z0-9][a-z0-9-_]{0,61}[a-z0-9]{0,1}\.(xn--)?([a-z0-9\-]{1,61}|[a-z0-9-]{1,30}\.[a-z]{2,})$/.test(searchText)) {
            // domainname 
            window.location = 'http://' + searchText;
            return;
        }
        if (/^((?!-))(xn--)?[a-z0-9][a-z0-9-_]{0,61}[a-z0-9]{0,1}\.(xn--)?([a-z0-9\-]{1,61}|[a-z0-9-]{1,30}\.[a-z]{2,})(\/.*)$/.test(searchText)) {
            // domainname with path 
            window.location = 'http://' + searchText;
            return;
        }
        if (/^(http:\/\/|https:\/\/).*$/.test(searchText)) {
            // full url
            window.location = searchText;
            return;
        }
    }

    let url = SearchEngine?.Url || SearchEngineDefault?.Url || 'https://duckduckgo.com/?q=%s';
    url = url.replace('%s', encodeURIComponent(searchText));
    window.location = url;
}