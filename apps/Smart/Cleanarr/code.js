class Cleanarr {
    doFetch(args, endpoint) {
        return args.fetch({
            url: endpoint,
            timeout: 18000
        });
    }

    async status(args) {
        let data = await this.doFetch(args, 'content/dupes');

        let count = data?.length;
        let totalSizeOfDups = 0;

        for (const fileData of data) {
            totalSizeOfDups += fileData.media[1].parts[0].size;
        }
        return args.liveStats([
            ['Duplicate Count', count],
            ['Size of Duplicates', args.Utils.formatBytes(totalSizeOfDups)]
        ]);
    }

    async test(args) {
        let data = await this.doFetch(args, 'server/info');
        console.log("data", data);
        return data != null && data.name != null && data.name.length > 0;
    }
}

module.exports = Cleanarr;
