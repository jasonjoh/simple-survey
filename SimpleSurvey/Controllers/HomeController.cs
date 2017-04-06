// Copyright (c) Microsoft. All rights reserved. Licensed under the MIT license. See LICENSE.txt in the project root for license information.
using Microsoft.Graph;
using Microsoft.Identity.Client;
using SimpleSurvey.TokenStorage;
using System.Configuration;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace SimpleSurvey.Controllers
{
    public class HomeController : Controller
    {
        public async Task<ActionResult> Index()
        {
            if (Request.IsAuthenticated)
            {
                string userName = ClaimsPrincipal.Current.FindFirst("name").Value;
                string userId = ClaimsPrincipal.Current.FindFirst(ClaimTypes.NameIdentifier).Value;
                if (string.IsNullOrEmpty(userName) || string.IsNullOrEmpty(userId))
                {
                    // Invalid principal, sign out
                    return RedirectToAction("SignOut", "Account");
                }

                // Since we cache tokens in the session, if the server restarts
                // but the browser still has a cached cookie, we may be
                // authenticated but not have a valid token cache. Check for this
                // and force signout.
                SessionTokenCache tokenCache = new SessionTokenCache(userId, HttpContext);
                if (tokenCache.Count <= 0)
                {
                    // Cache is empty, sign out
                    return RedirectToAction("SignOut", "Account");
                }

                // Create a Graph client
                GraphServiceClient graphClient = new GraphServiceClient(
                    new DelegateAuthenticationProvider(AddAccessToken));

                // Get the sender's email address
                Microsoft.Graph.User sender = await graphClient.Me.Request().GetAsync();
                ViewBag.SenderEmail = sender.Mail;
            }

            ViewBag.ShowSurveyForm = Request.IsAuthenticated;
            return View();
        }

        private async Task AddAccessToken(System.Net.Http.HttpRequestMessage request)
        {
            // Load the app config from web.config
            string appId = ConfigurationManager.AppSettings["ida:AppId"];
            string appSecret = ConfigurationManager.AppSettings["ida:AppSecret"];
            string redirectUri = ConfigurationManager.AppSettings["ida:RedirectUri"];

            // Get the current user's ID
            string userId = ClaimsPrincipal.Current.FindFirst(ClaimTypes.NameIdentifier).Value;

            // Get the user's token cache
            SessionTokenCache tokenCache = new SessionTokenCache(userId, HttpContext);

            ConfidentialClientApplication cca = new ConfidentialClientApplication(
                appId, redirectUri, new ClientCredential(appSecret), tokenCache);

            // Call AcquireTokenSilentAsync, which will return the cached
            // access token if it has not expired. If it has expired, it will
            // handle using the refresh token to get a new one.
            string[] scopes = { "User.Read", "Mail.Send" };
            AuthenticationResult result = await cca.AcquireTokenSilentAsync(scopes);

            // Set the token on the request
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", result.Token);
        }
    }
}