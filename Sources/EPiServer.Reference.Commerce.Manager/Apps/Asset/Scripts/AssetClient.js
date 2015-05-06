// JScript File
function Mediachase_AssetClient()
{
    // Properties
   
    this.AssetSaveRedirect = function()
    {
        var folderid = CSManagementClient.QueryString("id");
        CSManagementClient.CloseTab();
        CSManagementClient.ChangeView('Asset', 'Asset-List', 'id='+folderid);
    };    

    // Added to allow file items to be deleted after clicking on the edit icon on the right side
    this.ViewItem = function(type, id)
    {
        type = type.trim().toLowerCase();
        var folderid = CSManagementClient.QueryString("id");
       
        if(type=='node')
            CSManagementClient.ChangeView("Asset","FileItem-Edit", 'objectid='+id+'&id='+folderid);
        else
            this.OpenItem(type, id);
    };
    
    this.OpenItem2 = function(params)
    {
        var id = '';
        var type = '';
        try
        {
            var cmdObj = Sys.Serialization.JavaScriptSerializer.deserialize(params);
            id = cmdObj.CommandArguments.ID;
            type = cmdObj.CommandArguments.Type;
            type = type.trim().toLowerCase();
            var parentid = CSManagementClient.QueryString("id");
        }
        catch(e)
        {
            alert('A problem occured with retrieving parameters for function OpenItem');
            return;
        }
        
        if(type=='folder' || type=='levelup')
        {
            // The folder edit button was clicked
            CSManagementClient.ChangeView("Asset","FolderItem-Edit", 'Id='+parentid+'&folderid='+id);
        }
        else
            this.OpenItem(type, id);
    };
        
    this.OpenItem = function(type, id)
    {
        type = type.trim().toLowerCase();
        var folderid = CSManagementClient.QueryString("id");

        // If this is a folder, the name of the folder was clicked and the sub-files and folders
        // need to be displayed. For files, the file edit screen needs to be seen       
        if(type=='folder')
            CSManagementClient.ChangeView("Asset", "Asset-List",'id='+id);
        else if(type=='node')
            CSManagementClient.ChangeView("Asset","FileItem-Edit", 'objectid='+id+'&id='+folderid);
    };
    
    this.NewFile = function(source)
    {
        var folderid = CSManagementClient.QueryString("id");
        CSManagementClient.ChangeView('Asset', 'FileItem-Edit', 'id='+folderid);
    };  
};

var CSAssetClient = new Mediachase_AssetClient();

