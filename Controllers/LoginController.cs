using Microsoft.AspNetCore.Mvc;

namespace QuailtyForm.Controllers
{
    public class LoginController : Controller
    {
        public IActionResult Login()
        {
            return View();
        }
    }
}
