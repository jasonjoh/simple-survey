// Copyright (c) Microsoft. All rights reserved. Licensed under the MIT license. See LICENSE.txt in the project root for license information.
using System.Runtime.Serialization;

namespace MessageCard
{
    [DataContract]
    [KnownType(typeof(TextInput))]
    [KnownType(typeof(MultichoiceInput))]
    public abstract class Input
    {
        [DataMember(Name = "@type")]
        private string Type
        {
            get { return this.GetType().Name; }
            set { }
        }

        [DataMember(Name = "id", EmitDefaultValue = false)]
        public string Id { get; set; }

        [DataMember(Name = "title", EmitDefaultValue = false)]
        public string Title { get; set; }

        [DataMember(Name = "value", EmitDefaultValue = false)]
        public string Value { get; set; }

        [DataMember(Name = "isRequired", EmitDefaultValue = false)]
        public bool IsRequired { get; set; }
    }
}
