using Clean.Site.umbraco.models.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace Clean.Site.Components
{
    [ViewComponent(Name = "Register")]
    public class RegisterViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke(RegisterViewModel model)
        {
            return View(model);
        }
    }
}
