<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="RulesEditor.ascx.cs"
    Inherits="Mediachase.Commerce.Manager.Apps.Marketing.Modules.RulesEditor" %>
<table>
    <tr>
        <td valign="top">
            <asp:Button runat="server" ID="NewConditionButton" Text="<%$ Resources:MarketingStrings, New_Condition %>" OnClientClick="<%#GetJSCommandDialog() %>" />
        </td>   
    </tr>
    <tr><td>&nbsp;</td></tr>
    <tr>
        <td valign="top">
            <asp:UpdatePanel runat="server" ID="ExpressionsPanel" ChildrenAsTriggers="false" UpdateMode="Conditional">
                <ContentTemplate>
                    <asp:Repeater runat="server" ID="RulesList" DataMember="Expression" OnItemDataBound="RulesList_ItemDataBound" OnItemCommand="RulesList_ItemCommand">
                        <ItemTemplate>                            
                            <asp:ImageButton runat="server" ID="EditControl" AlternateText="Modify Condition" CommandArgument='<%#Eval("ExpressionId") %>' ImageUrl="~/Apps/Shell/styles/Images/edit.gif" />
                            <asp:ImageButton runat="server" ID="ExprDeleteButton" AlternateText="Remove Condition" OnClientClick="isSubmit = true;" CommandName="DeleteExpression" CommandArgument='<%#Eval("ExpressionId") %>' ImageUrl="../images/delete.png" />&nbsp;
                            <asp:LinkButton runat="server" ID="LinkControl" Text='<%#Eval("Name") %>'></asp:LinkButton>
                            <br />
                        </ItemTemplate>
                    </asp:Repeater>
                </ContentTemplate>
            </asp:UpdatePanel>            
        </td>
    </tr>
</table>
