using System.Collections.Generic;

namespace CHC.Consent.Api.Client.Models
{
    public interface IAcceptDefinitionVisitor<out TDefinition> where TDefinition : IDefinition
    {
        void Accept(IDefinitionVisitor<TDefinition> visitor);
    }

    public interface IDefinitionVisitor<in TDefinition> where TDefinition: IDefinition
    {
        void Visit(TDefinition definition, CompositeDefinitionType type);
        void Visit(TDefinition definition, DateDefinitionType type);
        void Visit(TDefinition definition, EnumDefinitionType type);
        void Visit(TDefinition definition, IntegerDefinitionType type);
        void Visit(TDefinition definition, StringDefinitionType type);
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
/*

    public interface IDefinitionTypeVisitorAcceptor
    {
        void Accept<TDefinition>(IDefinitionVisitor<TDefinition> visitor, TDefinition definition) where TDefinition : IDefinition;
    }*/
    
    public partial class IDefinitionType /*: IDefinitionTypeVisitorAcceptor*/
    {
        /// <inheritdoc />
        public virtual void
            Accept<TDefinition>(IDefinitionVisitor<TDefinition> visitor, TDefinition definition)
            where TDefinition : IDefinition =>
            throw new System.NotImplementedException("Sub types should implement this");
    }

    public partial class CompositeDefinitionType
    {
        /// <inheritdoc />
        public override void Accept<TDefinition>(IDefinitionVisitor<TDefinition> visitor, TDefinition definition)
        {
            visitor.Visit(definition, this);
        }
    }

    public partial class DateDefinitionType
    {
        /// <inheritdoc />
        public override void Accept<TDefinition>(IDefinitionVisitor<TDefinition> visitor, TDefinition definition)
        {
            visitor.Visit(definition, this);
        }
    }

    public partial class EnumDefinitionType
    {
        /// <inheritdoc />
        public override void Accept<TDefinition>(IDefinitionVisitor<TDefinition> visitor, TDefinition definition)
        {
            visitor.Visit(definition, this);
        }
    }

    public partial class IntegerDefinitionType
    {
        /// <inheritdoc />
        public override void Accept<TDefinition>(IDefinitionVisitor<TDefinition> visitor, TDefinition definition)
        {
            visitor.Visit(definition, this);
        }
    }

    public partial class StringDefinitionType
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