function getUrl(args, endpoint){
    return `api/v3/${endpoint}?apikey=${args.properties['apikey']}`;
}

module.exports = { 
    status: (args) => {
        return new Promise((resolve, reject) => {

        args.fetch(getUrl(args, 'movie'))
            .then(data => {
                let missing = data?.filter(x => x.hasFile == false)?.length ?? 0;
                args.fetch(getUrl(args, 'queue'))
                    .then(data => {

                        let queue = data?.records?.length ?? 0;
                        
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
            });
        });
    },
    
    test: (args) => {
        return new Promise(function (resolve, reject) {            
            args.fetch(getUrl(args, 'movie')).then(data => {
                if (data?.records?.length === 0 || data?.records?.length)
                    resolve();
                else
                    reject();
            }).catch((error) => {
                reject(error);
            });
        })
    }
}