class tautulli
{
    getUrl(args, endpoint) {
        return `api/v2?apikey=${args.properties['apikey']}&cmd=${endpoint}`;
    }
    async status(args) {
        let data = await args.fetch(this.getUrl(args, 'get_activity'));

        let streamCount = data?.response.data.stream_count ?? 0;
        
        return args.liveStats([
            ['Stream Count', streamCount]
        ]);
    }
    
    async test(args) {
        let data = await args.fetch(this.getUrl(args, 'arnold'));
        console.log('data', data);
        return data?.response.result == 'success';
    }
}

module.exports = tautulli;