using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace MvcWebApi.Controllers
{
    
    public class HomeController : Controller
    {
        public string post = "Hello World";
        public string result { get; set; }
        public  ActionResult Index()
        {
            var client = new HttpClient();
            var postcontent = new StringContent(post);
            var postUrl = "http://localhost:7114/api/product/PostRawContent";

            var task = client.PostAsync(postUrl, postcontent).Result;
            GetContent(task.Content);
            return View();
        }

        public async Task<string> GetContent(HttpContent content)
        {
            string result = await content.ReadAsStringAsync();
            this.result = result;
            var ss = "\"" + post + "\"";
            if(result=="\""+post+"\""){
            
            }
            ViewBag.result = this.result;
            return result;
        }
    }
}
