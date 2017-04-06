// Copyright (c) Microsoft. All rights reserved. Licensed under the MIT license. See LICENSE.txt in the project root for license information.
using MessageCard;
using System;
using System.Collections.Generic;

namespace SurveyService.Models
{
    public class SimpleSurvey
    {
        public int SimpleSurveyId { get; set; }
        public string Sender { get; set; }
        public string Name { get; set; }
        public DateTime Expiration { get; set; }
        public string QuestionTitle { get; set; }
        public string QuestionChoices { get; set; }
        public bool ResultsReported { get; set; }
        public virtual List<Participant> Participants { get; set; }
        public virtual List<Response> Responses { get; set; }

        public Card GenerateSurveyResultSnapshotCard()
        {
            Card statsCard = new Card();
            statsCard.ThemeColor = "00B200";
            statsCard.HideOriginalBody = true;
            statsCard.Title = $"Thanks for responding to \"{Name}\"";

            Section section = new Section();
            section.Title = $"The question was: \"{QuestionTitle}\"";
            section.Text = $"Responses as of {DateTime.Now.ToString("MM/dd/yy \"at\" HH:mm \"UTC\"")}";

            List<Fact> facts = new List<Fact>();

            string[] surveyChoices = QuestionChoices.Split(';');
            foreach (string choice in surveyChoices)
            {
                int numResponses = Responses.FindAll(r => string.Equals(r.ParticipantResponse, choice, StringComparison.OrdinalIgnoreCase)).Count;
                facts.Add(new Fact() { Name = choice, Value = numResponses.ToString() });
            }

            section.Facts = facts;

            statsCard.Sections = new List<Section>() { section };

            return statsCard;
        }

        public Card GenerateSurveyResultFinalCard()
        {
            Card statsCard = new Card();
            statsCard.ThemeColor = "00B200";
            statsCard.HideOriginalBody = true;
            statsCard.Title = $"Final result for \"{Name}\"";
            statsCard.Text = $"{Responses.Count} of {Participants.Count} participants responded.";

            Section section = new Section();
            section.Title = $"The question was: \"{QuestionTitle}\"";
            section.Text = "Here's how particpants responded";

            List<Fact> facts = new List<Fact>();

            string[] surveyChoices = QuestionChoices.Split(';');
            foreach (string choice in surveyChoices)
            {
                int numResponses = Responses.FindAll(r => string.Equals(r.ParticipantResponse, choice, StringComparison.OrdinalIgnoreCase)).Count;
                facts.Add(new Fact() { Name = choice, Value = numResponses.ToString() });
            }

            section.Facts = facts;

            section.Actions = new List<MessageCard.Action>()
            {
                // Add a button to open a URL
                // For example, this could be a link into a web app that displays the
                // results in a rich manner 
                new OpenUri()
                {
                    Name = "View Result Details",
                    Targets = new List<OpenUriTarget>()
                    {
                        new OpenUriTarget() { OS = "default", Uri = "https://..." }
                    }
                }
            };

            statsCard.Sections = new List<Section>() { section };

            return statsCard;
        }
    }
}