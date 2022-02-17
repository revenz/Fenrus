class NextCloud
{
    doFetch (args) {
        return args.fetch({
            url: `ocs/v1.php/cloud/users/${args.properties['username']}?format=json`, timeout: 10, headers: {
                'Authorization': 'Basic ' + args.Utils.btoa(args.properties['username'] + ':' + args.properties['password']),
                'OCS-APIRequest': 'true'
            }
        });
    }   

    async status(args) {
        let data = await this.doFetch(args);
        let free = data?.ocs?.data?.quota?.free ?? 0;
        //let total = data?.ocs?.data?.quota?.total ?? 0;
        let used = data?.ocs?.data?.quota?.used ?? 0;

        return args.liveStats([
            ['Used', args.Utils.formatBytes(used)],
            ['Free', args.Utils.formatBytes(free)]
        ]);
    }
    
    async test(args) {
        let data = await this.doFetch(args);
        return isNaN(data?.ocs?.data?.quota?.free) === false;
    }
}

module.exports = NextCloud;