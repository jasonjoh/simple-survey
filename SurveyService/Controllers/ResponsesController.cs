// Copyright (c) Microsoft. All rights reserved. Licensed under the MIT license. See LICENSE.txt in the project root for license information.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using SurveyModels;
using SurveyService.Models;
using SurveyService.Tokens;
using Microsoft.O365.ActionableMessages.Authentication;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using MessageCard;

namespace SurveyService.Controllers
{
    public class ResponsesController : ApiController
    {
        // TODO: Update this value with the new ngrok forwarding URL 
        // each time you restart ngrok
        private string actionBaseUrl = "YOUR NGROK URL";

        public async Task<IHttpActionResult> PostResponse(RespondToSurveyRequest surveyResponse)
        {
            // Validate the authorization header
            bool isTokenValid = await ValidateAuthorizationHeader(Request.Headers.Authorization,
                actionBaseUrl, surveyResponse.UserId);
            if (!isTokenValid)
            {
                return Unauthorized();
            }

            try
            {
                using (var db = new SurveyContext())
                {
                    // Make sure the survey ID is valid
                    SimpleSurvey survey = db.Surveys.Find(surveyResponse.SurveyId);
                    if (survey == null)
                    {
                        return BadRequest("Invalid survey ID");
                    }

                    // Is the response a valid one?
                    string[] validResponses = survey.QuestionChoices.Split(';');
                    int userResponseIndex = Convert.ToInt32(surveyResponse.Response) - 1;
                    if (userResponseIndex < 0 || userResponseIndex >= validResponses.Length)
                    {
                        return BadRequest("Invalid response");
                    }

                    // Is the user a participant?
                    Participant responder = survey.Participants.FirstOrDefault(p => p.Email == surveyResponse.UserId.ToLower());
                    if (responder == null)
                    {
                        return BadRequest("Invalid participant");
                    }

                    // Is the limited purpose token a match?
                    string expectedToken = TokenGenerator.GenerateLimitedToken(survey.SimpleSurveyId, responder.Email, responder.TokenSalt);
                    if (surveyResponse.LimitedToken != expectedToken)
                    {
                        return BadRequest("Limited purpose token validation failed");
                    }

                    // Has the user already responded?
                    Response previousResponse = survey.Responses.FirstOrDefault(r => r.Participant == responder);
                    if (previousResponse != null)
                    {
                        return GenerateFriendlyResponse("You've already responded to this survey!");
                    }

                    // Has the survey expired?
                    if (DateTime.Compare(survey.Expiration, DateTime.UtcNow) < 0)
                    {
                        return GenerateFriendlyResponse("This survey's response window has closed! Your response has not been recorded.");
                    }

                    // Create the response object
                    Response response = new Response
                    {
                        Participant = responder,
                        ParticipantResponse = validResponses[userResponseIndex],
                        Survey = survey
                    };

                    db.Responses.Add(response);
                    db.SaveChanges();

                    return GenerateRefreshCardResponse(survey.GenerateSurveyResultSnapshotCard());
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        private async Task<bool> ValidateAuthorizationHeader(AuthenticationHeaderValue authHeader, string targetUrl, string userId)
        {
            // Validate that we have a bearer token
            if (authHeader == null ||
                !string.Equals(authHeader.Scheme, "bearer", StringComparison.OrdinalIgnoreCase) ||
                string.IsNullOrEmpty(authHeader.Parameter))
            {
                return false;
            }

            // Validate the token
            ActionableMessageTokenValidator validator = new ActionableMessageTokenValidator();
            ActionableMessageTokenValidationResult result = await validator.ValidateTokenAsync(authHeader.Parameter, targetUrl);
            if (!result.ValidationSucceeded)
            {
                return false;
            }

            // Token is valid, now check the sender and action performer
            // Both should equal the user
            if (!string.Equals(result.ActionPerformer, userId, StringComparison.OrdinalIgnoreCase) ||
                !string.Equals(result.Sender, userId, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            return true;
        }

        private IHttpActionResult GenerateFriendlyResponse(string message)
        {
            HttpResponseMessage friendlyResponse = new HttpResponseMessage(HttpStatusCode.BadRequest);
            friendlyResponse.Headers.Add("CARD-ACTION-STATUS", message);
            return ResponseMessage(friendlyResponse);
        }

        private IHttpActionResult GenerateRefreshCardResponse(Card refreshCard)
        {
            HttpResponseMessage refreshCardResponse = new HttpResponseMessage(HttpStatusCode.OK);
            refreshCardResponse.Headers.Add("CARD-UPDATE-IN-BODY", "true");

            // Serialize the card as JSON to the response body
            refreshCardResponse.Content = new StringContent(refreshCard.ToJson(), System.Text.Encoding.UTF8, "application/json");
            return ResponseMessage(refreshCardResponse);
        }
    }
}
