class Deluge
{   
    callCount = 0;

    async getToken(args){
        let password = args.properties['password'];

        try
        {
            let res = await args.fetchResponse({
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
            console.log('Failed to retrieve Deluge access token', err);
            return;
        }
    }

    
    async getData(args){
        let token = await this.getToken(args);     
        let data = await args.fetch({
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
            console.log('Deluge error: ' + data.error.message);
            return;
        }
        return data;
    }

    async status(args) {
        let data = await this.getData(args);
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

    async test(args) {
        let token = await this.getToken(args);
        return !!token;
    }
}
