class Radarr {
    getUrl(args, endpoint) {
        return `api/v3/${endpoint}?apikey=${args.properties['apikey']}`;
    }

    async status(args) {
        let filter = args.properties['filters'];

        let data = await args.fetch(this.getUrl(args, 'movie'));
        let filteredData = data?.filter((x, index, arr) => {
            let validEntry = x.hasFile == false;
            if (validEntry && (filter == 'both' || filter == 'available')) {
                validEntry = x.isAvailable == true;
            }
            if (validEntry && (filter == 'both' || filter == 'monitored')) {
                validEntry = x.monitored == true;
            }
            return validEntry;
        })
        let missingCount = filteredData?.length ?? 0;

        data = await args.fetch(this.getUrl(args, 'queue'));
        let queue = data?.records?.length ?? 0;
        return args.liveStats([
            ['Missing', missingCount],
            ['Queue', queue]
        ]);
    }

    async test(args) {
        let data = await args.fetch(this.getUrl(args, 'queue'));
        return isNaN(data?.records?.length) === false;
    }
}

module.exports = Radarr;