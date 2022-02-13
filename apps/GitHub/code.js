module.exports = { 

    status: (args) => {
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
        console.log('fetchgin: '+ url);

        return new Promise((resolve, reject) => {
            args.fetch({ url: url, headers: { 'User-Agent': 'User-Agent: Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/97.0.4692.99 Safari/537.36'} })
                .then(data => {

                    let commit = data?.commit
                    let date = commit?.author?.date;
                    let total = data?.files?.length ?? '';

                    if (!total){
                        resolve();
                        return;
                    }

                    resolve(
                        args.liveStats([
                            ['Date', args.Utils.formatDate(date)],
                            ['Changes', total]
                        ])
                    );
                }).catch(error => {
                    reject(error);
                });
            });
    }
}