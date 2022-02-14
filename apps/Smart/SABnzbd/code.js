module.exports = { 

    status: (args) => {
        return new Promise((resolve, reject) => {
            args.fetch(`api?output=json&apikey=${args.properties['apikey']}&mode=queue`)
                .then(data => {

                    if (isNaN(data?.queue?.mbleft) || isNaN(data?.queue?.kbpersec)){
                        resolve();
                        return;
                    }

                    let mbleft = parseFloat(data.queue.mbleft, 10);
                    if(isNaN(mbleft))
                        mbleft = 0;                    
                    mbleft = args.Utils.formatBytes(mbleft * 1000 * 1000);

                    let kbpersec = parseFloat(data.queue.kbpersec, 10);
                    if(isNaN(kbpersec))
                        kbpersec = 0;                    
                    kbpersec = args.Utils.formatBytes(kbpersec * 1000) + '/s';

                    resolve(
                        args.liveStats([
                            ['Queue', mbleft],
                            ['Speed', kbpersec ]
                        ])
                    );
                });
            });
    },

    test: (args) => {
        return new Promise(function (resolve, reject) {
            args.fetch(`api?output=json&apikey=${args.properties['apikey']}&mode=queue`).then(data => {
                if (isNaN(data?.queue?.mbleft) === false)
                    resolve();
                else
                    reject();
            }).catch((error) => {
                reject(error);
            });
        })
    }
}