function closeUpdateHistory(){
    document.getElementById('up-time-wrapper').style.display = '';
}

function openUpdateHistory(app) {
    new UpdateHistory(app);
}

class UpdateHistory
{
    app;
    utcContainer;

    constructor(app){
        this.app = app;
        this.init();
    }

    init() {
        let upTime = document.getElementById('up-time-wrapper');
        document.getElementById('up-time-title').innerText = 'Update History For ' + this.app.Name;
        this.utcContainer = document.getElementById('up-time-chart-container');
        this.utcContainer.className = '';
        this.utcContainer.innerHTML = '';
        upTime.style.display = 'unset';
        this.getData();
    }

    getData(){            
        fetch(`/apps/${this.app.Uid}/history/list`)
        .then((response) => response.json())
        .then((data) => {
            this.renderTable(data);
        });
    }
 

    renderTable(data){
        if(!data?.length)
            return;
        let tableContainer = this.getSection('Data', 'update-history-data', true);
        let table = document.createElement('table');
        table.className = 'table update-history-table';
        let tbody = document.createElement('tbody');
        table.appendChild(tbody);
        let count = 0;
        for(let d of data){
            let tr = document.createElement('tr');
            tbody.appendChild(tr);
            for(let col of ['Date', 'Success']){
                let td = document.createElement('td');             
                td.className = col.toLowerCase();   
                tr.appendChild(td);
                let value = d[col.toLowerCase()];
                if(col === 'Success')
                {
                    td.innerHTML = `<span class="icon fas fa-${value ? 'check' : 'times'} ${value ? 'up' : 'down'}"></span>`;
                }
                else
                {
                    const dt = new Date(value);
                    td.innerText = dt.toLocaleString('default', { day: 'numeric', month: 'short' }) 
                            + " " + dt.toLocaleTimeString();
                }
            }
            tr.addEventListener('click', () => {
                for(let selected of tbody.querySelectorAll('.selected'))
                    selected.classList.remove('selected');
                tr.classList.add('selected');
                this.getItemData(d);
            })
        }
        
        tableContainer.innerHTML = '<div class="table-header"><span class="Date">Date</span><span class="Success">Success</span></div>';        
        let wrapper = document.createElement('div');
        wrapper.appendChild(table);
        tableContainer.appendChild(wrapper);
    }
    
    getItemData(item){
        fetch(`/apps/${this.app.Uid}/history/${item.date}`)
            .then((response) => response.json())
            .then((data) => {
                this.renderItem(data);
            });
    }
    
    renderItem(item)   
    {
        let eleLog = this.getSection('Log', 'update-history-log', true);
        eleLog.innerText = item.log;

        let eleResponse = this.getSection('Response', 'update-history-response', true);
        eleResponse.innerText = item.response;
    }
    
    getSection(label, id, expanded) 
    {
        let section = document.getElementById(id);
        if(section)
            return section;
        
        let wrapper = document.createElement('div');
        wrapper.setAttribute('id', id + '-wrapper');
        wrapper.className = 'css-collapsible ' + (expanded ? 'expanded' : 'collapsed');
        wrapper.innerHTML =
            `<label class="lbl-toggle"></label>` +
            '<div class="collapsible-content">' +
            `<div class="content-inner" id="${id}"></div>` +
            '</div>';
        let lbl = wrapper.querySelector('label');
        lbl.innerText = label;
        lbl.addEventListener('click', () => {
            expanded = !expanded;
            wrapper.className = 'css-collapsible ' + (expanded ? 'expanded' : 'collapsed');
        });
        this.utcContainer.appendChild(wrapper);
        return document.getElementById(id);
    }

}
