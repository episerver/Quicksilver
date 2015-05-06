    
Ext.onReady(function(){
    // Perform initialization
    // shorthand
    var Tree = Ext.tree;
    
    var tree = new Tree.TreePanel({
        id: 'dashboardtree',
        autoScroll:true,
        animate:true,
        rootVisible:false,
        enableDD:false,
        title: 'Dashboard',
        containerScroll: true, 
        loader: new Tree.TreeLoader({
            dataUrl:'Apps/Dashboard/Tree/TreeSource.aspx?app=dashboard'
        })
    });
    
    tree.on('click', function (node) {
        if (typeof node.attributes.viewid != 'undefined') 
            CSManagementClient.ChangeView(node.attributes.app, node.attributes.viewid, 'id='+node.attributes.id);
    });     
    
    tree.loader.on("beforeload", function(treeLoader, node) {
        treeLoader.baseParams.nodeid = node.attributes.nodeid;
    });    
        
    CSManagementClient.RegisterMenuView(tree);
    
    // set the root node
    var root = new Tree.AsyncTreeNode({
        text: 'Dashboard',
        draggable:false,
        expanded: true,
        id:'dashboard-root'
    });
    tree.setRootNode(root);    
});