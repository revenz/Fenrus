var contextMenus = {};
const deleteIcon = `<span class="icon fa-solid fa-trash" style="padding-right:0.5rem"></span>`;
const infoIcon = `<span class="icon fa-solid fa-circle-info" style="padding-right:0.5rem"></span>`;
const addIcon = `<span class="icon fa-solid fa-plus" style="padding-right:0.5rem"></span>`;
const editIcon = `<span class="icon fa-solid fa-pen-to-square" style="padding-right:0.5rem"></span>`;
const resizeIcon = `<span class="icon fa-solid fa-maximize" style="padding-right:0.5rem"></span>`;
const dashboardIcon = `<span class="icon fa-solid fa-house" style="padding-right:0.5rem"></span>`;
const terminalIcon = `<span class="icon fa-solid fa-terminal" style="padding-right:0.5rem"></span>`;
const logIcon = `<span class="icon fa-solid fa-file-lines" style="padding-right:0.5rem"></span>`;

function openDefaultContextMenu(event) {
    console.log('openDefaultContextMenu');
    event?.preventDefault();
    event?.stopPropagation();
    if(!contextMenus['DEFAULT'])
    {
        let dashboardUid = document.querySelector('.dashboard').getAttribute('x-uid');
        const menuItems = [
            {
                content: `${dashboardIcon}Edit Dashboard`,
                events: {
                    click: (e) => {
                        document.location = '/settings/dashboards/' + dashboardUid                
                    }
                }
            },
            {
                content: `${terminalIcon}Terminal`,
                events: {
                    click: (e) => openTerminal()
                }
            }

        ];
        
        console.log('menuItems before menu: ', JSON.parse(JSON.stringify(menuItems)));
        let menu = new ContextMenu({
            menuItems
        });
          
        menu.init();
        contextMenus['DEFAULT'] = menu;
    }
    
    console.log('contextMenus[\'DEFAULT\']', contextMenus['DEFAULT']);
    
    contextMenus['DEFAULT'].open(event);
}

function openContextMenu(event, app){
    event?.preventDefault();
    event?.stopPropagation();
    if(typeof(app) === 'string')
        app = JSON.parse(app);
        
    let uid = app.Uid;
    let ele = document.getElementById(uid);
    let group = ele.closest('.db-group');
    let groupUid = group.getAttribute('id');
    let dashboardUid = ele.closest('.dashboard').getAttribute('x-uid');
    let ssh = ele.getAttribute('x-ssh') === '1';
    let docker = ele.getAttribute('x-docker');
    if(!contextMenus[uid])
    {
        let menuItems = [];
        if(app._Type !== 'DashboardTerminal'){
            menuItems.push(
            {
                content: `${infoIcon}Up-Time`,
                events: {
                    click: (e) => openUpTime(app)                
                }
            });
        }
        if(ssh){
            menuItems.push(
            {
                divider: "top",
                content: `${terminalIcon}Terminal`,
                events: {
                    click: (e) => openTerminal(1, uid)
                }
            });
        }
        else if(docker){
            menuItems.push(
            {
                content: `${terminalIcon}Terminal`,
                events: {
                    click: (e) => openTerminal(2, uid)
                }
            });
            menuItems.push(
            {
                content: `${logIcon}Log`,
                events: {
                    click: (e) => openDockerLog(uid)
                }
            });
        }

        menuItems = menuItems.concat([
            {
                content: `${resizeIcon}Resize`,
                divider: "top",
                submenu: ['Small', 'Medium', 'Large', 'Larger', 'X-Large', 'XX-Large'].map((x) =>
                {
                    return { 
                        content: x,
                        events: {
                            click: (e) => {
                                for(let size of ['Small', 'Medium', 'Large', 'Larger', 'X-Large', 'XX-Large']){
                                    ele.classList.remove(size.toLowerCase());
                                }
                                ele.classList.add(x.toLowerCase());
                                document.dispatchEvent(new CustomEvent('fenrus-item-resized', {
                                    detail: { element: ele }
                                }));

                                fetch(`/settings/groups/${groupUid}/resize/${uid}/${x.toLowerCase().replace('-', '')}`, { method: 'POST'});
                            }
                        }
                    };
                })
            },
        {
            content: `${editIcon}Edit Group`,
            divider: "top",
            events: {
                click: (e) => {
                    document.location = '/settings/groups/' + groupUid                
                }
            }
        },
        {
            content: `${dashboardIcon}Edit Dashboard`,
            events: {
                click: (e) => {
                    document.location = '/settings/dashboards/' + dashboardUid                
                }
            }
        },
        {
            divider: "top",
            content: `${deleteIcon}Delete`,
            events: { 
                click: async (e) => {
                    if(await modalConfirm('Delete', `Delete ${app.Name}?`)) {
                        ele.remove();
                        let items = group.querySelectorAll('.items .db-item');
                        if(items.length === 0){
                            group.remove();
                        }
                        else
                        {
                            document.dispatchEvent(new CustomEvent('fenrus-item-deleted', {
                                detail: { group: group }
                            }));
                        }
                        fetch(`/settings/groups/${groupUid}/delete/${uid}`, { method: 'DELETE'});
                    }
                }
            }
        },
        ]);
        
        let menu = new ContextMenu({
            menuItems
        });
          
        menu.init();
        contextMenus[uid] = menu;
    }

    contextMenus[uid].open(event);
}
