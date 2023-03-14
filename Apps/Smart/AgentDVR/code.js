class AgentDVR {

    doFetch(args, endpoint) {
        var data = args.fetch({
            url:  endpoint,
            timeout: 5
        })
        return data;
    }

    status(args) {
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
		let snapshotMode = args.properties['snapshotMode'] ?? false;
		
		let endpoint = "";
		if(snapshotMode){
			endpoint = "grab.jpg";
		} else {
			endpoint = "video.mjpg";
		}
		let url = args.url;
		if(url.endsWith('/') === false)
			url += '/';
		args.changeIcon(url + endpoint + '?oids=' + cameraIds + '&size=' + resolution + '&maintainAR=' + dontScale.toString() + '&backColor=' + rgb);

        return;

    }

    test(args) {
        let data = this.doFetch(args, "command.cgi?cmd=getStatus");
        console.log('data', data);
        return isNaN(data?.devices) === false;
    }
}