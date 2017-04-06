// Copyright (c) Microsoft. All rights reserved. Licensed under the MIT license. See LICENSE.txt in the project root for license information.
using System.Collections.Generic;

namespace SurveyService.Models
{
    public class Participant
    {
        public int ParticipantId { get; set; }
        public string Email { get; set; }
        public string TokenSalt { get; set; }
        public virtual List<SimpleSurvey> Surveys { get; set; }
        public virtual List<Response> Responses { get; set; }
    }
}