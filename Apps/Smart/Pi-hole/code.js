class PiHole
{
    getUrl(args) {
        let url = args.url;
        if (!url)
            return '';
        let token = args.properties['token'];
    
        if (url.endsWith('/'))
            url = url.substring(0, url.length - 1);
        if (url.endsWith('/admin') == false)
            url += '/admin';
        url += '/api.php/?summary&auth=' + token;
        return url;
    }
    
    status(args) {
        let url = this.getUrl(args, args.url);
        let data = args.fetch(url);
        let blocked = data?.ads_blocked_today ?? 0;
        let percent = data?.ads_percentage_today ?? 0;
        
        if(typeof(percent) === 'string')
            percent = parseFloat(percent.replace(/,/g, ''), 10);

        return args.liveStats([
            ['Queries', blocked],
            ['Percent', percent.toFixed(2) + '%']
        ]);
    }

    test(args) {
        let url = this.getUrl(args);

        let data = args.fetch(url);
        return isNaN(data?.ads_blocked_today) === false;
    }
}
