<%@ Control Language="C#" AutoEventWireup="true" Inherits="Mediachase.Commerce.Manager.Folders.Tabs.PageEditTab" Codebehind="PageEditTab.ascx.cs" %>
<%@ Register Src="~/Apps/Core/Controls/HtmlEditControl.ascx" TagName="HtmlEditControl" TagPrefix="ecf" %>
<%@ Register Src="~/Apps/Core/Controls/BooleanEditControl.ascx" TagName="BooleanEditControl" TagPrefix="ecf" %>
<div id="DataForm">
    <table class="DataForm" width="650"> 
        <tr>
            <td class="FormLabelCell"><asp:Literal ID="NameLiteral" runat="server" Text='<%$ Resources:SharedStrings, Name %>'></asp:Literal>:</td>
            <td class="FormFieldCell">
                <asp:TextBox runat="server" Width="250" ID="Name"></asp:TextBox><br />
                <asp:Label ID="NameDescriptionLabel" CssClass="FormFieldDescription" runat="server"
                    Text="<%$ Resources:ContentStrings, Page_Name_Instruction %>"></asp:Label>
                <asp:RequiredFieldValidator runat="server" ID="valName" ControlToValidate="Name" 
                    ErrorMessage="<%$ Resources:SharedStrings, Name_Required %>" Display="Dynamic"></asp:RequiredFieldValidator>
                <asp:CustomValidator runat="server" ID="NameCheckCustomValidator" ControlToValidate="Name" OnServerValidate="NameCheck" Display="Dynamic" ErrorMessage="<%$ Resources:ContentStrings, Folder_With_Name_Exists %>" />
            </td>
        </tr>
        <tr>
            <td colspan="2" class="FormSpacerCell">
            </td>
        </tr>
        <tr id="ParentFolderRow" runat="server" visible="false">
            <td class="FormLabelCell" nowrap><asp:Literal ID="ParentFolderLiteral" runat="server" Text='<%$ Resources:ContentStrings, Folder_ParentFolder %>'></asp:Literal>:</td>
            <td class="FormFieldCell"><asp:DropDownList runat="server" ID="Root"></asp:DropDownList></td>
        </tr>
        <tr id="MasterPageRow" runat="server" visible="false">
            <td class="FormLabelCell"><asp:Literal ID="MasterPageLiteral" runat="server" Text='<%$ Resources:ContentStrings, Page_MasterPage %>'></asp:Literal>:</td>
            <td class="FormFieldCell"><asp:TextBox runat="server" ID="MasterPageText" Width="220"></asp:TextBox></td>
        </tr>
        <tr id="DefaultRow" runat="server">
            <td class="FormLabelCell" nowrap><asp:Literal ID="IsDefaultLiteral" runat="server" Text='<%$ Resources:ContentStrings, Page_DefaultPage %>'></asp:Literal>:</td>
            <td class="FormFieldCell"><ecf:BooleanEditControl id="IsDefault" runat="server"></ecf:BooleanEditControl>
            <asp:Label ID="IsDefaultDescriptionLabel" CssClass="FormFieldDescription" runat="server" 
                Text="<%$ Resources:ContentStrings, Page_Default_Description %>"></asp:Label>            
            </td>
        </tr>
        <!-- BEGIN: MetaAttributes -->
        <tr>
            <td colspan="2" class="FormSpacerCell">
            </td>
        </tr>        
        <tr>
            <td class="FormLabelCell"><asp:Literal ID="TitleLiteral" runat="server" Text='<%$ Resources:SharedStrings, Title %>'></asp:Literal>:</td>
            <td class="FormFieldCell"><asp:TextBox runat="server" ID="txtTitle" TextMode="MultiLine" Rows="2" Columns="100"></asp:TextBox><br />
            <asp:Label ID="TitleDescriptionLabel" CssClass="FormFieldDescription" runat="server"
                Text="<%$ Resources:ContentStrings, Page_Title_Instruction %>"></asp:Label>
            </td>
        </tr>
        <tr>
            <td colspan="2" class="FormSpacerCell">
            </td>
        </tr>        
        <tr>
            <td class="FormLabelCell"><asp:Literal ID="KeywordsLiteral" runat="server" Text='<%$ Resources:SharedStrings, Keywords %>'></asp:Literal>: <br />
                <asp:Literal ID="Literal4" runat="server" Text="<%$ Resources:ContentStrings, Meta_Keywords_Parenthesis %>"/>
            </td>
            <td class="FormFieldCell"><asp:TextBox runat="server" ID="txtKeywords" TextMode="MultiLine" Rows="6" Columns="100"></asp:TextBox><br />
            <asp:Label ID="KeywordsDescriptionLabel" CssClass="FormFieldDescription" runat="server"
                Text="<%$ Resources:ContentStrings, Page_Keywords_Instruction %>"></asp:Label>
            </td>
        </tr>
        <tr>
            <td colspan="2" class="FormSpacerCell">
            </td>
        </tr>               
        <tr>
            <td class="FormLabelCell"><asp:Literal ID="MetaDescriptionLiteral" runat="server" Text='<%$ Resources:SharedStrings, Description %>'></asp:Literal>: <br />
                <asp:Literal ID="Literal6" runat="server" Text="<%$ Resources:ContentStrings, Meta_Description_Parenthesis %>"/>
            </td>
            <td class="FormFieldCell"><asp:TextBox runat="server" ID="txtDescription" TextMode="MultiLine" Rows="6" Columns="100"></asp:TextBox><br />
            <asp:Label ID="Label10" CssClass="FormFieldDescription" runat="server"
                Text="<%$ Resources:ContentStrings, Meta_Description_Instructions %>"></asp:Label>
            </td>
        </tr>
        <!-- END: MetaAttributes -->
    </table>
</div>