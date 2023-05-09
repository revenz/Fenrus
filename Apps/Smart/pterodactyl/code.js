class Pterodactyl {
    doFetch(args, endpoint) {
        return args.fetch({
            url: `api/client/` + endpoint,
            timeout: 10,
            headers: {
                "Accept": "Application/vnd.pterodactyl.v1+json",
                'Authorization': 'Bearer ' + args.properties['apiKey']
            }
        }).data;
    }

    status(args) {
        let data = null
        if (args.properties['serverKey']) {
            data = this.doFetch(args, 'servers/' + args.properties['serverKey'] + '/resources');
            return args.liveStats([
                ['State', data?.attributes?.current_state ?? 'Unreachable'],
                ['CPU', data?.attributes?.resources?.cpu_absolute ?? 0 + '%'],
                ['Memory', args.Utils.formatBytes(data?.attributes?.resources?.memory_bytes ?? 0)],
                ['Network Up/Down', args.Utils.formatBytes(data?.attributes?.resources?.network_rx_bytes ?? 0) + " / " + args.Utils.formatBytes(data?.attributes?.resources?.network_tx_bytes ?? 0)]
            ]);
			
        } else {
			
            args.log("Pterodactyl: serverKey param not provided to grabbing data for first server which can be found");
            let serverList = this.doFetch(args, '');
            if (serverList?.data != null && serverList?.data.length > 0) {
                const serverObj = serverList?.data[0];
                let name = serverObj?.attributes?.name;
                args.log("Name: " + name + ", ID: " + serverObj?.attributes?.identifier)
                data = this.doFetch(args, 'servers/' + serverObj?.attributes?.identifier + '/resources');
                let title = (name.length > 18) ? [name] : ['Server Name', name];
                return args.liveStats([
                    title,
                    ['State', data?.attributes?.current_state ?? 'Unreachable'],
                    ['CPU/Memory', (data?.attributes?.resources?.cpu_absolute ?? 0) + '% / ' + (args.Utils.formatBytes(data?.attributes?.resources?.memory_bytes) ?? 0)],
                    ['Network Up/Down', args.Utils.formatBytes(data?.attributes?.resources?.network_rx_bytes ?? 0) + " / " + args.Utils.formatBytes(data?.attributes?.resources?.network_tx_bytes ?? 0)]
                ]);
            }
        }
    }

    test(args) {
        let data = this.doFetch(args, '');
        return isNaN(data?.meta?.pagination?.total) === false;
    }
}
