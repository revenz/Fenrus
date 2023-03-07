class Lidarr
{
    getUrl(args, endpoint) {
        return `api/v1/${endpoint}?apikey=${args.properties['apikey']}`;
    }

    async status(args) {
        let data = await args.fetch(this.getUrl(args, 'wanted/missing'));

        let missing = data?.totalRecords ?? 0;

        data = await args.fetch(this.getUrl(args, 'queue'));
        let queue = data?.totalRecords ?? 0;

        return args.liveStats([
            ['Missing', missing],
            ['Queue', queue]
        ]);
    }
    
    async test(args) {
        let data = await args.fetch(this.getUrl(args, 'wanted/missing'));
        console.log('data', data);
        return isNaN(data?.totalRecords) === false;
    }
}
