using System.Linq;
using CHC.Consent.Identity.Core;

namespace CHC.Consent.WebApi.Abstractions
{
    public interface IPersonRepository
    {
        IQueryable<IPerson> GetPeople();
    }
}