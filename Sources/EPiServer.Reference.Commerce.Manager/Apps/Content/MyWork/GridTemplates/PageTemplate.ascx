<%@ Control Language="C#" %>
<%@ Import Namespace="ComponentArt.Web.UI" %>
<%@ Import Namespace="Mediachase.Cms" %>
<script runat="server">
    protected string GetPreviewUrl(Guid siteId, int PageId, bool WithUserId)
    {
        string url = GlobalVariable.GetVariable("url", siteId);
        string path = GetPageUrlInternal(PageId, WithUserId);
        return url + path;       
    }

    protected string GetPageUrl(int PageId, bool WithUserId)
    {
        return GetPageUrlInternal(PageId, WithUserId);
    }
    
    protected string GetPageUrlInternal(int PageId, bool WithUserId)
    {
        string path = String.Empty;
        using (System.Data.IDataReader reader = Mediachase.Cms.FileTreeItem.GetItemById(PageId))
        {
            if (reader.Read())
            {
                if ((bool)reader["IsFolder"])
                    path += "/" + reader["Name"].ToString() + "/";
                else
                    path += "/" + reader["Name"].ToString();
            }
        }
        if (WithUserId)
        {
            //path += "?UserId=" + ProfileContext.Current.UserId.ToString();
        }
        else
        {
            //trunc path
            if (path.Length > 100)
            {
                string begin = string.Empty;
                string end = string.Empty;
                if (Regex.IsMatch(path, "/[\\w\\d]+\\u002Easpx"))
                {
                    end = Regex.Match(path, "/[\\w\\d]+\\u002Easpx").Value;
                }
                if (Regex.IsMatch(path, "^/[\\w\\d]*"))
                {
                    end = Regex.Match(path, "^/[\\w\\d]*").Value;
                }
                path = begin + "..." + end;
            }
        }
        return path.Trim();
    }

</script>
<a target="_top" href="<%#GetPreviewUrl((Guid)((GridServerTemplateContainer)Container).DataItem["SiteId"],(int)((GridServerTemplateContainer)Container).DataItem["PageId"], true) %>"><%# GetPageUrl((int)((GridServerTemplateContainer)Container).DataItem["PageId"], false)%> (#<%# ((GridServerTemplateContainer)Container).DataItem["PageId"]%>)</a>
