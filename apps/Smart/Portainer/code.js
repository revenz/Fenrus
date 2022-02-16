
module.exports = { 
    status: async (args) => {
        let data = args.fetch({
            url: 'api/endpoints?limit=100&start=0',
            headers: { 'Authorization': 'Bearer ' + args.properties['token']}
        });
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
        return args.liveStats([
            ['Running', running],
            ['Stopped', stopped]
        ]);
    },
    
    test: async (args) => {
        let data = await args.fetch({
            url: 'api/endpoints?limit=100&start=0',
            headers: { 'Authorization': 'Bearer ' + args.properties['token']}
        });
        return (data && typeof (data[Symbol.iterator]) === 'function');
    }
}