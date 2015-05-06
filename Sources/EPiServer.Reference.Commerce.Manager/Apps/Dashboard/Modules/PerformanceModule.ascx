<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="PerformanceModule.ascx.cs"
    Inherits="Mediachase.Commerce.Manager.Apps.Dashboard.Modules.PerformanceModule" %>
<%@ Register Assembly="Microsoft.ReportViewer.WebForms"
    Namespace="Microsoft.Reporting.WebForms" TagPrefix="rsweb" %>
<rsweb:ReportViewer ID="PerfReportViewer" runat="server" ShowDocumentMapButton="False"
    ShowExportControls="False" ShowFindControls="False" ShowPageNavigationControls="False"
    ShowPrintButton="False" ShowPromptAreaButton="False" ShowRefreshButton="False"
    ShowZoomControl="False" Font-Names="Verdana" Font-Size="8pt" ShowToolbar="False"
    Height="600px" Width="100%">
    <LocalReport>
    </LocalReport>
</rsweb:ReportViewer>
