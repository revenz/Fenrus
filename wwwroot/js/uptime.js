function closeUpTime(){
    document.getElementById('up-time-wrapper').style.display = '';
}

function openUpTime(app) {
    new UpTime(app);
}

class UpTime 
{
    app;
    ctx;

    constructor(app){
        console.log('uptime scontsrf');
        this.app = app;
        this.init();
    }

    init() {        
        let upTime = document.getElementById('up-time-wrapper');
        document.getElementById('up-time-title').innerText = 'Up-Time For ' + this.app.Name;
        let utcContainer = document.getElementById('up-time-chart-container');
        utcContainer.className = '';
        utcContainer.innerHTML = '<div id="up-time-chart"></div>';
        this.ctx = document.getElementById('up-time-chart');
        upTime.style.display = 'unset';
        this.getData();
    }

    getData(){
            
        fetch('/settings/up-time/' + this.app.Uid)
        .then((response) => response.json())
        .then((data) => {
            this.renderChart(data);
            this.renderTable(data);
        });
    }
 
    renderChart(data){        
        if(!data?.length)
        {
            utcContainer.className = 'no-data';
            utcContainer.innerText = 'No up-time data available';
            return;
        }
        data = data.map(x => ({
                x: x.date, //new Date(x.date),
                y: x.up === true ? 1 : 0
        }));
        var options = {
            chart: {
                height: 400,
                width:'100%',
                type: "line",
                stacked: false,
                toolbar: {
                    show: false
                },
                zoom: {
                    enabled: false
                },
                animations: {
                    enabled: false
                }
            },
            series: [
            {
                data: data
            }],
            stroke: {
                curve: 'stepline',
            },
            tooltip: {
                theme: 'dark',
                x: {
                    show: false
                },
                y: {
                    title: {
                        formatter: function formatter(val, o) {
                            let item = data[o.dataPointIndex];
                            let dt = new Date(item.x);
                            return dt.toLocaleTimeString();
                        }
                    },
                    formatter: function(){
                        return '';
                    }
                }
            },
            xaxis: {
                type: 'datetime',
                labels: {
                    show: true,
                    style: {
                        colors: 'var(--color)'
                    },
                }
            },
            yaxis: {
                show: false
            }  
        };

        var chart = new ApexCharts(this.ctx, options);
        chart.render();
    }

    renderTable(data){
        if(!data?.length)
            return;
        let table = document.createElement('table');
        table.className = 'table up-time-table';
        let head = document.createElement('thead');
        table.appendChild(head);
        let headRow = document.createElement('tr');
        head.appendChild(headRow);
        for(let col of ['Date', 'Up', 'Message']){
            let th = document.createElement('th');
            th.className = col.toLowerCase();
            th.innerText = col;
            headRow.appendChild(th);
        }
        let tbody = document.createElement('tbody');
        table.appendChild(tbody);
        let count = 0;
        for(let d of data){
            let tr = document.createElement('tr');
            tbody.appendChild(tr);
            for(let col of ['Date', 'Up', 'Message']){
                let td = document.createElement('td');             
                td.className = col.toLowerCase();   
                tr.appendChild(td);
                let value = d[col.toLowerCase()];
                if(col === 'Up')
                {
                    td.innerHTML = `<span class="icon icon-${value ? 'check' : 'times'} ${value ? 'up' : 'down'}"></span>`;
                }
                else
                {
                    if(col === 'Date')
                        value = new Date(value).toLocaleTimeString();                    
                    if(!value && col === 'Message' && d.up)
                        value = 'Success';
                    td.innerText = value;
                }
            }
            if(++count >= 10)
                break;
        }

        let utcContainer = document.getElementById('up-time-chart-container');
        let wrapper = document.createElement('div');
        wrapper.appendChild(table);
        wrapper.className = 'up-time-table-wrapper';
        utcContainer.appendChild(wrapper);
    }
}
