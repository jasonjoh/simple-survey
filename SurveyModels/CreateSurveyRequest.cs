// Copyright (c) Microsoft. All rights reserved. Licensed under the MIT license. See LICENSE.txt in the project root for license information.
namespace SurveyModels
{
    public class CreateSurveyRequest
    {
        public Survey Survey { get; set; }
        public string Sender { get; set; }
        public string[] Recipients { get; set; }
    }
}
