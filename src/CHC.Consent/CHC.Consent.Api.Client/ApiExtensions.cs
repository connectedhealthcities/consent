// <auto-generated>
// Code generated by Microsoft (R) AutoRest Code Generator.
// Changes may cause incorrect behavior and will be lost if the code is
// regenerated.
// </auto-generated>

namespace CHC.Consent.Api.Client
{
    using Models;
    using System.Collections;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Extension methods for Api.
    /// </summary>
    public static partial class ApiExtensions
    {
            /// <param name='operations'>
            /// The operations group for this extension method.
            /// </param>
            /// <param name='specification'>
            /// </param>
            public static void ConsentPut(this IApi operations, ConsentSpecification specification = default(ConsentSpecification))
            {
                operations.ConsentPutAsync(specification).GetAwaiter().GetResult();
            }

            /// <param name='operations'>
            /// The operations group for this extension method.
            /// </param>
            /// <param name='specification'>
            /// </param>
            /// <param name='cancellationToken'>
            /// The cancellation token.
            /// </param>
            public static async Task ConsentPutAsync(this IApi operations, ConsentSpecification specification = default(ConsentSpecification), CancellationToken cancellationToken = default(CancellationToken))
            {
                (await operations.ConsentPutWithHttpMessagesAsync(specification, null, cancellationToken).ConfigureAwait(false)).Dispose();
            }

            /// <param name='operations'>
            /// The operations group for this extension method.
            /// </param>
            /// <param name='id'>
            /// </param>
            public static void ConsentByIdGet(this IApi operations, long id)
            {
                operations.ConsentByIdGetAsync(id).GetAwaiter().GetResult();
            }

            /// <param name='operations'>
            /// The operations group for this extension method.
            /// </param>
            /// <param name='id'>
            /// </param>
            /// <param name='cancellationToken'>
            /// The cancellation token.
            /// </param>
            public static async Task ConsentByIdGetAsync(this IApi operations, long id, CancellationToken cancellationToken = default(CancellationToken))
            {
                (await operations.ConsentByIdGetWithHttpMessagesAsync(id, null, cancellationToken).ConfigureAwait(false)).Dispose();
            }

            /// <param name='operations'>
            /// The operations group for this extension method.
            /// </param>
            /// <param name='id'>
            /// </param>
            public static IList<IIdentifier> IdentitiesByIdGet(this IApi operations, long id)
            {
                return operations.IdentitiesByIdGetAsync(id).GetAwaiter().GetResult();
            }

            /// <param name='operations'>
            /// The operations group for this extension method.
            /// </param>
            /// <param name='id'>
            /// </param>
            /// <param name='cancellationToken'>
            /// The cancellation token.
            /// </param>
            public static async Task<IList<IIdentifier>> IdentitiesByIdGetAsync(this IApi operations, long id, CancellationToken cancellationToken = default(CancellationToken))
            {
                using (var _result = await operations.IdentitiesByIdGetWithHttpMessagesAsync(id, null, cancellationToken).ConfigureAwait(false))
                {
                    return _result.Body;
                }
            }

            /// <param name='operations'>
            /// The operations group for this extension method.
            /// </param>
            /// <param name='specification'>
            /// </param>
            public static long? IdentitiesPut(this IApi operations, PersonSpecification specification = default(PersonSpecification))
            {
                return operations.IdentitiesPutAsync(specification).GetAwaiter().GetResult();
            }

            /// <param name='operations'>
            /// The operations group for this extension method.
            /// </param>
            /// <param name='specification'>
            /// </param>
            /// <param name='cancellationToken'>
            /// The cancellation token.
            /// </param>
            public static async Task<long?> IdentitiesPutAsync(this IApi operations, PersonSpecification specification = default(PersonSpecification), CancellationToken cancellationToken = default(CancellationToken))
            {
                using (var _result = await operations.IdentitiesPutWithHttpMessagesAsync(specification, null, cancellationToken).ConfigureAwait(false))
                {
                    return _result.Body;
                }
            }

    }
}