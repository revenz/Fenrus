class Plex
{ 
    getUrl(endpoint, args) {
        return `library/${endpoint}?X-Plex-Token=${args.properties['token']}`;
    }
    
    async status(args) {
        let data = await args.fetch(this.getUrl('recentlyAdded', args));
        let recent = data?.MediaContainer?.size ?? 0;
        data = await args.fetch(this.getUrl('onDeck', args));
        let ondeck = data?.MediaContainer?.size ?? 0;
        return args.liveStats([
            ['Recent', recent],
            ['On Deck', ondeck]
        ]);
    }

    async test(args) {
        let url = this.getUrl('recentlyAdded', args);
        let data = await args.fetch(url);
        return isNaN(data.MediaContainer?.size) === false;
    }
}

module.exports = Plex;