<%@ Control Language="C#" AutoEventWireup="true" Inherits="Apps_Marketing_Modules_CategoryFilter" Codebehind="CategoryFilter.ascx.cs" %>
<ComponentArt:ComboBox ID="CategoryFilter" RunningMode="CallBack" runat="server" AutoHighlight="false" AutoFilter="true" AutoComplete="true" 
    CssClass="comboBox"
    HoverCssClass="comboBoxHover"
    FocusedCssClass="comboBoxHover"
    TextBoxCssClass="comboTextBox"
    TextBoxHoverCssClass="comboBoxHover"
    DropDownCssClass="comboDropDown"
    ItemCssClass="comboItem"
    ItemHoverCssClass="comboItemHover"
    SelectedItemCssClass="comboItemHover"
    DropHoverImageUrl="~/Apps/Shell/Styles/images/combobox/drop_hover.gif"
    DropImageUrl="~/Apps/Shell/Styles/images/combobox/drop.gif"
    DropDownPageSize="10" CacheSize="200" FilterCacheSize="1"
    Width="350px" ItemClientTemplateId="itemTemplate">
    <ClientTemplates>
        <ComponentArt:ClientTemplate ID="itemTemplate">
            <img src="## DataItem.getProperty('icon') ##" />
            ## DataItem.getProperty('Text') ##</ComponentArt:ClientTemplate>
    </ClientTemplates>
</ComponentArt:ComboBox>
<asp:RequiredFieldValidator ID="RequiredValidator" runat="server" ControlToValidate="CategoryFilter" Display="Dynamic" ErrorMessage="*"></asp:RequiredFieldValidator>