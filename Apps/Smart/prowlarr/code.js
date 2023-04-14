class Prowlarr {
    doFetch(args, endpoint) {
        var data = args.fetch({
            url: 'api/v1/' + endpoint + '?apikey=' + args.properties['apikey'],
            timeout: 5
        })
        return data;
    }

    status(args) {
        let indexer = this.doFetch(args, "indexer");
        let indexerStatus = this.doFetch(args, "indexerstatus");
        
        let indexerEnabled = indexer?.filter((x, index, arr) => {
            return x.enable == true;
        });
        let enabledCount = indexerEnabled?.length ?? 0;
        let failCount = indexerStatus?.length ?? 0;

        let working = enabledCount - failCount;

        return args.liveStats([
            ['Indexers Working', working + ' / ' + enabledCount]
        ]);
    }

    test(args) {
        let status = this.doFetch(args, "system/status");
        args.log("status", status);
        return status?.appName != null;
    }
}

