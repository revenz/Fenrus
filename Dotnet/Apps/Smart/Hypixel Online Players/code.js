class HypixelOnlinePlayers {
    static API_URL = "https://api.hypixel.net/counts";

    async status(args) {
        const apikey = args.properties.apikey;
        const data = await args.fetch({
            url: HypixelOnlinePlayers.API_URL,
            headers: {
                'API-Key': apikey
            }
        });
        const online = data?.playerCount ?? 0;

        return args.liveStats([
            ['Online Players', online]
        ]);
    }

    async test(args) {
        const apikey = args.properties.apikey;
        const data = await args.fetch({
            url: HypixelOnlinePlayers.API_URL,
            headers: {
                'API-Key': apikey
            }
        });

        return data?.success ?? false;
    }
}

module.exports = HypixelOnlinePlayers;