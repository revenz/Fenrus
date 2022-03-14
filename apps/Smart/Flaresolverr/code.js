class Flaresolverr {

async status(args) {
        const [ data ] = await Promise.all([
                await args.fetch(`${args.url}`)
                ]);
        if(!data?.version)
                return args.liveStats([
                        ['Version','Err'],
                        ['Up','false'],
                ]);
        console.log(data);

                return args.liveStats([
                        ['Version',data.version],
                        ['Up',data.msg == 'FlareSolverr is ready!'],
                ]);
        }
        async test(args) {
                const data = await args.fetch(`${args.url}`);
                //console.log(data);
                return data.msg == 'FlareSolverr is ready!';
        }
}
module.exports = Flaresolverr;
