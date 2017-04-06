// Copyright (c) Microsoft. All rights reserved. Licensed under the MIT license. See LICENSE.txt in the project root for license information.
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;

namespace MessageCard
{
    [DataContract]
    public class Card
    {
        public string ToJson()
        {
            DataContractJsonSerializerSettings settings = new DataContractJsonSerializerSettings();
            settings.EmitTypeInformation = EmitTypeInformation.Never;

            DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(Card), settings);

            using (MemoryStream stream = new MemoryStream())
            {
                serializer.WriteObject(stream, this);

                stream.Position = 0;

                using (StreamReader reader = new StreamReader(stream))
                {
                    return reader.ReadToEnd();
                }
            }
        }

        [DataMember(Name = "@type")]
        private string Type { get; set; } = "MessageCard";

        [DataMember(Name = "@context")]
        private string Context { get; set; } = "http://schema.org/extensions";

        [DataMember(Name = "hideOriginalBody", EmitDefaultValue = false)]
        public bool HideOriginalBody { get; set; }

        [DataMember(Name = "themeColor", EmitDefaultValue = false)]
        public string ThemeColor { get; set; }

        [DataMember(Name = "title", EmitDefaultValue = false)]
        public string Title { get; set; }

        [DataMember(Name = "text", EmitDefaultValue = false)]
        public string Text { get; set; }

        [DataMember(Name = "sections", EmitDefaultValue = false)]
        public List<Section> Sections { get; set; }
    }
}
