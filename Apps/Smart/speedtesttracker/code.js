// https://glances.readthedocs.io/en/latest/api.html
class SpeedtestTracker {
    doFetch(args, endpoint) {
        return args.fetch({
            url: `api/speedtest/` + endpoint,
            timeout: 10
        }).data;
    }

    status(args) {
        let mode = args.properties['mode'];

        if (mode === 'graph') {
            return this.chartDownAndUp(args);
        } else {
            return this.liveStats(args, mode);
        }
    }

    chartDownAndUp(args) {
        let stats = this.doFetch(args, '');
        if (!stats?.data?.data)
            return;
        let labels = stats.data.data.map(x => new Date(x.created_at));
        let data = [
            stats.data.data.map(x => x.download),
            stats.data.data.map(x => x.upload)
        ];

        //ratio stats out of 100 so that they fit on graph
        let maxValue = args.properties['maxY'];
        let yRatio = 100 / maxValue;
        for (var i = 0; i < data.length; i++) {
            for (var j = 0; j < data[i].length; j++) {
                data[i][j] = data[i][j] * yRatio;
            }
            //reverse as first entry is latest
            data[i].reverse()
        }
        let title = Math.round(+stats.data.data[0].download) + ' Mbit/s     \n' + Math.round(+stats.data.data[0].upload) + ' Mbit/s';
        //let title = 'Speedtest Mb/s';

        return args.chart.line({
            title,
            labels,
            data
        });
    }

    liveStats(args, mode) {
        let stats = this.doFetch(args, 'latest');

        let ping = 0;
        let download = 0;
        let upload = 0;
        if (mode == 'latest') {
            ping = stats?.data?.ping;
            download = stats?.data?.download;
            upload = stats?.data?.upload;

        } else if (mode == 'average') {
            ping = stats?.average?.ping;
            download = stats?.average?.download;
            upload = stats?.average?.upload;

        } else if (mode == 'max') {
            ping = stats?.maximum?.ping;
            download = stats?.maximum?.download;
            upload = stats?.maximum?.upload;

        } else if (mode == 'lowest') {
            ping = stats?.minimum?.ping;
            download = stats?.minimum?.download;
            upload = stats?.minimum?.upload;

        } else {
            return;
        }

        return args.liveStats([
            ['Download', Math.round(download * 100) / 100 + ' Mb/s'],
            ['Upload', Math.round(upload * 100) / 100 + ' Mb/s'],
            ['Ping', Math.round(ping * 100) / 100 + 'ms']
        ]);

    }

    test(args) {
        let data = this.doFetch(args, 'latest');
        args.log('data', data);
        return isNaN(data?.data?.id) === false;
    }
}

