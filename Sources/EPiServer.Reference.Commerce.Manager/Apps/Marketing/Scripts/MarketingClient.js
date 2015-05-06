// JScript File
function Mediachase_MarketingClient()
{
    // Properties
    
    // Method Mappings   
    this.NewPromotion = function(source)
    {
        CSManagementClient.ChangeView('Marketing', 'Promotion-Edit');
    };
    
    this.NewSegment = function(source)
    {
        CSManagementClient.ChangeView('Marketing', 'Segment-Edit');
    };
    
    this.NewCampaign = function(source)
    {
        CSManagementClient.ChangeView('Marketing', 'Campaign-Edit');
    };
    
    this.NewPolicy = function(source)
    {
        var group = CSManagementClient.QueryString("group");
        CSManagementClient.ChangeView('Marketing', 'Policy-Edit', 'group='+group);
    };
    
    this.NewExpression = function(source)
    {
        var group = CSManagementClient.QueryString("group");
        CSManagementClient.ChangeView('Marketing', 'Expression-Edit', 'group='+group);
    };
};

var CSMarketingClient = new Mediachase_MarketingClient();

