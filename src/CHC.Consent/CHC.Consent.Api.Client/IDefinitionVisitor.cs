using System.Collections.Generic;

namespace CHC.Consent.Api.Client.Models
{
    public interface IAcceptDefinitionVisitor<out TDefinition> where TDefinition : IDefinition
    {
        void Accept(IDefinitionVisitor<TDefinition> visitor);
    }

    public interface IDefinitionVisitor<in TDefinition> where TDefinition: IDefinition
    {
        void Visit(TDefinition definition, CompositeIdentifierType type);
        void Visit(TDefinition definition, DateIdentifierType type);
        void Visit(TDefinition definition, EnumIdentifierType type);
        void Visit(TDefinition definition, IntegerIdentifierType type);
        void Visit(TDefinition definition, StringIdentifierType type);
    }

    public partial class IDefinition 
    {
    }


    public partial class IdentifierDefinition : IAcceptDefinitionVisitor<IdentifierDefinition>
    {
        /// <inheritdoc />
        public void Accept(IDefinitionVisitor<IdentifierDefinition> visitor)
        {
            Type.Accept(visitor, this);
        }
    }
    
    public partial class EvidenceDefinition : IAcceptDefinitionVisitor<EvidenceDefinition>
    {
        /// <inheritdoc />
        public void Accept(IDefinitionVisitor<EvidenceDefinition> visitor)
        {
            Type.Accept(visitor, this);
        }
    }

    public interface IIdentifierTypeVisitorAcceptor
    {
        void Accept<TDefinition>(IDefinitionVisitor<TDefinition> visitor, TDefinition definition) where TDefinition : IDefinition;
    }
    
    public partial class IIdentifierType : IIdentifierTypeVisitorAcceptor
    {
        /// <inheritdoc />
        public virtual void
            Accept<TDefinition>(IDefinitionVisitor<TDefinition> visitor, TDefinition definition)
            where TDefinition : IDefinition =>
            throw new System.NotImplementedException("Sub types should implement this");
    }

    public partial class CompositeIdentifierType
    {
        /// <inheritdoc />
        public override void Accept<TDefinition>(IDefinitionVisitor<TDefinition> visitor, TDefinition definition)
        {
            visitor.Visit(definition, this);
        }
    }

    public partial class DateIdentifierType
    {
        /// <inheritdoc />
        public override void Accept<TDefinition>(IDefinitionVisitor<TDefinition> visitor, TDefinition definition)
        {
            visitor.Visit(definition, this);
        }
    }

    public partial class EnumIdentifierType
    {
        /// <inheritdoc />
        public override void Accept<TDefinition>(IDefinitionVisitor<TDefinition> visitor, TDefinition definition)
        {
            visitor.Visit(definition, this);
        }
    }

    public partial class IntegerIdentifierType
    {
        /// <inheritdoc />
        public override void Accept<TDefinition>(IDefinitionVisitor<TDefinition> visitor, TDefinition definition)
        {
            visitor.Visit(definition, this);
        }
    }

    public partial class StringIdentifierType
    {
        /// <inheritdoc />
        public override void Accept<TDefinition>(IDefinitionVisitor<TDefinition> visitor, TDefinition definition)
        {
            visitor.Visit(definition, this);
        }
    }


    public static class VisitorExtensions
    {
        public static TVisitor VisitAll<TVisitor, TDefinition>(
            this TVisitor visitor, IEnumerable<TDefinition> definitions)
            where TDefinition : IDefinition, IAcceptDefinitionVisitor<TDefinition>
            where TVisitor : IDefinitionVisitor<TDefinition>
        {
            foreach (var definition in definitions)
            {
                definition.Accept(visitor);
            }

            return visitor;
        }
    }
}