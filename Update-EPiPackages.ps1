# This script is designed to be run in the package manager console in Visual Studio to update
# dependencies to EPiServer packages from nuget.episerver.com. 
Update-Package EPiServer.Commerce -Source http://nuget.episerver.com/nuget
Update-Package EPiServer.Commerce.UI.ManagerIntegration -Source http://nuget.episerver.com/nuget
Update-Package EPiServer.CommerceManager -Source http://nuget.episerver.com/nuget
Update-Package EPiServer.Commerce.Core -Source http://nuget.episerver.com/nuget
Update-Package EPiServer.Tracking.Commerce -Source http://nuget.episerver.com/nuget
Update-Package EPiServer.Personalization.Commerce -Source http://nuget.episerver.com/nuget
Update-Package EPiServer.CMS -Source http://nuget.episerver.com/nuget
Update-Package EPiServer.CMS.UI -Source http://nuget.episerver.com/nuget
Update-Package EPiServer.CMS.Core -Source http://nuget.episerver.com/nuget