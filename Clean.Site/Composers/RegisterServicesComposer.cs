using Clean.Site.Services;
using Umbraco.Cms.Core.Composing;

namespace Clean.Site.Composers
{
    public class RegisterServicesComposer : IComposer
    {
        public void Compose(IUmbracoBuilder builder)
        {
            builder.Services.AddTransient<IAccountService, AccountService>();
            builder.Services.AddTransient<IDataTypeValueService, DataTypeValueService>();
            builder.Services.AddTransient<IMediaUploadService, MediaUploadService>();
        }
    }
}
