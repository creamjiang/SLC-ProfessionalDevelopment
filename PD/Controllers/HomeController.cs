﻿using System.Configuration;
using System.Net;
using System.Web.Mvc;
using Newtonsoft.Json.Linq;

namespace SLC.Controllers
{
    public class HomeController : Controller
    {
        public void Index()
        {
            var clientId = ConfigurationManager.AppSettings["clientId"];
            var clientSecret = ConfigurationManager.AppSettings["clientSecret"];
            var redirectUri = ConfigurationManager.AppSettings["redirectUri"];

            // We need an access token to call the API.  If we don't have one, let's get it, otherwise, redirect to main.aspx.
            if (Session["access_token"] == null)
            {
                // We get a code back from the first leg of OAuth process.  If we don't have one, let's get it.
                if (Request.QueryString["code"] == null)
                {
                    // Here the user will log into the SLC.  This page (start.aspx) will be called back with the code to do second leg of OAuth.
                    var authorizeUrl =
                        string.Format(
                            "https://api.sandbox.slcedu.org/api/oauth/authorize?client_id={0}&redirect_uri={1}",
                            clientId, redirectUri);
                    Response.Redirect(authorizeUrl);
                }
                else
                {
                    // Now we have a code, we can run the second leg of OAuth process.
                    var code = Request.QueryString["code"];

                    // Set the authorization URL
                    var sessionUrl =
                        string.Format(
                            "https://api.sandbox.slcedu.org/api/oauth/token?client_id={0}&client_secret={1}&grant_type=authorization_code&redirect_uri={2}&code={3}",
                            clientId, clientSecret, redirectUri, code);

                    var restClient = new WebClient();

                    restClient.Headers.Add("Content-Type", "application/vnd.slc+json");
                    restClient.Headers.Add("Accept", "application/vnd.slc+json");

                    // Call authorization endpoint
                    var result = restClient.DownloadString(sessionUrl);

                    // Convert response into a JSON object
                    var response = JObject.Parse(result);
                    var access_token = (string) response["access_token"];

                    // If we have a valid token, it'll be 38 chars long.  Let's add it to session if so.
                    if (access_token.Length == 38)
                    {
                        Session.Add("access_token", access_token);

                        // Redirect to app main page.
                        //Response.Redirect("main.aspx");
                        Response.Redirect("/Home/Main");
                        //RedirectToAction("Main", "Home");
                    }
                }
            }
            else
            {
                // We have an access token in session, let's redirect to app main page.
                Response.Redirect("/Home/Main");
                //RedirectToAction("Main", "Home");
            }
        }

        public ActionResult Main()
        {
            return View();
        }

        public ActionResult Plan()
        {
            return View();
        }

        public ActionResult Resources()
        {
            return View();
        }

        // This is a demo to see if we can get REAL SLC data coming though!
        // To test, go to /Home/MyProfile and you will see the JSON in the browser
        public JsonResult MyProfile()
        {
            var result = "";

            // Do we have a valid access token in session?  If yes, let's make an API call.
            if (Session["access_token"] != null)
            {
                var lindaKimTeacherId = "2012rr-08e09b06-3123-11e2-ad37-02786541ab34";
                var apiEndPoint = string.Format("https://api.sandbox.slcedu.org/api/rest/v1/teachers/{0}", lindaKimTeacherId);

                var restClient = new WebClient();

                var bearerToken = string.Format("bearer {0}", Session["access_token"]);

                restClient.Headers.Add("Authorization", bearerToken);
                restClient.Headers.Add("Content-Type", "application/vnd.slc+json");
                restClient.Headers.Add("Accept", "application/vnd.slc+json");

                result = restClient.DownloadString(apiEndPoint);

            }
            return Json(result, JsonRequestBehavior.AllowGet);
        }
    }
}