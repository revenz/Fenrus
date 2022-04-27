class Radarr {
    getUrl(args, endpoint) {
        return `api/v3/${endpoint}?apikey=${args.properties['apikey']}`;
    }

    async status(args) {
        let filter = args.properties['filters'];

        let data = []
        let queueData = []

        if (args.properties['fetchWarnings'] == 'true') {
            [data, queueData] = await Promise.all([
                await args.fetch({
                    url: this.getUrl(args, 'movie'),
                    timeout: 10000
                }),
                await args.fetch({
                    url: this.getUrl(args, 'queue') + '&pageSize=10000',
                    timeout: 5000
                })
            ]);
        } else {
            data = await args.fetch({
                url: this.getUrl(args, 'movie'),
                timeout: 10000
            });
        }

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

        if (args.properties['fetchWarnings'] != 'true') {
            return args.liveStats([
                ['Missing', missingCount],
                ['Queue', queue]
            ]);
        } else {
            let queueDataRecords = queueData?.records;
            let filteredQueue = queueDataRecords?.filter((x, index, arr) => {
                return x.trackedDownloadStatus == 'warning'
            })
            let totalWarnings = filteredQueue?.length ?? 0;

            return args.liveStats([
                ['Missing', missingCount],
                ['Queue', queue],
                ['Warnings', totalWarnings]
            ]);
        }



    }

    async test(args) {
        let data = await args.fetch(this.getUrl(args, 'queue'));
        return isNaN(data?.records?.length) === false;
    }
}

module.exports = Radarr;