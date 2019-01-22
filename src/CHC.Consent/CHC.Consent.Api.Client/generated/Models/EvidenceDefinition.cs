// <auto-generated>
// (C) 2018 CHC  License: TBC
// </auto-generated>

namespace CHC.Consent.Api.Client.Models
{
    using Newtonsoft.Json;
    using System.Linq;

    public partial class EvidenceDefinition : IDefinition
    {
        /// <summary>
        /// Initializes a new instance of the EvidenceDefinition class.
        /// </summary>
        public EvidenceDefinition()
        {
            CustomInit();
        }

        /// <summary>
        /// Initializes a new instance of the EvidenceDefinition class.
        /// </summary>
        public EvidenceDefinition(string systemName = default(string), IIdentifierType type = default(IIdentifierType), string name = default(string))
            : base(systemName, type)
        {
            Name = name;
            CustomInit();
        }

        /// <summary>
        /// An initialization method that performs custom operations like setting defaults
        /// </summary>
        partial void CustomInit();

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "name")]
        public string Name { get; private set; }

    }
}