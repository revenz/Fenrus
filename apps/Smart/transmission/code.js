class transmission {

    doFetch(args, sessionId) {
        var data = args.fetch({
            url: `transmission/rpc`,
            timeout: 10,
            method: 'POST',
            body: "{\"arguments\": {\"fields\": [\"percentDone\",\"status\",\"rateDownload\",\"rateUpload\"]},\"method\": \"torrent-get\"}",
            headers: {
                "Accept": "text/html",
                "X-Transmission-Session-Id": sessionId,
                'Authorization': 'Basic ' + args.Utils.btoa(args.properties['username'] + ':' + args.properties['password'])
            }
        }).catch(err => console.log(err));

        return data;
    }

    async status(args) {
        let sessionId = ""
        if (args.properties['x-transmission-session-id'] != null) {
            sessionId = args.properties['x-transmission-session-id'];
        }
        let data = await this.doFetch(args, sessionId);
        let responseCode = data.status;
        if (responseCode == 409) {
            sessionId = data.headers.get('x-transmission-session-id');
            args.properties['x-transmission-session-id'] = sessionId;
            //if 409 retry query with session ID of previous run
            data = await this.doFetch(args, sessionId);
            responseCode = data.status;
        }
        let jsonData = await data.json();

        let torrents = jsonData?.arguments.torrents;
        let torrentCount = torrents.length;

        var rateDownload = 0;
        var rateUpload = 0;
        var completedTorrents = 0;
        for (var i = 0; i < torrents.length; i++) {
            var thisTorrent = torrents[i]

            rateDownload += thisTorrent.rateDownload;
            rateUpload += thisTorrent.rateUpload;
            if (thisTorrent.percentDone == 1) {
                completedTorrents += 1;
            }
        }
        var leech = torrentCount - completedTorrents;

        return args.liveStats([
            ['LEECH: ' + leech ?? 0, (args.Utils.formatBytes(rateDownload) ?? 0) + '/S'],
            ['SEED: ' + completedTorrents ?? 0, (args.Utils.formatBytes(rateUpload) ?? 0) + '/S']
        ]);

    }

    async test(args) {
        let data = await this.doFetch(args, "");
        let responseCode = data.status;
        let sessionId = data.headers.get('x-transmission-session-id');

        data = await this.doFetch(args, sessionId);
        responseCode = data.status;
        const jsonData = await data.json();
        console.log('data', jsonData);
        return jsonData.result == 'success';
    }
}

module.exports = transmission;
