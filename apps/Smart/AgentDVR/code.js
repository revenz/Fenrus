class AgentDVR {

    doFetch(args, endpoint) {
        var data = args.fetch({
            url:  endpoint,
            timeout: 5
        })
        return data;
    }

    async status(args) {
		let resolution = args.properties['resolution'] || (
						args.size === 'small' ? '44x44' : 
						args.size === 'medium' ? '104x104' : 
                        args.size === 'large' ? '344x104' : 
                        args.size === 'x-large' ? '224x224' : 
                        args.size === 'xx-large' ? '344x344' : 
                        '344x344'
                   );
				   
		let cameraIds = args.properties['cameraIds'] ?? 1;
		let dontScale = !args.properties['scale'] ?? true;
		let rgb = args.properties['rgb'] ?? '0,0,0';
		
		console.log('args.size ', args.size );
		console.log('resolution', resolution );
		let url = args.url;
		if(url.endsWith('/') === false)
			url += '/';
		args.changeIcon(url + 'grab.jpg?oids=' + cameraIds + '&size=' + resolution + '&maintainAR=' + dontScale.toString() + '&backColor=' + rgb);

        return;

    }

    async test(args) {
        let data = await this.doFetch(args, "command.cgi?cmd=getStatus");
        console.log('data', data);
        return isNaN(data?.devices) === false;
    }
}

module.exports = AgentDVR;
