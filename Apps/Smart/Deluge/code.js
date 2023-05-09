class Deluge
{   
    callCount = 0;

    fetch(args, url) {
        let result = args.fetch(url);
        args.log('Fetching URL: ' + url);
        return result.data;
    }

    getToken(args){
        let password = args.properties['password'];

        try
        {            
            let res = args.fetch({
                url: 'json',
                method: 'POST',
                headers: {
                    'content-type': 'application/json'
                },
                body: JSON.stringify({
                    "id":++this.callCount,
                    "method":"auth.login",
                    "params": [password]
                })
            });

            args.log('trying to get cookie');
            let sessionId = res.cookies['_session_id'];
            if(!sessionId) {
                args.log('Failed to retrieve Deluge access token from cookies');
                return;
            }
            args.log('sessionId: ' + sessionId);
            return sessionId;
        }
        catch(err)
        {
            args.log('debug 10');
            args.log('Failed to retrieve Deluge access token', err);
            return;
        }
    }

    
    getData(args){
        let token = this.getToken(args);     
        let data = args.fetch({
            url: 'json', 
            method: 'POST',
            headers: {
                'content-type': 'application/json',
                'cookie': '_session_id=' + token
            },
            body: JSON.stringify({
                "id": ++this.callCount,
                "method": "web.update_ui",
                "params": [["none"], {}]
            })
        });
        
        if(!data.success)
        {
            args.log('Deluge error: ' + data.Content);
            return;
        }
        return data.Data;
    }

     status(args) {
        let data = this.getData(args);
        if(!data)
            return;

        let download_rate = data.result?.stats?.download_rate || 0;
        let upload_rate = data.result?.stats?.upload_rate || 0;

        let downloading = data.result.filters.state?.find(x => x[0] === 'Downloading');
        if(downloading)
            downloading = downloading[1];
        let seeding = data.result.filters.state?.find(x => x[0] === 'Seeding');
        if(seeding)
            seeding = seeding[1];
        args.log(`upload_rate: [${upload_rate}]`);
        
        return args.liveStats([
            ['Downloading', args.Utils.formatBytes(download_rate) + '/s'],
            ['Seeding', args.Utils.formatBytes(upload_rate) + '/s']
        ]);
    }

    test(args) {
        let token = this.getToken(args);
        return !!token;
    }
}
