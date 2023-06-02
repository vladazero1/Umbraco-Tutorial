using Umbraco.Cms.Web.Common.PublishedModels;

namespace Clean.Site.interfaces
{
    public interface ISettingsService
    {
        Settings SettingsPage { get; }

        HomePage Homepage { get; }

        string HompageAbsoluteUrl { get; }
    }
}