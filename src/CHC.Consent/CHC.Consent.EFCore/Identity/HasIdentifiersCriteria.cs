using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using CHC.Consent.Common.Identity;
using CHC.Consent.EFCore.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Net.Http.Headers;

namespace CHC.Consent.EFCore.Identity
{
    public class HasIdentifiersCriteria : ICriteria<PersonEntity>
    {
        private readonly IdentifierSearch[] criteria;

        public HasIdentifiersCriteria(params IdentifierSearch[] search) : this(search.AsEnumerable())
        {
            
        }
            
        public HasIdentifiersCriteria(IEnumerable<IdentifierSearch> criteria)
        {
            this.criteria = criteria.ToArray();
        }


        /// <inheritdoc />
        public IQueryable<PersonEntity> ApplyTo(IQueryable<PersonEntity> queryable, ConsentContext context)
        {
            if(criteria.Length == 0) return queryable;
            var query = new StringBuilder("SELECT * FROM dbo.Person WHERE ");
            var vars = new List<SqlParameter>();
            var counter = 0;
            var isFirstGroup = true;
            foreach (var grouping in criteria.GroupBy(_ => _.KeySelector()))
            {
                if (isFirstGroup) isFirstGroup = false;
                else query.Append(" AND ");
                query.AppendFormat(
                    "EXISTS (SELECT 1 FROM dbo.PersonIdentifier WHERE Person.Id = PersonIdentifier.PersonId AND TypeName = @var{0} AND ",
                    ++counter);
                vars.Add(new SqlParameter($"@var{counter}", grouping.Key));
                var isFirst = true;
                foreach (var identifierSearch in grouping)
                {
                    if (isFirst)
                    {
                        isFirst = false;
                    }
                    else
                    {
                        query.Append(" AND ");
                    }

                    switch (identifierSearch.Operator)
                    {
                        case IdentifierSearchOperator.Contains:
                            query.AppendFormat(
                                " cast([Value] as xml).exist('{0}/text()[ contains( lower-case(.), sql:variable(\"@var{1}\") ) ]') = 1 ",
                                identifierSearch.IdentifierName.Replace(IdentifierSearch.Separator, "/"),
                                ++counter
                            );
                            break;
                        case IdentifierSearchOperator.LessThanOrEqual:
                            query.AppendFormat(" cast([Value] as xml).value('.', 'date') <= @var{0} ", ++counter);
                            break;
                        case IdentifierSearchOperator.GreaterThanOrEqual:
                            query.AppendFormat(" cast([Value] as xml).value('.', 'date') >= @var{0} ", ++counter);
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }


                    vars.Add(new SqlParameter($"@var{counter}", identifierSearch.Value?.ToLowerInvariant()));

                }
                query.Append(") ");
            }

            return queryable.FromSql(query.ToString(), vars.Cast<object>().ToArray());
        }
    }
}