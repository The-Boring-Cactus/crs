using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    public class UserMiddleware : IConcern
    {
        public IHandler Content { get; }

        public UserMiddleware(IHandler content)
        {
            Content = content;
        }

        public ValueTask PrepareAsync() => Content.PrepareAsync();

        public async ValueTask<IResponse?> HandleAsync(IRequest request)
        {
            var response = await Content.HandleAsync(request);

            if (response != null)
            {
                response.Headers.Add("X-Custom-Header", "Custom Concern");
            }

            return response;
        }
    }
}
