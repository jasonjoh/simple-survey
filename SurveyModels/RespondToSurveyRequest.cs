// Copyright (c) Microsoft. All rights reserved. Licensed under the MIT license. See LICENSE.txt in the project root for license information.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SurveyModels
{
    public class RespondToSurveyRequest
    {
        public string UserId { get; set; }
        public int SurveyId { get; set; }
        public string LimitedToken { get; set; }
        public string Response { get; set; }
    }
}
