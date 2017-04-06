// Copyright (c) Microsoft. All rights reserved. Licensed under the MIT license. See LICENSE.txt in the project root for license information.
using System.Runtime.Serialization;

namespace MessageCard
{
    [DataContract]
    public class TextInput : Input
    {
        [DataMember(Name = "isMultiline", EmitDefaultValue = false)]
        public bool IsMultiline { get; set; }
    }
}
