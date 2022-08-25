var contextMenus = {};
const deleteIcon = `<span class="icon fa-solid fa-trash" style="padding-right:0.5rem"></span>`;
const infoIcon = `<span class="icon fa-solid fa-circle-info" style="padding-right:0.5rem"></span>`;
const addIcon = `<span class="icon fa-solid fa-plus" style="padding-right:0.5rem"></span>`;
const editIcon = `<span class="icon fa-solid fa-pen-to-square" style="padding-right:0.5rem"></span>`;
const dashboardIcon = `<span class="icon fa-solid fa-house" style="padding-right:0.5rem"></span>`;
const terminalIcon = `<span class="icon fa-solid fa-terminal" style="padding-right:0.5rem"></span>`;

function openDefaultContextMenu(event) {
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
            
            let menu = new ContextMenu({
                menuItems
            });
              
            menu.init();
            contextMenus['DEFAULT'] = menu;
        }
    
        contextMenus['DEFAULT'].open(event);
}

function openContextMenu(event, app){
    event?.preventDefault();
    event?.stopPropagation();
    if(typeof(app) === 'string')
        app = JSON.parse(app);

        
    let uid = app.Uid;
    let ele = document.getElementById(uid);
    let groupUid = ele.closest('.db-group').getAttribute('id');
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

        menuItems = menuItems.concat([
        {
          content: `${deleteIcon}Delete`
        },
        {
            content: `${editIcon}Edit Group`,
            divider: "top", // top, bottom, top-bottom
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
        }
        ]);
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
                divider: "top",
                content: `${terminalIcon}Terminal`,
                events: {
                    click: (e) => openTerminal(2, uid)
                }
            });
        }
        
        let menu = new ContextMenu({
            menuItems
        });
          
        menu.init();
        contextMenus[uid] = menu;
    }

    contextMenus[uid].open(event);
}
