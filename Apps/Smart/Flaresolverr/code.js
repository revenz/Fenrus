class Flaresolverr {

status(args) {
        const data = args.fetch(args.url);
        
        if(!data?.version)
                return args.liveStats([
                        ['Version','Err'],
                        ['Up','false'],
                ]);
        

                return args.liveStats([
                        ['Version',data.version],
                        ['Up',data.msg == 'FlareSolverr is ready!'],
                ]);
        }
        test(args) {
                const data = args.fetch(args.url);
                return data.msg == 'FlareSolverr is ready!';
        }
}
