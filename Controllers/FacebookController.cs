using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Facebook;

namespace FBGroupPublisherMVC.Controllers
{
    public class FacebookController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
        [HttpGet]
        public IActionResult Login()
        {
            var redirectUrl = Url.Action("FacebookCallback", "Facebook");
            var properties = new AuthenticationProperties { RedirectUri = redirectUrl };
            return Challenge(properties, "Facebook");
        }
        [HttpGet]
        public async Task<IActionResult> FacebookCallback()
        {
            var result = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            if (!result.Succeeded)
                return BadRequest(); // Handle this better in a real app

            var accessToken = result.Properties.GetTokenValue("access_token");
            HttpContext.Session.SetString("AccessToken", accessToken);

            return RedirectToAction("GetGroups");
        }

        [HttpGet]
        public async Task<IActionResult> GetGroups()
        {
            var accessToken = HttpContext.Session.GetString("AccessToken");
            if (accessToken == null)
                return RedirectToAction("Login");

            var fb = new FacebookClient(accessToken);

            dynamic groups;
            try 
            {
                groups = await fb.GetTaskAsync("me/groups");
            }
            catch (FacebookOAuthException ex)
            {
                ViewBag.Message = "Error: " + ex.Message;
                return View();
            }

            var groupList = new List<string>();
            foreach (var group in groups.data)
            {
                groupList.Add(group.name);
            }

            ViewBag.Groups = groupList;
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> PostToFacebook()
        {
            var accessToken = HttpContext.Session.GetString("AccessToken");
            if (accessToken == null)
                return RedirectToAction("Login");

            var fb = new FacebookClient(accessToken);

            dynamic messagePost = new
            {
                message = "Hello, world!"
            };

            try
            {
                dynamic result = await fb.PostTaskAsync("me/feed", messagePost);
                ViewBag.Message = "Post Id: " + result.id;
            }
            catch (FacebookOAuthException ex)
            {
                ViewBag.Message = "Error: " + ex.Message;
            }

            return View();
        }
    }
}
