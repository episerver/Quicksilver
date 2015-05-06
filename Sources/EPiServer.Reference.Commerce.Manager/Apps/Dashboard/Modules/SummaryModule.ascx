<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="SummaryModule.ascx.cs"
    Inherits="Mediachase.Commerce.Manager.Apps.Dashboard.Modules.SummaryModule" %>
<div class="db-panel-outer">
    <div class="db-panel">
        <div class="summary-row">
            <div class="summary-item">
                <div class="body">
                    <div class="stat">
                        <div class="image">
                            <asp:Image ID="Image1" runat="server" ImageUrl="~/Apps/Shell/styles/Images/Icons/Bundle.gif" /></div>
                        <div class="name">
                            <asp:Literal ID="Literal1" runat="server" Text="<%$ Resources:SharedStrings, Products %>"/>:</div>
                        <div class="value" runat="server" id="EntriesCount">
                            0</div>
                    </div>
                </div>
            </div>
            <div class="summary-item">
                <div class="body">
                    <div class="stat">
                        <div class="image">
                            <asp:Image runat="server" ImageUrl="~/Apps/Shell/styles/Images/Icons/node.gif" /></div>
                        <div class="name">
                            <asp:Literal ID="Literal2" runat="server" Text="<%$ Resources:SharedStrings, Categories %>"/>:</div>
                        <div class="value" runat="server" id="NodesCount">
                            0</div>
                    </div>
                </div>
            </div>
            <div class="summary-item">
                <div class="body">
                    <div class="stat">
                        <div class="image">
                            <asp:Image ID="Image2" runat="server" ImageUrl="~/Apps/Dashboard/Images/orders.png" /></div>
                        <div class="name">
                            <asp:Literal ID="Literal3" runat="server" Text="<%$ Resources:SharedStrings, Orders %>"/>:</div>
                        <div class="value" runat="server" id="OrdersCount">
                            0</div>
                    </div>
                </div>
            </div>
            <div class="summary-item">
                <div class="body">
                    <div class="stat">
                        <div class="image">
                            <asp:Image ID="Image3" runat="server" ImageUrl="~/Apps/Dashboard/Images/customers.png" /></div>
                        <div class="name">
                            <asp:Literal ID="Literal4" runat="server" Text="<%$ Resources:SharedStrings, Customers %>"/>:</div>
                        <div class="value" runat="server" id="CustomerCount">
                            0</div>
                    </div>
                </div>
            </div>
            <div class="summary-item">
                <div class="body">
                    <div class="stat">
                        <div class="image">
                            <asp:Image ID="Image4" runat="server" ImageUrl="~/Apps/Dashboard/Images/promotions.png" /></div>
                        <div class="name">
                            <asp:Literal ID="Literal5" runat="server" Text="<%$ Resources:SharedStrings, Promotions %>"/>:</div>
                        <div class="value" runat="server" id="PromotionCount">
                            0</div>
                    </div>
                </div>
            </div>                
            
        </div>
    </div>
</div>