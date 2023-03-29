class HomeAssistant {

    fetch(args, url) {
        let result = args.fetch(url);
        return result?.Result || result;
    }
    doFetch(args, template, isTest) {
        if (isTest) {
            return this.fetch(args, {
                url: `api/`,
                timeout: 10,
                method: 'GET',
                headers: {
                    'Authorization': 'Bearer ' + args.properties['apikey']
                }
            });
        } else {
            return this.fetch(args, {
                url: `api/template`,
                timeout: 10,
                method: 'POST',
                body: "{\"template\": \"" + template + "\"}",
                headers: {
                    "Accept": "text/html",
                    'Authorization': 'Bearer ' + args.properties['apikey']
                }
            });
        }
    }
    status(args) {

        let firstTemplate = args.properties['firstStatTemplate'];
        let secondTemplate = args.properties['secStatTemplate'];

        let firstTemplateRespText = this.doFetch(args, firstTemplate, false);
        let secondTemplateRespText = this.doFetch(args, secondTemplate, false);

        let firstTitle = args.properties['firstStatTitle'];
        let secondTitle = args.properties['secStatTitle'];
		
        return args.liveStats([
            [firstTitle, firstTemplateRespText],
            [secondTitle, secondTemplateRespText]
        ]);
    }

    test(args) {
        let data = this.doFetch(args, "", true);
        return data?.message == 'API running.';
    }
}

