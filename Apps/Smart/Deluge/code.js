class Deluge
{   
    callCount = 0;

    getToken(args){
        let password = args.properties['password'];

        try
        {
            let res = args.fetchResponse({
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

            let cookie = res.headers.get('set-cookie');
            let sessionId = /(?<=(_session_id=))[^;]+/.exec(cookie)[0];
            return sessionId;
        }
        catch(err)
        {
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
        if(data.error)
        {
            args.log('Deluge error: ' + data.error.message);
            return;
        }
        return data;
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
