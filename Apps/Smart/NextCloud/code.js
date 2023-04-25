class NextCloud
{
    doFetch (args) {
        let url = `ocs/v2.php/cloud/users/${args.properties['username']}?format=json`;
        return args.fetch({
            url: url, timeout: 10, headers: {
                'Authorization': 'Basic ' + args.Utils.btoa(args.properties['username'] + ':' + args.properties['password']),
                'OCS-APIRequest': 'true'
            }
        });
    }   

    status(args) {
        let data = this.doFetch(args);
        let free = data?.ocs?.data?.quota?.free ?? 0;
        let used = data?.ocs?.data?.quota?.used ?? 0;
        args.log('mode: ' + args.properties['mode']);
        args.log('free: ' + free);
        args.log('used: ' + used);
		if(args.properties['mode'] == 'bar'){
            let percent = Math.round((used / (used + free)) * 10000)/100;
            args.log('Percent: ' + percent);
			return args.barInfo([{label:'Storage', percent: percent  || 0, icon: '/common/hdd.svg'}]);
        } else {
			return args.liveStats([
				['Used', args.Utils.formatBytes(used)],
				['Free', args.Utils.formatBytes(free)]
			]);
		}
    }
    
    test(args) {
        let data = this.doFetch(args);
        return isNaN(data?.ocs?.data?.quota?.free) === false;
    }
}
