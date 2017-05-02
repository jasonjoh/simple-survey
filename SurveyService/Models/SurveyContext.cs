// Copyright (c) Microsoft. All rights reserved. Licensed under the MIT license. See LICENSE.txt in the project root for license information.
using System.Data.Entity;

namespace SurveyService.Models
{
    public class SurveyContext : DbContext
    {
        public DbSet<SimpleSurvey> Surveys { get; set; }
        public DbSet<Participant> Participants { get; set; }
        public DbSet<Response> Responses { get; set; }
    }
}