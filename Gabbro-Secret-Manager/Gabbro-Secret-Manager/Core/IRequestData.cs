using Microsoft.Extensions.Primitives;
using System.Collections;

namespace Gabbro_Secret_Manager.Core
{
    public interface IRequestData
    {
        IFormCollection Form { get; }
        IQueryCollection Query { get; }
        IRequestCookieCollection Cookies { get; }
    }
}
