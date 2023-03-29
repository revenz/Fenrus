class Jenkins {
    doFetch(args) {
        return args.fetch({
            url: 'computer/api/xml?tree=computer[executors[currentExecutable[url]]]&depth=1&xpath=//url&wrapper=buildUrls',
            timeout: 10,
            headers: {
                "Accept": "text/html",
                'Authorization': 'Basic ' + args.Utils.btoa(args.properties['username'] + ':' + args.properties['password'])
            }
        });
    }

    status(args) {
        let xmlStr = this.doFetch(args);
        var count = (xmlStr.match(/<\/url>/g) || []).length;
        return args.liveStats([
            ['Active jobs', count]
        ]);
    }

    test(args) {
        let xmlStr = this.doFetch(args);
        console.log("xmlStr", xmlStr);
        return xmlStr.includes('<buildUrls');
    }
}
