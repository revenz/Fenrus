// https://glances.readthedocs.io/en/latest/api.html
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

    async status(args) {
        let data = await this.doFetch(args);
        let xmlStr = await data.text();
        var count = (xmlStr.match(/<\/url>/g) || []).length;
        return args.liveStats([
            ['Active jobs', count]
        ]);
    }

    async test(args) {
        let data = await this.doFetch(args);
        let xmlStr = await data.text();
        console.log("xmlStr", xmlStr);
        return xmlStr.includes('<buildUrls');
    }
}
module.exports = Jenkins;
