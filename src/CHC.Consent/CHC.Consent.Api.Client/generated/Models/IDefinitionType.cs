// <auto-generated>
// (C) 2018 CHC  License: TBC
// </auto-generated>

namespace CHC.Consent.Api.Client.Models
{
    using Newtonsoft.Json;
    using System.Linq;

    public partial class IDefinitionType
    {
        /// <summary>
        /// Initializes a new instance of the IDefinitionType class.
        /// </summary>
        public IDefinitionType()
        {
            CustomInit();
        }

        /// <summary>
        /// Initializes a new instance of the IDefinitionType class.
        /// </summary>
        public IDefinitionType(string systemName = default(string))
        {
            SystemName = systemName;
            CustomInit();
        }

        /// <summary>
        /// An initialization method that performs custom operations like setting defaults
        /// </summary>
        partial void CustomInit();

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "systemName")]
        public string SystemName { get; private set; }

    }
}