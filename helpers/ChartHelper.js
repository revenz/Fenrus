const { ChartJSNodeCanvas } = require('chartjs-node-canvas');

class ChartHelper
{
    width = 400; //px
    height = 400; //px
    backgroundColour = 'rgba(0,0,0,0.3)'; // Uses https://www.w3schools.com/tags/canvas_fillstyle.asp
    chartJSNodeCanvas;

    constructor(widthPixels, heightPixels) {
        this.width = widthPixels || 400;
        this.height = heightPixels || 400;
        this.chartJSNodeCanvas = new ChartJSNodeCanvas({ width: this.width, height: this.height, backgroundColour: this.backgroundColour});
    }
    
    async line(args) {
        let title = args.title;
        let labels = args.labels;
        let datasets = args.data;
        let min = args.min;
        let max = args.max;
        var data = {
            labels: labels,
            datasets: datasets.map((x, index) => {
                return {
                    data: x,
                    fill: false,
                    borderColor: ['green', 'blue', 'yellow', 'red', 'purple', 'orange', 'cornflowerblue'][index],
                    lineTension: 0.1
                };
            })
          };
        
          //options
          var options = {
            responsive: true,
            plugins: { 
                title: {
                    display:true,
                    color:'white',
                    align:'end',
                    position:'top',
                    text: '   ' + title + '   ',
                },
                legend: {
                    display:false
                }
            },
            elements: {
                point: {
                    radius:0
                }
            },
            scales: {
                yAxes: {
                    grid: {
                      display: false,
                      drawBorder: false
                    },
                    min: min == -1 ? null : min || 0,
                    max: max == -1 ? null : max || 100,
                    display: false,
                    ticks: {
                        display:false
                    }
                },
                xAxes: {
                    grid: {
                      display: true
                    },
                    ticks: {
                        display:false
                    }
                }
            }
          };

          return await this.render({
            type: "line",
            data: data,
            options: options
          });
    }

    async render(configuration) {
        // See https://www.chartjs.org/docs/latest/configuration            
        configuration = configuration || {
            type: 'bar',
            data: {
              labels: ["Africa", "Asia", "Europe", "Latin America", "North America"],
              datasets: [
                {
                  label: "Population (millions)",
                  backgroundColor: ["#3e95cd", "#8e5ea2","#3cba9f","#e8c3b9","#c45850"],
                  data: [2478,5267,734,784,433]
                }
              ]
            },
            options: {
              legend: { display: false },
              title: {
                display: true,
                text: 'Predicted world population (millions) in 2050'
              }
            }
        };
        //const image = await chartJSNodeCanvas.renderToBuffer(configuration);
        //base64 image
        //const dataUrl = await chartJSNodeCanvas.renderToDataURL(configuration);
        //const stream = chartJSNodeCanvas.renderToStream(configuration);
        try{
            console.log(';about to render!');
            let result = await this.chartJSNodeCanvas.renderToDataURL(configuration);
            //let result = await this.chartJSNodeCanvas.renderToBuffer(configuration);//,'image/svg+xml')
        
        return result;
        }catch(err) {
            console.log('error: ' + err);
        }
    }
}

module.exports = ChartHelper;