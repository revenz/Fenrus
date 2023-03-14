class Sonarr {
    getUrl(args, endpoint) {
        return `api/v3/${endpoint}?apikey=${args.properties['apikey']}`;
    }

    status(args) {

        let url = this.getUrl(args, 'wanted/missing') + '&sortKey=airDateUtc&sortDir=desc';
        let queueUrl = this.getUrl(args, 'queue') + '&pageSize=10000';
        let data = args.fetch(url);
        let queueData = []
        
        if (args.properties['fetchWarnings'] == 'true')
            queueData = args.fetch(queueUrl);

        let missing = data?.totalRecords ?? 0;
        data = args.fetch(this.getUrl(args, ('queue')))
        let queue = data?.totalRecords ?? 0;

        if (args.properties['fetchWarnings'] != 'true') {
            return args.liveStats([
                ['Missing', missing],
                ['Queue', queue]
            ]);
        } else {
            let queueDataRecords = queueData?.records;
            let filteredQueue = queueDataRecords?.filter((x, index, arr) => {
                return x.trackedDownloadStatus == 'warning'
            })
            let totalWarnings = filteredQueue?.length ?? 0;

            return args.liveStats([
                ['Missing', missing],
                ['Queue', queue],
                ['Warnings', totalWarnings]
            ]);
        }
    }

    test(args) {
        let data = args.fetch(this.getUrl(args, 'wanted/missing') + '&sortKey=airDateUtc&sortDir=desc');
        return isNaN(data?.totalRecords) === false;
    }
}
