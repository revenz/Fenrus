module.exports = { 

    status: async (args) => {
        let branch = args.properties ? args.properties['branch'] : 'master';
        branch = branch || 'master';
        let url = args.url;
        url = url.substring(url.indexOf('//') + 2);
        if (url.indexOf('/') < 0)
            return; // no repo set
    
        url = url.substring(url.indexOf('/') + 1)
        url = 'https://api.github.com/repos/' + url;
        if(url.endsWith('/') === false)
            url += '/';
        url = `${url}commits/${branch}`;

        let data = await args.fetch({ url: url, headers: { 'User-Agent': 'User-Agent: Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/97.0.4692.99 Safari/537.36'} })
                
        let commit = data?.commit
        let date = commit?.author?.date;
        let total = data?.files?.length ?? '';

        if (!total){
            return;
        }

        return args.liveStats([
            ['Date', args.Utils.formatDate(date)],
            ['Changes', total]
        ]);
    }
}