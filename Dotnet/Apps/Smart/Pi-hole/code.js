class PiHole
{
    getUrl(url) {
        if (!url)
            return '';
    
        if (url.endsWith('/'))
            url = url.substring(0, url.length - 1);
        if (url.endsWith('/admin') == false)
            url += '/admin';
        url += '/api.php';
        return url;
    }
    
    async status(args) {
        let url = this.getUrl(args.url);
        let data = await args.fetch(url);
        let blocked = data?.ads_blocked_today ?? 0;
        let percent = data?.ads_percentage_today ?? 0;

        return args.liveStats([
            ['Queries', blocked],
            ['Percent', percent.toFixed(2) + '%']
        ]);
    }

    async test(args) {
        let url = this.getUrl(args.url);

        let data = await args.fetch(url);
        return isNaN(data?.ads_blocked_today) === false;
    }
}
