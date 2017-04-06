// Copyright (c) Microsoft. All rights reserved. Licensed under the MIT license. See LICENSE.txt in the project root for license information.
namespace SurveyModels
{
    public class Survey
    {
        public string Name { get; set; }
        public int Duration { get; set; }
        public string QuestionTitle { get; set; }
        public string QuestionChoices { get; set; }
    }
}
