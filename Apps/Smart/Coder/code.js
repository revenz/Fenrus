class Coder {

    cleanUrl(args)
    {
        if(args.url.endsWith('/'))
            return args.url.substring(0, args.url.length - 1);
        else
            return args.url
    }
    
    fetchWorkspaces(args) {
        let cleanedUrl = this.cleanUrl(args)

        let allWorkspacesData =  args.fetch({
            url: cleanedUrl + "/api/v2/workspaces",
            method: "GET",
            headers: { 
                'Coder-Session-Token': args.properties['token'],
                'Accept': 'application/json'
         }
        }).data;

        let totalWorkspaces = allWorkspacesData.count;

        let runningWorkspacesData =  args.fetch({
            url: cleanedUrl + "/api/v2/workspaces?q=status:running",
            method: "GET",
            headers: { 
                'Coder-Session-Token': args.properties['token'],
                'Accept': 'application/json'
         }
        }).data;

        let runningWorkspaces = runningWorkspacesData.count;

        return { "total": totalWorkspaces, "active": runningWorkspaces }
    }

    status(args) {
        
        let results = this.fetchWorkspaces(args)
        
        return args.liveStats([
            ['Active Workspaces', results.active],
            ['Total Workspaces', results.total]
        ]);
    }

    test(args) {
       
        if (!args.properties || !args.properties['token']) {
            args.log("No API Key");
            return false
        }

        let results =  args.fetch({
            url: this.cleanUrl(args) + "/api/v2/workspaces",
            method: "GET",
            headers: { 
                'Coder-Session-Token': args.properties['token'],
                'Accept': 'application/json'
            }
        });

        if (results.status == 200) {
            return true
        } else {
            args.log(JSON.stringify(results));
            return false
        }
    }
}