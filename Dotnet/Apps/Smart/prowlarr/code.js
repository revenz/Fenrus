class Prowlarr {
    doFetch(args, endpoint) {
        var data = args.fetch({
            url: 'api/v1/' + endpoint + '?apikey=' + args.properties['apikey'],
            timeout: 5
        })
        return data;
    }

    async status(args) {
        const [ indexer, indexerStatus ] = await Promise.all([
			await this.doFetch(args, "indexer"),
			await this.doFetch(args, "indexerstatus")
		]);
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

    async test(args) {
        let status = await this.doFetch(args, "system/status");
        console.log("status", status);
        return status?.appName != null;
    }
}

