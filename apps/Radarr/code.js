function Radarr_Status(args) {

    let url = function(endpoint) {
        return `api/v3/${endpoint}?apikey=${args.properties['apikey']}`;
    }

    args.fetch(url('movie'))
        .then(data => {

            let missing = data?.filter(x => x.hasFile == false)?.length ?? 0;

            args.fetch(url('queue'))
                .then(data => {

                    let queue = data?.records?.length ?? 0;

                    args.liveStats([
                        ['Missing', missing],
                        ['Queue', queue]
                    ]);
                });
        });
}