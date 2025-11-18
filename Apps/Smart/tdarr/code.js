class Tdarr {
    status(args) {


        let url = args.url;
        if (url.endsWith('/'))
            url = url.substring(0, url.length - 1);

        const data = args.fetch(
            {
                url: `${url}/api/v2/stats/get-pies`,
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    Accept: 'application/json',
                },
                body: JSON.stringify({
                    data: {
                        libraryId: ''
                    }
                }),
            },
        ).data;

        if (!data) {
            throw 'no data';
        }

        var pieStats = data.pieStats;
        if (!pieStats) {
            throw 'no piestats';
        }
        const processed = pieStats.totalTranscodeCount;
        const total = pieStats.totalFiles;

        let results = [
            ['Proc / Total', `${processed} / ${total}`]]

        if (args.properties['showSpaceSaved'] == 'true') {
            const sizeSaved = args.Utils.formatBytes(pieStats.sizeDiff * 1000000000)
            results.push(['Space saved', sizeSaved])
        }

        return args.liveStats(results);
    }

    test(args) {
        let url = args.url;
        if (url.endsWith('/'))
            url = url.substring(0, url.length - 1);
        const data = args.fetch(`${url}/api/v2/status`).data;
        return (data.status === 'good');
    }
}

