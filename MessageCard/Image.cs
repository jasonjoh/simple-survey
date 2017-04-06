// Copyright (c) Microsoft. All rights reserved. Licensed under the MIT license. See LICENSE.txt in the project root for license information.
using System.Runtime.Serialization;

namespace MessageCard
{
    [DataContract]
    public class Image
    {
        [DataMember(Name = "image", EmitDefaultValue = false)]
        public string Url { get; set; }

        [DataMember(Name = "title", EmitDefaultValue = false)]
        public string AltText { get; set; }
    }
}
