module.exports = { 

    status: (args) => {
        return new Promise((resolve, reject) => {
            args.fetch('api/status').then(data => {
                if (!data || isNaN(data.queue)) {
                    resolve();
                    return;
                }
                let secondlbl = 'Time';
                let secondValue = data.time;

                if (!data.time) {
                    if (!data.processing) {
                        secondlbl = 'Processed';
                        secondValue = data.processed;
                    }
                    else {
                        secondlbl = 'Processing';
                        secondValue = data.processing;
                    }
                } 

                resolve(
                    args.liveStats([
                        ['Queue', data.queue],
                        [secondlbl, secondValue]
                   ])
                );
            }).catch(error => {
                reject(error);
            });
        })
    },

    test: (args) => {
        return new Promise(function (resolve, reject) {
            args.fetch(args.url + '/api/status').then(data => {
                if (data.processed === 0 || data.processed)
                    resolve();
                else
                    reject();
            }).catch((error) => {
                reject(error);
            });
        })
    }
}