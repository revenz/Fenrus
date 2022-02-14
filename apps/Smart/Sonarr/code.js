function getUrl(args, endpoint){
    return `api/v3/${endpoint}?apikey=${args.properties['apikey']}`;
}

module.exports = { 
    status: (args) => {
        return new Promise((resolve, reject) => {
            let url = getUrl(args, 'wanted/missing') + '&sortKey=airDateUtc&sortDir=desc';

            args.fetch(url)
                .then(data => {

                    let missing = data?.totalRecords ?? 0;

                    args.fetch(getUrl(args, ('queue')))
                        .then(data => {

                            let queue = data?.totalRecords ?? 0;

                            resolve(
                                args.liveStats([
                                    ['Missing', missing],
                                    ['Queue', queue]
                                ])
                            );
                        }).catch(error => {
                            reject(error);
                        });
                }).catch(error => {
                    reject(error);
                }
            );
        })
    },

    
    test: (args) => {
        return new Promise(function (resolve, reject) {            
            args.fetch(getUrl(args, 'wanted/missing') + '&sortKey=airDateUtc&sortDir=desc').then(data => {
                if (data?.totalRecords === 0 || data?.totalRecords)
                    resolve();
                else
                    reject();
            }).catch((error) => {
                reject(error);
            });
        })
    }
}