function PlexUrl(endpoint, args) {
    return `library/${endpoint}?X-Plex-Token=${args.properties['token']}`;
}

function Plex_Status(args) {

    args.fetch(PlexUrl('recentlyAdded', args)).then(data => {
        let recent = data?.MediaContainer?.size ?? 0;
        args.fetch(PlexUrl('onDeck', args)).then(data => {
            let ondeck = data?.MediaContainer?.size ?? 0;
            args.liveStats([
                ['Recent', recent],
                ['On Deck', ondeck]
            ]);
        });
    });
}

function Plex_Test(args) {
    return new Promise(function (resolve, reject) {
        let url = PlexUrl('recentlyAdded', args);
        args.fetch(url).then(data => {
            if (isNaN(data.MediaContainer?.size))
                reject();
            else
                resolve();
        }).catch((error) => {
            reject(error);
        });
    })
}