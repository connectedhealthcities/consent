using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace CHC.Consent.Api.Infrastructure.IdentifierDisplay
{
    public class IdentifierDisplayOptions
    {
        public List<string> Default { get; set; } = new List<string>();
        public List<FieldAndLabel> Search { get; set; } = new List<FieldAndLabel>();
        
        [UsedImplicitly]
        public class FieldAndLabel
        {
            public string Name { get; set; }
            public string Label { get; set; }
        }
    }
}