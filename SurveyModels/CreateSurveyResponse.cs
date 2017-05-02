// Copyright (c) Microsoft. All rights reserved. Licensed under the MIT license. See LICENSE.txt in the project root for license information.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SurveyModels
{
    public class SurveyParticipant
    {
        public string Email { get; set; }
        public string LimitedToken { get; set; }
    }
    public class CreateSurveyResponse
    {
        public string Status { get; set; }
        public int SurveyId { get; set; }
        public DateTime Expiration { get; set; }
        public List<SurveyParticipant> Participants { get; set; }
    }
}
