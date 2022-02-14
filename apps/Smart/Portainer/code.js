
module.exports = { 
    status: (args) => {
        return new Promise((resolve, reject) => {
            args.fetch({
                url: 'api/endpoints?limit=100&start=0',
                headers: { 'Authorization': 'Bearer ' + args.properties['token']}
            }).then(data => {
                let running = 0;
                let stopped = 0;

                if (data && typeof (data[Symbol.iterator]) === 'function') {
                    for (let instance of data) {
                        if (instance?.Snapshots) {
                            for (let snapshot of instance.Snapshots) {
                                running += snapshot.RunningContainerCount;
                                stopped += snapshot.StoppedContainerCount;
                            }
                        }
                    }
                }
                resolve(
                    args.liveStats([
                        ['Running', running],
                        ['Stopped', stopped]
                    ])
                );            
            }).catch(error => {
                reject(error);
            });
        })
    }
}