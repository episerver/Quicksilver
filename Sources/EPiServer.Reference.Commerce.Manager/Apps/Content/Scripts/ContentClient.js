// JScript File
function Mediachase_ContentClient()
{
    // Properties    
   
    // Method Mappings
    this.NewWorkflow = function(source) {
        CSManagementClient.ChangeView('Content', 'Workflow-Edit', '');
    };

    this.EditWorkflow = function(params) {
        var wfId = '';
        try {
            var cmdObj = Sys.Serialization.JavaScriptSerializer.deserialize(params);
            wfId = cmdObj.CommandArguments.WorkflowId;
        }
        catch (e) {
            alert('A problem occured with retrieving parameters for function EditWorkflow');
            return;
        }
        CSManagementClient.ChangeView('Content', 'Workflow-Edit', 'WorkflowId=' + wfId);
    };

    this.EditWorkflowStates = function(params) {
        var wfId = '';
        try {
            var cmdObj = Sys.Serialization.JavaScriptSerializer.deserialize(params);
            wfId = cmdObj.CommandArguments.WorkflowId;
        }
        catch (e) {
            alert('A problem occured with retrieving parameters for function EditWorkflowStates');
            return;
        }
        CSManagementClient.ChangeView('Content', 'State-List', 'WorkflowId=' + wfId);
    };

    this.NewWorkflowState = function(source) {
        var workflowid = CSManagementClient.QueryString("WorkflowId");
        CSManagementClient.ChangeView('Content', 'State-Edit', 'WorkflowId=' + workflowid);
    };

    this.EditWorkflowState = function(params) {
        var id = '';
        var wfId = '';
        try {
            var cmdObj = Sys.Serialization.JavaScriptSerializer.deserialize(params);
            id = cmdObj.CommandArguments.StatusId;
            wfId = cmdObj.CommandArguments.WorkflowId;
        }
        catch (e) {
            alert('A problem occured with retrieving parameters for function EditWorkflowState');
            return;
        }
        CSManagementClient.ChangeView('Content', 'State-Edit', 'StatusId=' + id + '&WorkflowId=' + wfId);
    };
    
    // Folder & Pages Methods
    this.EditPage = function(params) {
        var folderid = CSManagementClient.QueryString("FolderId");

        var id = '';
        var siteid = '';
        var isFolder = false;
        try {
            var cmdObj = Sys.Serialization.JavaScriptSerializer.deserialize(params);
            id = cmdObj.CommandArguments.PageId;
            siteid = cmdObj.CommandArguments.SiteId;
            isFolder = cmdObj.CommandArguments.IsFolder;
        }
        catch (e) {
            alert('A problem occured with retrieving parameters for function EditPage');
            return;
        }

        if (isFolder.toLowerCase() == 'true')
            CSManagementClient.ChangeView('Content', 'Folder-Edit', 'PageId=' + id + '&FolderId=' + folderid + '&siteid=' + siteid);
        else
            CSManagementClient.ChangeView('Content', 'Page-Edit', 'PageId=' + id + '&FolderId=' + folderid + '&siteid=' + siteid);          
    };

    this.NewPageCmd = function() {
        var pageId = CSManagementClient.QueryString("PageId");
        var siteId = CSManagementClient.QueryString("SiteId");
        var folderId = CSManagementClient.QueryString("folderid");
        CSManagementClient.ChangeView('Content', 'PageCmd-Edit', 'PageId=' + pageId + '&FolderId=' + folderId + '&SiteId=' + siteId);
    };

    this.EditPageCmd = function(params) {
        var siteId = CSManagementClient.QueryString("SiteId");
        var folderId = CSManagementClient.QueryString("folderid");

        var id = '';
        var pageId = '';
        try {
            var cmdObj = Sys.Serialization.JavaScriptSerializer.deserialize(params);
            id = cmdObj.CommandArguments.Id;
            pageId = cmdObj.CommandArguments.UrlUID;
        }
        catch (e) {
            alert('A problem occured with retrieving parameters for function EditPageCmd');
            return;
        }
        CSManagementClient.ChangeView('Content', 'PageCmd-Edit', 'commandid=' + id + '&pageid=' + pageId + '&FolderId=' + folderId + '&SiteId=' + siteId);
    };

    this.PageCmdSaveRedirect = function() {
        var folderid = CSManagementClient.QueryString("FolderId");
        var siteid = CSManagementClient.QueryString("SiteId");
        var pageId = CSManagementClient.QueryString("PageId");
        CSManagementClient.ChangeView('Content', 'PageCmd-List', 'PageId=' + pageId + '&FolderId=' + folderid + '&siteid=' + siteid);
    };

    this.ViewPageCmds = function(params) {
        var items = Toolbar_GetSelectedGridItems(params);

        if (items) {
            var splitter = ';';

            // remove trailing splitter
            if ((items.length > 1) && (items.lastIndexOf(splitter) == items.length - 1))
                items = items.substring(0, items.length - 1);

            // get array of selected items
            var selectedItems = items.split(splitter);
            if (selectedItems.length == 0) {
                alert("You must select an item.");
                return;
            }
            else if (selectedItems.length > 1) {
                alert('You must select only one item!');
                return;
            }
            else if (selectedItems.length == 1) {
                var keys = selectedItems[0].split(CSManagementClient.EcfListView_PrimaryKeySeparator);
                if (keys && (keys.length >= 3)) {
                    // check if selected element is not a folder
                    var isfolder = keys[2];
                    if (isfolder.toString().toLowerCase() == 'true') {
                        alert('This action can only be performed on page.');
                        return;
                    }

                    // change view
                    var folderId = CSManagementClient.QueryString("FolderId");
                    var id = keys[0]; // pageId
                    var siteId = keys[1]; // siteId

                    CSManagementClient.ChangeView('Content', 'PageCmd-List', 'PageId=' + id + '&FolderId=' + folderId + '&SiteId=' + siteId);
                }
                else
                    alert('Invalid primaryKey!');
                return;
            }
        }
    };

    this.CreateFolder = function(source) {
        var siteid = CSManagementClient.QueryString("SiteId");
        var folderid = CSManagementClient.QueryString("folderid");
        CSManagementClient.ChangeView('Content', 'Folder-Edit', 'FolderId=' + folderid + '&siteid=' + siteid);
    };

    this.CreatePage = function(source) {
        var siteid = CSManagementClient.QueryString("SiteId");
        var folderid = CSManagementClient.QueryString("folderid");
        CSManagementClient.ChangeView('Content', 'Page-Edit', 'FolderId=' + folderid + '&siteid=' + siteid);
    };

    this.FolderSaveRedirect = function() {
        var folderid = CSManagementClient.QueryString("folderid");
        var siteid = CSManagementClient.QueryString("SiteId");
        CSManagementClient.ChangeView('Content', 'Folder-List', 'FolderId=' + folderid + '&siteid=' + siteid);
    };
    
    // Menu Methods
    this.MenuSaveRedirect = function() {
        var folderid = CSManagementClient.QueryString("folderid");
        var siteid = CSManagementClient.QueryString("SiteId");
        CSManagementClient.ChangeView('Content', 'Menu-List', 'FolderId=' + folderid + '&siteid=' + siteid);
    };

    this.MenuItemSaveRedirect = function() {
        var menuitemid = CSManagementClient.QueryString("parentid");
        var siteid = CSManagementClient.QueryString("SiteId");
        var langid = CSManagementClient.QueryString("LangId");
        CSManagementClient.ChangeView('Content', 'MenuItem-List', 'menuitemid=' + menuitemid + '&parentid=' + menuitemid + '&siteid=' + siteid + '&langid=' + langid);
    };

    this.EditMenuItem = function(menuItemId, isdirectory) {
        var siteid = CSManagementClient.QueryString("SiteId");
        var langid = CSManagementClient.QueryString("langid");
        var parentItemId = CSManagementClient.QueryString("parentid");
        var id = menuItemId;
        if (isdirectory == 'True')
            CSManagementClient.ChangeView('Content', 'MenuItem-List', 'menuitemid=' + id + '&parentid=' + parentItemId + '&siteid=' + siteid + '&langid=' + langid);
        else
            CSManagementClient.ChangeView('Content', 'MenuItem-Edit', 'menuitemid=' + id + '&parentid=' + parentItemId + '&siteid=' + siteid + '&langid=' + langid);
    };

    this.EditMenuItem2 = function(params) {
        var menuItemId = '';
        var isDirectory = '';
        try {
            var cmdObj = Sys.Serialization.JavaScriptSerializer.deserialize(params);
            menuItemId = cmdObj.CommandArguments.MenuItemId;
            isDirectory = cmdObj.CommandArguments.IsDirectory;
        }
        catch (e) {
            alert('A problem occured with retrieving parameters for function EditMenuItem2');
            return;
        }
        this.EditMenuItem(menuItemId, isDirectory);
    };

    this.CreateMenu = function(source) {
        var siteid = CSManagementClient.QueryString("SiteId");
        CSManagementClient.ChangeView('Content', 'Menu-Edit', 'isnew=true&siteid=' + siteid);
    };

    this.CreateMenuItem = function(source) {
        var id = CSManagementClient.QueryString("parentid");
        var siteid = CSManagementClient.QueryString("SiteId");
        var langid = CSManagementClient.QueryString("langid");
        CSManagementClient.ChangeView('Content', 'MenuItem-Edit', 'menuitemid=' + id + '&parentid=' + id + '&isnew=true&siteid=' + siteid + '&langid=' + langid);
    };

    this.EditMenuCommand = function(i, source) {
        var CommandTitle = $get(source + "_CommandTitle");
        var CommandText = $get(source + "_CommandText");
        var NavigationTitle = $get(source + "_NavigationTitle");
        var NavigationText = $get(source + "_NavigationText");

        switch (i) {
            case 0:
                CommandTitle.style.display = "none";
                CommandText.style.display = "none";
                NavigationTitle.style.display = "none";
                NavigationText.style.display = "none";
                break;
            case 1:
                NavigationTitle.style.display = "none";
                NavigationText.style.display = "none";
            case 2:
                CommandTitle.style.display = "block";
                CommandText.style.display = "block";
                NavigationTitle.style.display = "none";
                NavigationText.style.display = "none";
                break;
            case 3:
                CommandTitle.style.display = "none";
                CommandText.style.display = "none";
                NavigationTitle.style.display = "block";
                NavigationText.style.display = "block";
                break;
        }
    };
    
    // Site functions
    this.NewSite = function(source) {
        CSManagementClient.ChangeView('Content', 'Site-Edit', '');
    };

    this.CreateTemplateItem = function(source) {
        var languageCode = CSManagementClient.QueryString("lang");
        CSManagementClient.ChangeView('Content', 'Template-Edit', 'lang=' + languageCode);
    };

    this.TemplateListRedirect = function(source) {
        var languageCode = CSManagementClient.QueryString("lang");
        CSManagementClient.ChangeView('Content', 'Templates-List', 'lang=' + languageCode);
    };
    
    // Import/Export Site
    this.ImportSite = function(source) {
        CSManagementClient.ChangeView('Content', 'Site-Import', '');
    };

    this.ExportSite = function(params) {
        if (params != null) {
            var items = Toolbar_GetSelectedGridItems(params);

            if (items) {
                var splitter = ';';

                // remove trailing splitter
                if ((items.length > 1) && (items.lastIndexOf(splitter) == items.length - 1))
                    items = items.substring(0, items.length - 1);

                // get array of selected items
                var selectedItems = items.split(splitter);
                if (selectedItems.length == 0) {
                    alert("You must select a site before you can perform export.");
                    return;
                }
                else if (selectedItems.length > 1) {
                    alert('You must select only one site!');
                    return;
                }
                else if (selectedItems.length == 1) {
                    var keys = selectedItems[0].split(CSManagementClient.EcfListView_PrimaryKeySeparator);
                    if (keys && (keys.length >= 2)) {
                        var siteId = keys[0];
                        var siteName = keys[1];

                        CSManagementClient.ChangeView('Content', 'Site-Export', 'siteid=' + siteId + '&sitename=' + encodeURI(siteName));
                    }
                    else
                        alert('Invalid primaryKey!');
                    return;
                }
            }
        }
    };
    
    // site grid events - copy and export site
    this.ExportSite2 = function(params) {
        var siteId = '';
        var siteName = '';
        try {
            var cmdObj = Sys.Serialization.JavaScriptSerializer.deserialize(params);
            siteId = cmdObj.CommandArguments.SiteId;
            siteName = cmdObj.CommandArguments.Name;
        }
        catch (e) {
            alert('A problem occured with retrieving parameters for function ExportSite2');
            return;
        }
        CSManagementClient.ChangeView('Content', 'Site-Export', 'siteid=' + siteId + '&sitename=' + encodeURI(siteName));
    };

    this.CopySite = function(params) {
        var siteId = '';
        try {
            var cmdObj = Sys.Serialization.JavaScriptSerializer.deserialize(params);
            siteId = cmdObj.CommandArguments.SiteId;
        }
        catch (e) {
            alert('A problem occured with retrieving parameters for function CopySite');
            return;
        }
        CSManagementClient.ChangeView('Content', 'Site-Edit', 'SiteId=' + siteId + '&cmd=copy');
    };
};

var CSContentClient = new Mediachase_ContentClient();