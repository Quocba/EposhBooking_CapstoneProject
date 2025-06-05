namespace GraduationAPI_EPOSHBOOKING.MiddleWare
{
    public class MiddleWareAPI
    {
        private readonly RequestDelegate requestDelegate;
        private const String APIKeyHeader = "X-Api-key";
        public MiddleWareAPI(RequestDelegate _requestDelegate)
        {
            this.requestDelegate = _requestDelegate;
        }
        public async Task InvokeAsync(HttpContext context, IConfiguration configuration)
        {
            if (!context.Request.Headers.TryGetValue(APIKeyHeader, out var extractedApikey))
            {
                context.Response.StatusCode = 401;
                await context.Response.WriteAsync("API Key Incorrect");
                return;
            }
            var apikey = configuration["APISetting:SecretKey"];
            if (!apikey.Equals(extractedApikey))
            {
                context.Response.StatusCode = 403;
                await context.Response.WriteAsync("Unauthorize client");
                return;
            }
            await requestDelegate(context);
        }
    }
}
