class Synology
{

	sessionId(args){
		const user = args.properties['username'];
		const password = args.properties['password'];
        if(!user || !password)
        return;

        try
        {
            let res = args.fetch({
                url:`${args.url}/webapi/auth.cgi?api=SYNO.API.Auth&version=6&method=login&account=${user}&passwd=${password}`,
                timeout: 5000,
			}).data;
            let sessionId = res.data.sid
			
            return sessionId;
		}
        catch(err)
        {
            args.log('Failed to retrieve sessionId', err);
            return;
		}
	}
	getData(args){
        let sessionId = this.sessionId(args);

        if(!sessionId)
        return;

        let data = args.fetch({
            url: `${args.url}/webapi/entry.cgi?api=SYNO.Core.System.Utilization&method=get&version=1&_sid=${sessionId}`,
            timeout: 10000, // 10 seconds
            method: 'POST',
            headers: {
                'content-type': 'application/json',
			},
            body: JSON.stringify({
				"stop_when_error":"false",
				"mode":"parallel",
				"api":"SYNO.Entry.Request",
				"method":"request",
				"version":"1"
			})
		});

		if(data.success === false)
		    return;
		
		let logout = args.fetch({
			url:`${args.url}/webapi/auth.cgi?api=SYNO.API.Auth&version=6&method=logout&_sid=${sessionId}`,
		});
        return data.data;
	}

	status(args) {
        let data = this.getData(args);
        if(!data)
		return;
		
        let cpu = data.data.cpu?.system_load || 0;
        let ram = data.data.memory?.real_usage || 0;
		
        return args.barInfo([{label:'CPU', percent: data.data.cpu?.system_load || 0,icon:'common/cpu.svg',},{label:'RAM', percent: data.data.memory?.real_usage || 0,icon:'/common/ram.svg',},]);
		
	}
	test(args) {
		let token = this.sessionId(args);
        return !!token;
	}
}