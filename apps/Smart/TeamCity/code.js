function doFetch (args, endpoint) {
    return args.fetch({ url: 'app/rest/' + endpoint, headers: { 'Authorization': 'Bearer ' + args.properties['token'] } })
}


module.exports = { 
    status: (args) => {
        return new Promise((resolve, reject) => {
            doFetch(args, 'buildQueue').then(data => {
                let queue = data?.count ?? 0;

                doFetch(args, 'builds?locator=running:true').then(data => {
                    if (data?.count > 1) {
                        resolve(                            
                            args.liveStats([
                                ['Queue', queue],
                                ['Building', data.count]
                            ])
                        );
                    }
                    else if (data?.count == 1) {
                        resolve(
                            args.liveStats([
                                ['Queue', queue],//['Building', data.build[0].buildTypeId],
                                ['Percent', data.build[0].percentageComplete + '%']
                            ])
                        );
                    }
                    else {
                        doFetch(args, 'builds').then(data => {
                            resolve(
                                args.liveStats([
                                    ['Queue', queue],
                                    ['Total', data?.build[0]?.id ?? '0']
                                ])
                            );
                        }).catch(error => {
                            reject(error);
                        });
                    }
                }).catch(error => {
                    reject(error);
                });
            
            }).catch(error => {
                reject(error);
            });
        });
    },
    
    test: (args) => {
        return new Promise(function (resolve, reject) {
            doFetch(args, 'buildQueue').then(data => {
                if (isNaN(data?.count) === false)
                    resolve();
                else
                    reject();
            }).catch((error) => {
                reject(error);
            });
        })
    }
}