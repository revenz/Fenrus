function getUrl(endpoint, args) {
    return `library/${endpoint}?X-Plex-Token=${args.properties['token']}`;
}
module.exports = { 
    
    status: async (args) => {
        let data = await args.fetch(getUrl('recentlyAdded', args));
        let recent = data?.MediaContainer?.size ?? 0;
        data = await args.fetch(getUrl('onDeck', args));
        let ondeck = data?.MediaContainer?.size ?? 0;
        return args.liveStats([
            ['Recent', recent],
            ['On Deck', ondeck]
        ]);
    },

    test: async (args) => {
        let url = getUrl('recentlyAdded', args);
        let data = await args.fetch(url);
        return isNaN(data.MediaContainer?.size) === false;
    }
}