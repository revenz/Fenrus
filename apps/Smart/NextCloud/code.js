function doFetch (args) {
    return args.fetch({
        url: `ocs/v1.php/cloud/users/${args.properties['username']}?format=json`, timeout: 10, headers: {
            'Authorization': 'Basic ' + args.Utils.btoa(args.properties['username'] + ':' + args.properties['password']),
            'OCS-APIRequest': 'true'
        }
    });
}

module.exports = { 

    status: (args) => {
        return new Promise((resolve, reject) => {
            doFetch(args).then(data => {
                let free = data?.ocs?.data?.quota?.free ?? 0;
                //let total = data?.ocs?.data?.quota?.total ?? 0;
                let used = data?.ocs?.data?.quota?.used ?? 0;

                resolve(
                    args.liveStats([
                        ['Used', args.Utils.formatBytes(used)],
                        ['Free', args.Utils.formatBytes(free)]
                    ])
                );
            });
        });
    },
    
    test: (args) => {
        return new Promise(function (resolve, reject) {
            doFetch(args).then(data => {
                console.log('data', data);
                if (isNaN(data?.ocs?.data?.quota?.free) === false)
                    resolve();
                else
                    reject();
            }).catch((error) => {
                reject(error);
            });
        })
    }
}
