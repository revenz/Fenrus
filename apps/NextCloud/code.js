function NextCloud_Status(args) {
    args.fetch({
        url: `ocs/v1.php/cloud/users/${args.properties['username']}?format=json`, timeout: 10, headers: {
            'Authorization': 'Basic ' + window.btoa(args.properties['username'] + ':' + args.properties['password']),
            'OCS-APIRequest': 'true'
        }
    }).then(data => {
        let free = data?.ocs?.data?.quota?.free ?? 0;
        let total = data?.ocs?.data?.quota?.total ?? 0;
        let used = data?.ocs?.data?.quota?.used ?? 0;

        args.liveStats([
            ['Used', args.Utils.formatBytes(used)],
            ['Free', args.Utils.formatBytes(free)]
        ]);
    });
}