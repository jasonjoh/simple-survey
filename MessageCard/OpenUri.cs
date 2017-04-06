// Copyright (c) Microsoft. All rights reserved. Licensed under the MIT license. See LICENSE.txt in the project root for license information.
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace MessageCard
{
    [DataContract]
    public class OpenUriTarget
    {
        [DataMember(Name = "os", EmitDefaultValue = false)]
        public string OS { get; set; }

        [DataMember(Name = "uri", EmitDefaultValue = false)]
        public string Uri { get; set; }
    }

    [DataContract]
    public class OpenUri : ExternalAction
    {
        [DataMember(Name = "targets", EmitDefaultValue = false)]
        public List<OpenUriTarget> Targets { get; set; }
    }
}
