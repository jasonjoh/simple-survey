// Copyright (c) Microsoft. All rights reserved. Licensed under the MIT license. See LICENSE.txt in the project root for license information.
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace MessageCard
{
    [DataContract]
    public class Section
    {
        [DataMember(Name = "title", EmitDefaultValue = false)]
        public string Title { get; set; }

        [DataMember(Name = "text", EmitDefaultValue = false)]
        public string Text { get; set; }

        [DataMember(Name = "activityImage", EmitDefaultValue = false)]
        public string ActivityImage { get; set; }

        [DataMember(Name = "activityTitle", EmitDefaultValue = false)]
        public string ActivityTitle { get; set; }

        [DataMember(Name = "activitySubtitle", EmitDefaultValue = false)]
        public string ActivitySubtitle { get; set; }

        [DataMember(Name = "activityText", EmitDefaultValue = false)]
        public string ActivityText { get; set; }

        [DataMember(Name = "heroImage", EmitDefaultValue = false)]
        public Image HeroImage { get; set; }

        [DataMember(Name = "facts", EmitDefaultValue = false)]
        public List<Fact> Facts { get; set; }

        [DataMember(Name = "images", EmitDefaultValue = false)]
        public List<Image> Images { get; set; }

        [DataMember(Name = "potentialAction", EmitDefaultValue = false)]
        public List<Action> Actions { get; set; }
    }
}
