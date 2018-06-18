using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using CHC.Consent.Api.Infrastructure.IdentifierDisplay;
using CHC.Consent.Common;
using CHC.Consent.Common.Identity;
using CHC.Consent.Common.Infrastructure;
using CHC.Consent.Common.Infrastructure.Data;
using CHC.Consent.EFCore;
using CHC.Consent.EFCore.Consent;
using CHC.Consent.EFCore.Entities;
using CHC.Consent.EFCore.Security;
using IdentityModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;


namespace CHC.Consent.Api.Pages
{
    public class StudiesModel : PageModel
    {
        private readonly IUserProvider user;
        private ConsentContext Consent { get; }
        public IIdentifierHandlerProvider IdentifierHandlerProvider { get; }
        public IOptions<IdentifierDisplayOptions> DisplayOptions { get; }
        public IStoreProvider Stores { get; }

        public List<PersonEntity> People { get; private set; }

        public Dictionary<PersonIdentity, List<IPersonIdentifier>> Identifiers { get; } =
            new Dictionary<PersonIdentity, List<IPersonIdentifier>>();
        public StudyEntity Study { get; private set; }

        /// <inheritdoc />
        public StudiesModel(
            ConsentContext consent, 
            IUserProvider user, 
            IIdentifierHandlerProvider identifierHandlerProvider,
            IOptions<IdentifierDisplayOptions> displayOptions,
            IStoreProvider stores
            )
        {
            this.user = user;
            Consent = consent;
            IdentifierHandlerProvider = identifierHandlerProvider;
            DisplayOptions = displayOptions;
            Stores = stores;
        }

        public ActionResult OnGet(long id)
        {
            var roles = user.Roles.ToArray();
            var userName = user.UserName;
            var studiesForUser = Consent.Studies
                .Where(
                    s => s.ACL.Entries.Any(
                        acl => acl.Permission.Access == "Read" && (
                                   ((UserSecurityPrincipal) acl.Prinicipal).User.UserName == userName
                                   || roles.Contains(((RoleSecurityPrincipal) acl.Prinicipal).Role.Name)))
                );
            Study = studiesForUser.SingleOrDefault(_ => _.Id == id);

            if (Study == null) return NotFound();

            People = Consent.People
                .Where(
                    p => p.ACL.Entries.Any(
                        acl => acl.Permission.Access == "Read" && (
                                   ((UserSecurityPrincipal) acl.Prinicipal).User.UserName == userName
                                   || roles.Contains(((RoleSecurityPrincipal) acl.Prinicipal).Role.Name)))
                )
                .Distinct()
                .ToList();

            foreach (var personEntity in People)
            {
                var personIdentifiers = new List<IPersonIdentifier>();
                foreach (var identifierType in DisplayOptions.Value.Default??Enumerable.Empty<Type>())
                {
                    var handler = IdentifierHandlerProvider.GetPersistanceHandler(identifierType);
                    personIdentifiers.AddRange(handler.Get(personEntity, Stores));
                }

                Identifiers[personEntity] = personIdentifiers;
            }
            
            

            return Page();
        }
    }
}