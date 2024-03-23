namespace Gabbro_Secret_Manager.Core
{
    public static class HttpRequestExtensions
    {
        public static IRequestData AsRequestData(this HttpRequest request)
        {
            return new TransientRequestData(
                () => request.Form,
                () => request.Query,
                () => request.Cookies);
        }
    }
}
