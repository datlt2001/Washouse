using Microsoft.AspNetCore.Mvc;

namespace Washouse.Web.Controllers
{
    public class PromotionController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
