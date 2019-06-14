using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace CHC.Consent.Api.Infrastructure.IdentifierDisplay
{
    public class IdentifierDisplayOptions
    {
        public List<string> Default { get; set; } = new List<string>();
        public List<SearchGroup> Search { get; set; } = new List<SearchGroup>();
        
        [UsedImplicitly]
        public class FieldAndLabel
        {
            public string Name { get; set; }
            public string Label { get; set; }
        }

        [UsedImplicitly]
        public class SearchGroup
        {
            public List<SearchField> Fields { get; set; } = new List<SearchField>();
        }
        
        [UsedImplicitly]
        public class SearchField : FieldAndLabel
        {
            public string Compare { get; set; }
        }
            
    }
}