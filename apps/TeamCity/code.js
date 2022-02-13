function TeamCity_Status(args) {

    doFetch = function (endpoint) {
        return args.fetch({ url: 'app/rest/' + endpoint, headers: { 'Authorization': 'Bearer ' + args.properties['token'] } })
    }

    doFetch('buildQueue').then(data => {
        let queue = data?.count ?? 0;

        doFetch('builds?locator=running:true').then(data => {
            if (data?.count > 1) {
                args.liveStats([
                    ['Queue', queue],
                    ['Building', data.count]
                ]);
            }
            else if (data?.count == 1) {
                args.liveStats([
                    ['Queue', queue],//['Building', data.build[0].buildTypeId],
                    ['Percent', data.build[0].percentageComplete + '%']
                ]);
            }
            else {
                doFetch('builds').then(data => {
                    args.liveStats([
                        ['Queue', queue],
                        ['Total', data?.build[0]?.id ?? '0']
                    ]);
                });
            }
        });

    });
}