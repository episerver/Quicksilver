<%@ Control Language="C#" AutoEventWireup="true" Inherits="Mediachase.Commerce.Manager.Site.Tabs.SiteEditTab" Codebehind="SiteEditTab.ascx.cs" %>
<%@ Register Assembly="Mediachase.WebConsoleLib" Namespace="Mediachase.Web.Console.Controls" TagPrefix="console" %>
<%@ Register Src="~/Apps/Core/Controls/HtmlEditControl.ascx" TagName="HtmlEditControl" TagPrefix="ecf" %>
<%@ Register Src="~/Apps/Core/Controls/BooleanEditControl.ascx" TagName="BooleanEditControl" TagPrefix="ecf" %>
<div id="DataForm">
    <table>
        <tr>
            <td class="FormLabelCell">
                *<asp:Label runat="server" Text="<%$ Resources:SharedStrings, Name %>"></asp:Label>:</td>
            <td class="FormFieldCell">
                <asp:TextBox runat="server" Width="250" MaxLength="50" ID="SiteName"></asp:TextBox>
                <asp:RequiredFieldValidator runat="server" ID="NameValidation" ControlToValidate="SiteName" 
                    ErrorMessage="<%$ Resources:ContentStrings, Site_Name_Required %>" Display="Dynamic"></asp:RequiredFieldValidator>
                <%--<asp:CustomValidator runat="server" ID="SiteNameUniqueCustomValidator" ControlToValidate="SiteName"
                    OnServerValidate="SiteNameCheck" Display="Dynamic" ErrorMessage="<%$ Resources:ContentStrings, Site_Name_AlreadyExists %>" />--%>    
            </td>
        </tr>
        <tr>
            <td colspan="2" class="FormSpacerCell">
            </td>
        </tr>
        <tr>
            <td class="FormLabelCell">
                <asp:Label runat="server" Text="<%$ Resources:SharedStrings, Description %>"></asp:Label>:</td>
            <td class="FormFieldCell">
                <asp:TextBox runat="server" TextMode="MultiLine" Width="250" MaxLength="255" ID="SiteDescription"></asp:TextBox>
            </td>
        </tr>
        <tr>
            <td colspan="2" class="FormSpacerCell">
            </td>
        </tr>
        <tr>
            <td class="FormLabelCell">
                <asp:Label runat="server" Text="<%$ Resources:SharedStrings, Is_Active %>"></asp:Label>:</td>
            <td class="FormFieldCell">
                <ecf:BooleanEditControl ID="IsSiteActive" runat="server"></ecf:BooleanEditControl>
            </td>
        </tr>
        <tr>
            <td colspan="2" class="FormSpacerCell">
            </td>
        </tr>
        <tr runat="server" id="SiteTemplateTableRow">
            <td class="FormLabelCell">
                <asp:Label ID="Label3" runat="server" Text="<%$ Resources:ContentStrings, Site_Template %>"></asp:Label>:</td>
            <td class="FormFieldCell">
                <asp:DropDownList runat="server" ID="SiteTemplatesList">
                    <asp:ListItem Value="" Text="<%$ Resources:SharedStrings, Empty_Parenthesis %>"></asp:ListItem>
                </asp:DropDownList>
            </td>
        </tr>
        <tr>
            <td colspan="2" class="FormSpacerCell">
            </td>
        </tr>
        <tr>
            <td class="FormLabelCell">
                <asp:Label runat="server" Text="<%$ Resources:ContentStrings, Site_Theme %>"></asp:Label>:</td>
            <td class="FormFieldCell">
                <asp:TextBox runat="server" Width="250" ID="SiteTheme"></asp:TextBox>
                <br />
                <asp:Label CssClass="FormFieldDescription" runat="server"
                    Text="<%$ Resources:ContentStrings, Site_Theme_Description %>"></asp:Label>
            </td>
        </tr>        
        <tr>
            <td colspan="2" class="FormSpacerCell">
            </td>
        </tr>
        <tr>
            <td class="FormLabelCell">
                <asp:Label runat="server" Text="<%$ Resources:SharedStrings, Admin_Url %>"></asp:Label>:</td>
            <td class="FormFieldCell">
                <asp:TextBox runat="server" Width="250" ID="AdminUrl"></asp:TextBox>
                <br />
                <asp:Label CssClass="FormFieldDescription" runat="server"
                    Text="<%$ Resources:ContentStrings, Site_Admin_Url_Description %>"></asp:Label>
            </td>
        </tr>          
        <tr>
            <td colspan="2" class="FormSpacerCell">
            </td>
        </tr>
        <tr>
            <td colspan="2" class="FormSectionCell">
                <asp:Literal ID="Literal7" runat="server" Text="<%$ Resources:ContentStrings, Site_Parameters_To_Identify %>"/></td>
        </tr>
        <tr>
            <td class="FormLabelCell">
                <asp:Label runat="server" Text="<%$ Resources:SharedStrings, Is_Default %>"></asp:Label>:</td>
            <td class="FormFieldCell">
                <ecf:BooleanEditControl ID="IsSiteDefault" runat="server"></ecf:BooleanEditControl>
            </td>
        </tr>
        <tr>
            <td class="FormLabelCell">
                <asp:Label runat="server" Text="<%$ Resources:SharedStrings, Domains %>"></asp:Label>:</td>
            <td class="FormFieldCell">
                <asp:TextBox runat="server" TextMode="MultiLine" Height="50" Width="250" ID="SiteDomains"></asp:TextBox><br />
                <asp:Label CssClass="FormFieldDescription" runat="server"
                    Text="<%$ Resources:ContentStrings, Site_Domain_List %>"></asp:Label>
            </td>
        </tr>
        <tr>
            <td class="FormLabelCell">
                <asp:Label runat="server" Text="<%$ Resources:ContentStrings, Site_Folder %>"></asp:Label>:</td>
            <td class="FormFieldCell">
                <asp:TextBox runat="server" Width="250" MaxLength="255" ID="SiteFolder"></asp:TextBox>
                <%--<asp:RequiredFieldValidator runat="server" ID="SiteFolderValidation" ControlToValidate="SiteFolder" 
                    ErrorMessage="<%$ Resources:ContentStrings, Site_Folder_Required %>" Display="Dynamic"></asp:RequiredFieldValidator>
                <asp:CustomValidator runat="server" ID="SiteFolderUniqueCustomValidator" ControlToValidate="SiteFolder"
                     OnServerValidate="SiteFolderCheck" Display="Dynamic" ErrorMessage="<%$ Resources:ContentStrings, Site_Folder_AlreadyExists %>" />--%>    

            </td>
        </tr>
        <tr>
            <td colspan="2" class="FormSectionCell"><asp:Literal ID="Literal5" runat="server" 
                Text="<%$ Resources:ContentStrings, Site_Languages_Configure %>"/></td>
        </tr>        
        <tr>
            <td class="FormFieldCell" colspan="2">
                <console:DualList ID="LanguageList" runat="server" ListRows="6" EnableMoveAll="True"
                    CssClass="text" LeftDataTextField="DisplayName" LeftDataValueField="Name"
                    RightDataTextField="DisplayName" RightDataValueField="Name" ItemsName="Languages">
                    <RightListStyle Font-Bold="True" Width="200px" Height="150px"></RightListStyle>
                    <ButtonStyle Width="100px"></ButtonStyle>
                    <LeftListStyle Width="200px" Height="150px"></LeftListStyle>
                </console:DualList>
            </td>
        </tr>        
        <tr>
            <td colspan="2" class="FormSectionCell">
                <asp:Literal ID="Literal6" runat="server" Text="<%$ Resources:ContentStrings, Site_Parameters_Public %>"/></td>
        </tr>
        <tr>
            <td class="FormLabelCell">
                <asp:Label runat="server" Text='<%$ Resources:SharedStrings, Title %>'></asp:Label>:</td>
            <td class="FormFieldCell">
                <asp:TextBox runat="server" Width="250" ID="txtTitle"></asp:TextBox>
            </td>
        </tr>
        <tr>
            <td colspan="2" class="FormSpacerCell">
            </td>
        </tr>
        <tr>
            <td class="FormLabelCell">
                <asp:Label runat="server" Text='<%$ Resources:ContentStrings, Site_Address %>'></asp:Label>
                <asp:Label runat="server" ID="NameLanguageCode"></asp:Label>:</td>
            <td class="FormFieldCell">
                <asp:TextBox runat="server" Width="250" ID="txtURL"></asp:TextBox>
            </td>
        </tr>
        <tr>
            <td colspan="2" class="FormSpacerCell">
            </td>
        </tr>
        <tr>
            <td class="FormLabelCell">
                <asp:Label ID="Label15" runat="server" Text="<%$ Resources:ContentStrings, Site_Page_Template %>"></asp:Label>
                <asp:Label runat="server" ID="Label16"></asp:Label>:</td>
            <td class="FormFieldCell">
                <asp:DropDownList runat="server" ID="ddTemplate" />
            </td>
        </tr>
        <tr>
            <td class="FormLabelCell">
                <asp:Label ID="Label1" runat="server" Text='<%$ Resources:SharedStrings, Email %>'></asp:Label>
                <asp:Label runat="server" ID="Label2"></asp:Label>:</td>
            <td class="FormFieldCell">
                <asp:TextBox runat="server" Width="250" ID="txtEmail"></asp:TextBox>
            </td>
        </tr>
        <tr>
            <td class="FormLabelCell">
                <asp:Label ID="Label7" runat="server" Text='<%$ Resources:SharedStrings, Day_Phone %>'></asp:Label>
                <asp:Label runat="server" ID="Label8"></asp:Label>:</td>
            <td class="FormFieldCell">
                <asp:TextBox runat="server" Width="250" ID="txtPhone"></asp:TextBox>
            </td>
        </tr>
        <tr>
            <td class="FormLabelCell">
                <asp:Label ID="Label9" runat="server" Text='<%$ Resources:SharedStrings, Address %>'></asp:Label>
                <asp:Label runat="server" ID="Label10"></asp:Label>:</td>
            <td class="FormFieldCell">
                <asp:TextBox runat="server" ID="txtAddress" TextMode="MultiLine" Rows="6" Columns="60"></asp:TextBox>
            </td>
        </tr>
        <tr>
            <td colspan="2" class="FormSectionCell"><asp:Literal ID="Literal4" runat="server" Text="<%$ Resources:ContentStrings, Site_SEO_Parameters %>"/></td>
        </tr>       
        <tr>
            <td class="FormLabelCell">
                <asp:Label ID="Label11" runat="server" Text='<%$ Resources:SharedStrings, Keywords %>'></asp:Label>
                <asp:Label runat="server" ID="Label12"></asp:Label>:</td>
            <td class="FormFieldCell">
                <asp:TextBox runat="server" TextMode="MultiLine" Rows="6" Columns="80" ID="txtKeywords"></asp:TextBox><br />
                <asp:Literal ID="Literal2" runat="server" Text="<%$ Resources:ContentStrings, Meta_Keywords_Parenthesis %>"/>
            </td>
        </tr>
        <tr>
            <td class="FormLabelCell">
                <asp:Label ID="Label13" runat="server" Text='<%$ Resources:SharedStrings, Description %>'></asp:Label>
                <asp:Label runat="server" ID="Label14"></asp:Label>:</td>
            <td class="FormFieldCell">
                <asp:TextBox runat="server" ID="txtDescription" TextMode="MultiLine" Rows="6" Columns="80"></asp:TextBox><br />
                <asp:Literal ID="Literal3" runat="server" Text="<%$ Resources:ContentStrings, Meta_Description_Parenthesis %>"/>
            </td>
        </tr>
        <tr>
            <td class="FormLabelCell">
                <asp:Label runat="server" Text="<%$ Resources:SharedStrings, Google_Analytics %>"></asp:Label>
                <asp:Label runat="server"></asp:Label>:</td>
            <td class="FormFieldCell">
                <asp:TextBox runat="server" ID="PageInclude" TextMode="MultiLine" Rows="6" Columns="80"></asp:TextBox><br />
                <asp:Literal ID="Literal1" runat="server" Text="<%$ Resources:SharedStrings, Google_Analytics_Description %>"/>
            </td>
        </tr>        
    </table>
</div>
