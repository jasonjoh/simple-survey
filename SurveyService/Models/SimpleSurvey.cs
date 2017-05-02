// Copyright (c) Microsoft. All rights reserved. Licensed under the MIT license. See LICENSE.txt in the project root for license information.
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
    }
}