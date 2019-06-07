using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CHC.Consent.Api.Infrastructure;
using CHC.Consent.Common.Consent;
using CHC.Consent.Common.Identity;
using CHC.Consent.Common.Identity.Identifiers;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Newtonsoft.Json;

namespace CHC.Consent.Api.Features.Identity.Dto
{
    public abstract class MatchSpecification
    {
        public abstract IPersonSpecification ToPersonSpecification(MatchSpecificationConversionContext context);
        public abstract void Validate(MatchSpecificationValidationContext context);
    }

    public class MatchSpecificationConversionContext
    {
        public PersonIdentifiersDtosIdentifierDtoMarshaller IdentifierDtoMarshaller { get; }

        /// <inheritdoc />
        public MatchSpecificationConversionContext(PersonIdentifiersDtosIdentifierDtoMarshaller identifierDtoMarshaller)
        {
            IdentifierDtoMarshaller = identifierDtoMarshaller;
        }
    }

    public class MatchSpecificationValidationContext
    {
        public IIdentityRepository Repository { get; }
        public IdentifierDefinitionRegistry Registry { get; }
        
        private ModelStateDictionary modelState { get; }
        private string modelStateKey { get; }

        /// <inheritdoc />
        public MatchSpecificationValidationContext(IIdentityRepository repository, IdentifierDefinitionRegistry registry, ModelStateDictionary modelState, string modelStateKey)
        {
            Repository = repository;
            Registry = registry;
            this.modelState = modelState;
            this.modelStateKey = modelStateKey;
        }

        public void AddError(string message)
        {
            modelState.TryAddModelError(modelStateKey, message);
        }
    }

    public class IdentifierMatchSpecification : MatchSpecification
    {
        public IIdentifierValueDto[] Identifiers { get; set; } = Array.Empty<IIdentifierValueDto>();

        /// <inheritdoc />
        public override IPersonSpecification ToPersonSpecification(MatchSpecificationConversionContext context)
        {
            return new CompositePersonSpecification(
                context.IdentifierDtoMarshaller.ConvertToIdentifiers(Identifiers)
                    .Select(_ => new PersonIdentifierSpecification(_))
            );
        }

        /// <inheritdoc />
        public override void Validate(MatchSpecificationValidationContext context)
        {
            foreach (var valueDto in Identifiers.Where(_ => !context.Registry.IsValidIdentifierType(_)))
            {
                context.AddError($"'{valueDto.SystemName}' is not a valid identifier name");
            }
        }
    }

    public class PersonAgencyIdMatchSpecification : MatchSpecification
    {
        public string Agency { get; set; }
        public string PersonAgencyId { get; set; }

        /// <inheritdoc />
        public override IPersonSpecification ToPersonSpecification(MatchSpecificationConversionContext context)
        {
            return new AgencyIdentifierPersonSpecification(Agency, PersonAgencyId);
        }

        /// <inheritdoc />
        public override void Validate(MatchSpecificationValidationContext context)
        {
            if (context.Repository.GetAgency(Agency) == null)
            {
                context.AddError($"'{Agency}' is not a valid Agency");
            }
        }
    }

    public class ConsentedForStudyMatchSpecification : MatchSpecification
    {
        public long StudyId { get; set; }

        /// <inheritdoc />
        public override IPersonSpecification ToPersonSpecification(MatchSpecificationConversionContext context)
        {
           return new ConsentedPersonSpecification(new StudyIdentity(StudyId)); 
        }

        /// <inheritdoc />
        public override void Validate(MatchSpecificationValidationContext context)
        {
            if(context.Repository.GetStudy(new StudyIdentity(StudyId)) == null)
            {
                context.AddError($"{StudyId} is not a valid study");
            }
        }
    }

    public class CompositeMatchSpecification : MatchSpecification
    {
        public IEnumerable<MatchSpecification> Specifications { get; set; }

        /// <inheritdoc />
        public override IPersonSpecification ToPersonSpecification(MatchSpecificationConversionContext context)
        {
            return new CompositePersonSpecification(
                Specifications.Select(_ => _.ToPersonSpecification(context))
            );
        }

        /// <inheritdoc />
        public override void Validate(MatchSpecificationValidationContext context)
        {
            foreach (var specification in Specifications)
            {
                specification.Validate(context);
            }
        }
    }
}