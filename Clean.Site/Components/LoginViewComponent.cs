using Clean.Site.umbraco.models.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace Clean.Site.Components
{
    [ViewComponent(Name = "Login")]
    public class LoginViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke(LoginViewModel model)
        {
            return View(model);
        }
    }
}
