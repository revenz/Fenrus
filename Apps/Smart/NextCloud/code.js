class NextCloud
{
    doFetch (args) {
        args.log('NextCloud Username: ' + args.properties['username']);
        args.log('NextCloud Password: ' + args.properties['password']);
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
        let used = data?.ocs?.data?.quota?.used ?? 0;

		
		if(args.properties['mode'] == 'bar'){
			return args.barInfo([{label:'Storage', percent: Math.round((used / (free + used)) * 10000)/100  || 0, icon: '/common/hdd.svg'}]);
        } else {
			return args.liveStats([
				['Used', args.Utils.formatBytes(used)],
				['Free', args.Utils.formatBytes(free)]
			]);
		}
    }
    
    async test(args) {
        let data = await this.doFetch(args);
        return isNaN(data?.ocs?.data?.quota?.free) === false;
    }
}
