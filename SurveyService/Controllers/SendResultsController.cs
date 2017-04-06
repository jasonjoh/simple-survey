// Copyright (c) Microsoft. All rights reserved. Licensed under the MIT license. See LICENSE.txt in the project root for license information.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using SurveyService.Models;
using MessageCard;
using System.Threading.Tasks;

namespace SurveyService.Controllers
{
    public class SendResultsController : ApiController
    {
        private string webHookUrl = "https://outlook.office.com/webhook/47246dcf-e199-4ae9-9aa1-cd41e29a4c8d@c4dd2789-1144-4832-bfaa-87f9955f5ef6/IncomingWebhook/463dca07568f496c982ca4ebcf8e181f/1b1a2064-54a3-4f6e-b187-c1388c69fc0c";

        public async Task<IHttpActionResult> PostSendResults()
        {
            try
            {
                using (var db = new SurveyContext())
                {
                    var expiredSurveys = db.Surveys.Where(s => !s.ResultsReported && DateTime.Compare(s.Expiration, DateTime.UtcNow) < 0).ToList();

                    if (expiredSurveys.Count > 0)
                    {
                        using (var client = new HttpClient())
                        {
                            foreach (SimpleSurvey survey in expiredSurveys)
                            {
                                // Generate the card
                                Card resultsCard = survey.GenerateSurveyResultFinalCard();

                                // Post the card to the webhook
                                HttpResponseMessage response = await client.PostAsJsonAsync(webHookUrl, resultsCard);
                                response.EnsureSuccessStatusCode();

                                // Mark the survey as reported
                                survey.ResultsReported = true;
                                db.SaveChanges();
                            }
                        }
                    }
                }

                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
