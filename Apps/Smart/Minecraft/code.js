class Minecraft {

    getMCUrl(args) {
        let mcUrl = "";
        //ignore standard URL if hostname provided
        if (args.properties['hostname']) {
            mcUrl = args.properties['hostname'];
        } else {
            //to avoid confusion allow users to use standard URL 
            //remove http (due to HTTP regex force)
            mcUrl = args.url.replace(/^https?:\/\//, '');
            //remove trailing slash
            mcUrl = mcUrl.replace(/\/+$/, '');
        }
        if (args.properties['port']) {
            mcUrl += ":" + args.properties['port'];
        }
        return mcUrl;
    }

    doFetch(args) {
        return args.fetch({
            url: `https://mcapi.us/server/status?ip=` + this.getMCUrl(args),
            timeout: 10,
        });
    }

    async status(args) {

        let data = await this.doFetch(args);
        if (!data || data?.status == 'error' || data?.online == 'false') {
            return args.liveStats([
                ['Server Status', 'Offline']
            ]);
        }
        let playerCount = data?.players?.now;
        let maxPlayerCount = data?.players?.max;

        let playerList = data?.players?.sample;
        let playerNames = playerList.map(x => x?.name)
        //cant fit more than 6 for most views
        if (playerNames && playerNames.length > 0 && playerNames.length <= 6) {
            return args.liveStats([
                ['Player Count', playerCount + '/' + maxPlayerCount],
                ['Players Online', playerNames.join(", ")]
            ]);
        } else {
            return args.liveStats([
                ['Players Online', playerCount + '/' + maxPlayerCount]
            ]);
        }
    }

    async test(args) {
        let data = await this.doFetch(args);
        console.log('data', data);
        return data?.status == 'success';
    }
}
