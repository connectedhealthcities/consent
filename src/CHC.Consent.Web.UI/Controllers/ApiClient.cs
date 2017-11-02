using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;

namespace CHC.Consent.Web.UI.Controllers
{
    public partial class HomeController
    {
        public class ApiClient
        {
            private readonly IHttpContextAccessor httpContextAccessor;


            /// <inheritdoc />
            public ApiClient(IHttpContextAccessor httpContextAccessor)
            {
                this.httpContextAccessor = httpContextAccessor;
            }

            public async Task<HttpClient> GetClientAsync()
            {
                var client = new HttpClient {BaseAddress = new Uri("http://localhost:49410/v0.1-dev/")};
                
                client.DefaultRequestHeaders.Authorization= new AuthenticationHeaderValue("bearer", await httpContextAccessor.HttpContext.GetTokenAsync("id_token"));

                return client;
            }

            private async Task<T> Call<T>(Func<HttpClient, Task<T>> call)
            {
                return await call(await GetClientAsync());

            }

            public async Task<string> GetStringAsync(string uri)
            {
                return await Call(_ => _.GetStringAsync(uri));
            }
        }
    }
}