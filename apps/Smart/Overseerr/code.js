module.exports = 
{ 

    status: (args) => {
        return new Promise((resolve, reject) => {

            if (!args.properties || !args.properties['apikey']){
                reject('No API Key');
                return;
            }

            args.fetch({ url: 'api/v1/request/count', headers: { 'X-Api-Key': args.properties['apikey']} })        
                .then(data => {                    
                    resolve(
                        args.liveStats([
                            ['Pending', data.pending],
                            ['Processing', data.processing ]
                        ])
                    );
                }).catch(error => {
                    reject(error);
                });
        });
    },

    test: (args) => {
        return new Promise(function (resolve, reject) {
            args.fetch({ url: 'api/v1/request/count', headers: { 'X-Api-Key': args.properties['apikey'] } })
            .then(data => {
                if (data.pending === 0 || data.pending)
                    resolve();
                else
                    reject();
            }).catch((error) => {
                reject(error);
            });
        });
    }
}