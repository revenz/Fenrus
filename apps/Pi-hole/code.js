function getUrl(url) {
    if (!url)
        return '';

    if (url.endsWith('/'))
        url = url.substring(0, url.length - 1);
    if (url.endsWith('/admin') == false)
        url += '/admin';
    url += '/api.php';
    return url;
}

module.exports = { 
    
    status: (args) => {
        let url = getUrl(args.url);
        return new Promise((resolve, reject) => 
        {
            args.fetch(url).then(data => {
                let blocked = data?.ads_blocked_today ?? 0;
                let percent = data?.ads_percentage_today ?? 0;

                resolve(
                    args.liveStats([
                        ['Queries', blocked],
                        ['Percent', percent.toFixed(2) + '%']
                    ])
                );
            })
        }).catch(error => {
            reject(error);
        });
    },

    test: (args) => {
        let url = getUrl(args.url);

        return new Promise(function (resolve, reject) {
            args.fetch(url).then(data => {
                if (isNaN(data?.ads_blocked_today) === false)
                    resolve();
                else
                    reject();
            }).catch((error) => {
                reject(error);
            });
        })
    }
}