class Sentry {
    getUrl(url) {
        if (!url) {
            return null;
        }

        if (url.endsWith('/')) {
            url = url.substring(0, url.length - 1);
        }

        if (url.endsWith('/api/0') === false) {
            url += '/api/0';
        }

        return url;
    }

    makeRequest(args, path) {
        let url = this.getUrl(args.url);
        if (!url) {
            return null;
        }

        return args.fetch({
            url: url + path,
            headers: {
                Authorization: 'Bearer ' + args.properties.apiToken
            }
        })
    }

    getProjects(args) {
        return this.makeRequest(args, '/projects/');
    }

    getEventCounts(args, project) {
        const org = project.organization.slug;
        const projectId = project.slug;
        return this.makeRequest(args, '/projects/' + org + '/' + projectId + '/stats/?stat=received&resolution=1d')
            .then(result => {
                return result[0][1];
            });
    }

    status(args) {
        const projects = this.getProjects(args);

        const counts = [];
        for (const project of projects) {
            let result = this.getEventCounts(args, project);
            counts.push(result);
        }

        let total = 0;
        for (let i = 0; i < counts.length; i++) {
            total += counts[i];
        }

        return args.liveStats([
            ['Last 24hr Events', total]
        ]);
    }

    test(args) {
        let data = this.makeRequest(args, '/');
        return data?.user;
    }
}
