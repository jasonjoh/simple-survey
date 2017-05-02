// Copyright (c) Microsoft. All rights reserved. Licensed under the MIT license. See LICENSE.txt in the project root for license information.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using SurveyService.Models;
using SurveyService.Tokens;
using SurveyModels;
using System.Web.Http;

namespace SurveyService.Controllers
{
    public class SurveysController : ApiController
    {
        public IHttpActionResult PostSurvey(CreateSurveyRequest surveyRequest)
        {
            bool resetDb = Request.Headers.Contains("X-SimpleSurvey-ResetDB");
            string newSurveyLocation = string.Empty;
            CreateSurveyResponse response = new CreateSurveyResponse();

            try
            {
                using (var db = new SurveyContext())
                {
                    if (resetDb)
                    {
                        db.Database.Delete();
                    }

                    // Create the survey
                    SimpleSurvey newSurvey = new SimpleSurvey
                    {
                        Name = surveyRequest.Survey.Name,
                        Sender = surveyRequest.Sender,
                        Expiration = DateTime.UtcNow.AddMinutes(surveyRequest.Survey.Duration),
                        QuestionTitle = surveyRequest.Survey.QuestionTitle,
                        QuestionChoices = surveyRequest.Survey.QuestionChoices,
                        ResultsReported = false,
                        Participants = new List<Participant>()
                    };

                    // Create participants
                    foreach (string recipient in surveyRequest.Recipients)
                    {
                        // Check if recipient is already in database, create if not
                        var participant = db.Participants.FirstOrDefault(p => p.Email == recipient.ToLower());

                        if (participant == null)
                        {
                            participant = new Participant
                            {
                                Email = recipient.ToLower(),
                                TokenSalt = TokenGenerator.GenerateNewTokenSalt()
                            };

                            db.Participants.Add(participant);
                        }

                        newSurvey.Participants.Add(participant);
                    }

                    // Add survey to the database
                    db.Surveys.Add(newSurvey);
                    db.SaveChanges();

                    newSurveyLocation = Url.Link("DefaultApi", new { id = newSurvey.SimpleSurveyId });

                    response.Status = "Succeeded";
                    response.SurveyId = newSurvey.SimpleSurveyId;
                    response.Expiration = newSurvey.Expiration;
                    response.Participants = new List<SurveyParticipant>();

                    foreach (Participant participant in newSurvey.Participants)
                    {
                        response.Participants.Add(new SurveyParticipant
                        {
                            Email = participant.Email,
                            LimitedToken = TokenGenerator.GenerateLimitedToken(newSurvey.SimpleSurveyId, participant.Email, participant.TokenSalt)
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

            return Created(newSurveyLocation, response);
        }
    }
}
