// Copyright (c) Microsoft. All rights reserved. Licensed under the MIT license. See LICENSE.txt in the project root for license information.
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace MessageCard
{
    [DataContract]
    public class Choice
    {
        [DataMember(Name = "display", EmitDefaultValue = false)]
        public string Display { get; set; }

        [DataMember(Name = "value", EmitDefaultValue = false)]
        public string Value { get; set; }
    }

    [DataContract]
    public class MultichoiceInput : Input
    {
        [DataMember(Name = "choices", EmitDefaultValue = false)]
        public List<Choice> Choices { get; set; }
    }
}
