class Lidarr
{
    getUrl(args, endpoint) {
        return `api/v1/${endpoint}?apikey=${args.properties['apikey']}`;
    }

    status(args) {
        let data = args.fetch(this.getUrl(args, 'wanted/missing'));

        let missing = data?.totalRecords ?? 0;

        data = args.fetch(this.getUrl(args, 'queue'));
        let queue = data?.totalRecords ?? 0;

        return args.liveStats([
            ['Missing', missing],
            ['Queue', queue]
        ]);
    }
    
    test(args) {
        let data = args.fetch(this.getUrl(args, 'wanted/missing'));
        console.log('data', data);
        return isNaN(data?.totalRecords) === false;
    }
}
