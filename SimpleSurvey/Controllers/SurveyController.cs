// Copyright (c) Microsoft. All rights reserved. Licensed under the MIT license. See LICENSE.txt in the project root for license information.
using MessageCard;
using Microsoft.Graph;
using Microsoft.Identity.Client;
using SimpleSurvey.TokenStorage;
using SurveyModels;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace SimpleSurvey.Controllers
{
    public class SurveyController : Controller
    {
        [Authorize]
        [HttpPost]
        public async Task<ActionResult> SendSurvey(Survey Survey, string ToRecipients)
        {
            // Split the recipient list
            string[] recipients = ToRecipients.Split(new char[]{';'}, StringSplitOptions.RemoveEmptyEntries);

            // Create a Graph client
            GraphServiceClient graphClient = new GraphServiceClient(
                new DelegateAuthenticationProvider(AddAccessToken));

            // Get the sender's email address
            Microsoft.Graph.User sender = await graphClient.Me.Request().GetAsync();
            string senderEmail = sender.Mail;

            // TODO: Send survey, sender, and recipients to Web API
            // Web API will add survey to database and return a survey ID + 
            // closing date/time +
            // array of per-user limited purpose tokens
            string closingTime = DateTime.UtcNow.AddMinutes((double)Survey.Duration).ToString();
            string surveyId = "12345";
            Dictionary<string, string> recipientTokens = new Dictionary<string, string>();
            foreach (string recip in recipients)
            {
                recipientTokens.Add(recip, "faketoken");
            }

            // Send the survey
            string[] errors = await SendSurvey(surveyId, Survey, recipientTokens, closingTime, graphClient);

            return Json(errors);
        }

        private async Task<string[]> SendSurvey(string surveyId, Survey survey, Dictionary<string,string> toRecipients, string closingTime, GraphServiceClient graphClient)
        {
            List<string> errorMessages = new List<string>();

            foreach (KeyValuePair<string,string> recipient in toRecipients)
            {
                string error = await SendSurvey(surveyId, survey, recipient.Key, recipient.Value, closingTime, graphClient);
                if (!string.IsNullOrEmpty(error))
                {
                    errorMessages.Add(error);
                }
            }

            return errorMessages.ToArray();
        }

        private async Task<string> SendSurvey(string surveyId, Survey survey, string recipient, string token, string closingTime, GraphServiceClient graphClient)
        {
            // Build up the card
            Card card = new Card();
            card.ThemeColor = "00B200";
            card.Title = survey.Name;
            card.Text = "Survey closes at **" + closingTime + " (UTC)**";

            card.HideOriginalBody = true;
            card.Sections = new List<Section>();

            Section section = new Section();
            section.Title = survey.QuestionTitle;
            section.Actions = new List<MessageCard.Action>();

            ActionCard actionCard = new ActionCard();
            actionCard.Name = "Respond";
            actionCard.Inputs = new List<Input>();

            MultichoiceInput input = new MultichoiceInput();
            input.Id = "input";
            input.IsRequired = true;
            input.Title = "Select an option";
            input.Choices = new List<Choice>();

            string[] choices = survey.QuestionChoices.Split(';');
            for (int i = 0; i < choices.Length; i++)
            {
                input.Choices.Add(new Choice() { Display = choices[i], Value = (i + 1).ToString() });
            }

            actionCard.Inputs.Add(input);
            actionCard.Actions = new List<ExternalAction>()
            {
                // This HttpPOST action is defined so the following request is sent to the service:
                //
                // POST <service URL>
                // <Other HTTP headers>
                // ContentType: application/json
                //
                // {
                //      "UserId": "<id of user>",
                //      "SurveyId": "<id of the survey being responded to>",
                //      "Response": "{{input.value}}"
                // }
                new HttpPOST() {
                    Name = "Submit",
                    Target = "https://...", // TODO: Fix this with the real URL to web API
                    Body = "{ \"UserId\": \"" + recipient + "\", \"SurveyId\": \"" + surveyId + "\", \"LimitedToken\": \"" + token + "\", \"Response\": \"{{input.value}}\" }",
                    BodyContentType = "application/json"
                }
            };

            section.Actions.Add(actionCard);
            card.Sections.Add(section);

            Recipient toRecip = new Recipient()
            {
                EmailAddress = new EmailAddress() { Address = recipient }
            };

            // Create the message
            Message actionableMessage = new Message()
            {
                Subject = "RESPONSE REQUESTED: " + survey.Name,
                ToRecipients = new List<Recipient>() { toRecip },
                Body = new ItemBody()
                {
                    ContentType = BodyType.Html,
                    Content = LoadSurveyMessageBody(card)
                }
            };

            try
            {
                await graphClient.Me.SendMail(actionableMessage, true).Request().PostAsync();
            } 
            catch (ServiceException graphEx)
            {
                return string.Format("Send to {0} failed - {1}: {2}", recipient, graphEx.Error.Code, graphEx.Error.Message);
            }

            return string.Empty;
        }

        private string LoadSurveyMessageBody(Card surveyCard)
        {
            return
                "<html>" +
                "  <head>" +
                "    <meta http-equiv=\"Content-Type\" content=\"text/html; charset=utf-8\">" +
                "    <script type=\"application/ld+json\">" +
                surveyCard.ToJson() +
                "    </script>" +
                "  <head>" +
                "  <body>" +
                "    Your email client cannot display this survey." +
                "  </body>" +
                "</html>";
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