class LeagueOfLegends {
    doLeagueFetch(args, endpoint) {
        return args.fetch({
            url: 'https://' + args.properties['region'] + '.api.riotgames.com/lol/' + endpoint + '?api_key' + args.properties['apiKey'],
            timeout: 1500,
            headers: {
                'X-Riot-Token': args.properties['apiKey']
            }
        });
    }

    //TODO: cache files based on current ddragon version
    doDataFetch(args, fullUrl) {
        return args.fetch({
            url: fullUrl,
            timeout: 2500
        });
    }



    status(args) {
        let versionJson = this.doDataFetch(args, "https://ddragon.leagueoflegends.com/api/versions.json")
        if (versionJson == null || versionJson[0] == null) {
            return args.liveStats([
                ["Result", "Version error"]
            ]);
        }
        let versionNumber = versionJson[0]
        let summonerName = args.properties['summonerName']

        let summonerData = this.doLeagueFetch(args, "summoner/v4/summoners/by-name/" + summonerName)
        let summonerId = summonerData?.id
        let profileIconId = summonerData?.profileIconId

        if (profileIconId != null || profileIconId > 0) {
            args.changeIcon("https://ddragon.leagueoflegends.com/cdn/" + versionNumber + "/img/profileicon/" + profileIconId + ".png")
        }
        let ongoingGameData = this.doLeagueFetch(args, "spectator/v4/active-games/by-summoner/" + summonerId)
        if (ongoingGameData == null || ongoingGameData?.status?.status_code == 404) {
            return args.liveStats([
                [summonerName, "Not in Game"]
            ]);
        } else if (ongoingGameData?.status?.status_code == 403) {
            return args.liveStats([
                ["Result", "Authentication failure"]
            ]);
        }

        let gameType = ongoingGameData?.gameType
        let mapId = ongoingGameData?.mapId
        let gameMode = ongoingGameData?.gameMode
        let championId = -1
        let players = ongoingGameData?.participants
        let gameLength = ongoingGameData?.gameLength
        for (var playerIndex in players) {
            let playerJson = players[playerIndex]
            console.log(playerJson, playerJson?.summonerId, summonerId)
            if (playerJson?.summonerId == summonerId) {
                championId = playerJson?.championId
                break;
            }
        }
        let championName = "Unknown Champion"
        let mapName = "Unknown Map"
        let championJson = this.doDataFetch(args, "https://ddragon.leagueoflegends.com/cdn/" + versionNumber + "/data/en_US/champion.json")
        for (var champion in championJson.data) {
            let championObj = championJson.data[champion]
            if (championObj?.key == championId) {
                championName = championObj?.name
                break
            }

        }

        let mapJson = this.doDataFetch(args, "https://static.developer.riotgames.com/docs/lol/maps.json")
        for (var map in mapJson) {
            let mapObj = mapJson[map]
            if (mapObj?.mapId == mapId) {
                mapName = mapObj?.mapName
                break
            }

        }

        //change icon to champion logo rather than summoner
        //args.changeIcon("https://ddragon.leagueoflegends.com/cdn/" + versionNumber + "/img/champion/" + championName + ".png")
        let data = [];
        data.push(["Summoner", summonerName])
        if (gameMode != "CLASSIC") {
            data.push(["Mode", gameMode])
        } else {
            data.push(["Map", mapName])
        }
        if (gameLength != null && gameLength > 0) {
            data.push(["Runtime", args.Utils.formatMilliTimeToWords(gameLength * 1000, false)])
        }
        data.push(["Champion", championName])

        return args.liveStats(data);

    }

    test(args) {
        let summonerName = args.properties['summonerName']
        let summonerData = this.doLeagueFetch(args, "summoner/v4/summoners/by-name/" + summonerName)
        console.log("summonerData", summonerData)
        let summonerId = summonerData?.id
        return summonerId != null;
    }
}
