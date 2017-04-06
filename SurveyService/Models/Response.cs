// Copyright (c) Microsoft. All rights reserved. Licensed under the MIT license. See LICENSE.txt in the project root for license information.
namespace SurveyService.Models
{
    public class Response
    {
        public int ResponseId { get; set; }
        public virtual SimpleSurvey Survey { get; set; }
        public virtual Participant Participant { get; set; }
        public string ParticipantResponse { get; set; }
    }
}