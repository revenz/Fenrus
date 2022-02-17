class Sonarr
{
    getUrl(args, endpoint){
        return `api/v3/${endpoint}?apikey=${args.properties['apikey']}`;
    }
    
    async status(args) {
        let url = this.getUrl(args, 'wanted/missing') + '&sortKey=airDateUtc&sortDir=desc';

        let data = await args.fetch(url);
        let missing = data?.totalRecords ?? 0;
        data = await args.fetch(this.getUrl(args, ('queue')))
        let queue = data?.totalRecords ?? 0;

        return args.liveStats([
            ['Missing', missing],
            ['Queue', queue]
        ]);
    }
    
    async test(args) {
        let data = await args.fetch(this.getUrl(args, 'wanted/missing') + '&sortKey=airDateUtc&sortDir=desc');
        return isNaN(data?.totalRecords) === false;
    }
}

module.exports = Sonarr;