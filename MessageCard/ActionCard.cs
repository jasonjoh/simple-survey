// Copyright (c) Microsoft. All rights reserved. Licensed under the MIT license. See LICENSE.txt in the project root for license information.
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace MessageCard
{
    [DataContract]
    public class ActionCard : Action
    {
        [DataMember(Name = "inputs", EmitDefaultValue = false)]
        public List<Input> Inputs { get; set; }

        [DataMember(Name = "actions", EmitDefaultValue = false)]
        public List<ExternalAction> Actions { get; set; }
    }
}
