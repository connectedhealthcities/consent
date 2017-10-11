using System;
using System.Linq;
using System.Security.Claims;
using CHC.Consent.Identity.Core;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;

namespace CHC.Consent.WebApi.Features.Person
{
    public interface IPersonRepository
    {
        IQueryable<IPerson> GetPeople();
    }

    public interface IUserAccessor
    {
        ClaimsPrincipal GetUser();
    }

    public class HttpContextUserAccessor : IUserAccessor
    {
        private readonly IHttpContextAccessor contextAccessor;

        public HttpContextUserAccessor(IHttpContextAccessor contextAccessor)
        {
            this.contextAccessor = contextAccessor;
        }


        /// <inheritdoc />
        public ClaimsPrincipal GetUser()
        {
            return contextAccessor.HttpContext.User;
        }
    }
    
    public class PersonRepositoryWithSecurity : IPersonRepository
    {
        private readonly IPersonRepository inner;
        private readonly IUserAccessor userAccessor;
        
        

        /// <inheritdoc />
        public PersonRepositoryWithSecurity(IPersonRepository inner, IUserAccessor userAccessor)
        {
            this.inner = inner;
            this.userAccessor = userAccessor;
        }


        /// <inheritdoc />
        public IQueryable<IPerson> GetPeople()
        {
            var user = this.userAccessor.GetUser();

            var people = inner.GetPeople();
            if (user.IsInRole("study_administrator"))
            {
                return people;
            }
            
            throw new NotImplementedException("Need to decide how to implement security for non-administrators");
        }
    }
}