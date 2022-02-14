function Lidarr_Status(args) {

    let url = function (endpoint) {
        return `api/v1/${endpoint}?apikey=${args.properties['apikey']}`;
    }

    args.fetch(url('wanted/missing'))
        .then(data => {

            let missing = data?.totalRecords ?? 0;

            args.fetch(url('queue'))
                .then(data => {

                    let queue = data?.totalRecords ?? 0;

                    args.liveStats([
                        ['Missing', missing],
                        ['Queue', queue]
                    ]);
                });
        });
}