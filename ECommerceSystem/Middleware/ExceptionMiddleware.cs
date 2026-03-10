namespace ECommerceSystem.Middleware
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;

        public ExceptionMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext httpContext)
        {
            try
            {
                await _next(httpContext);
            }
            catch (Exception ex)
            {
              httpContext.Response.StatusCode = 500;
              httpContext.Response.ContentType = "application/json";
              
            var response = new
            {
                success= false,
                message = ex.Message
            };


            await httpContext.Response.WriteAsJsonAsync(response);
            }
        }

    }
}
