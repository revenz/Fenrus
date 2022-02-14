"use strict";

module.exports = {
  status: function status(args) {
    return new Promise(function (resolve, reject) {
      args.fetch('api/status').then(function (data) {
        console.log('ff data:', data);

        if (!data || isNaN(data.queue)) {
          resolve();
          return;
        }

        var secondlbl = 'Time';
        var secondValue = data.time;

        if (!data.time) {
          if (!data.processing) {
            secondlbl = 'Processed';
            secondValue = data.processed;
          } else {
            secondlbl = 'Processing';
            secondValue = data.processing;
          }
        }

        resolve(args.liveStats([['Queue', data.queue], [secondlbl, secondValue]]));
      });
    });
  },
  test: function test(args) {
    return new Promise(function (resolve, reject) {
      args.fetch(args.url + '/api/status').then(function (data) {
        if (data.processed === 0 || data.processed) resolve();else reject();
      })["catch"](function (error) {
        reject(error);
      });
    });
  }
};