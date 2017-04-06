// Copyright (c) Microsoft. All rights reserved. Licensed under the MIT license. See LICENSE.txt in the project root for license information.
using System.Runtime.Serialization;

namespace MessageCard
{
    [DataContract]
    public class HttpPOST : ExternalAction
    {
        [DataMember(Name = "target", EmitDefaultValue = false)]
        public string Target { get; set; }

        [DataMember(Name = "body", EmitDefaultValue = false)]
        public string Body { get; set; }

        [DataMember(Name = "bodyContentType", EmitDefaultValue = false)]
        public string BodyContentType { get; set; }
    }
}
