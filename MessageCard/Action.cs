// Copyright (c) Microsoft. All rights reserved. Licensed under the MIT license. See LICENSE.txt in the project root for license information.
using System.Runtime.Serialization;

namespace MessageCard
{
    [DataContract]
    [KnownType(typeof(OpenUri))]
    [KnownType(typeof(HttpPOST))]
    [KnownType(typeof(ActionCard))]
    public abstract class Action
    {
        [DataMember(Name = "@type")]
        private string Type
        {
            get { return this.GetType().Name; }
            set { }
        }

        [DataMember(Name = "name", EmitDefaultValue = false)]
        public string Name { get; set; }
    }
}