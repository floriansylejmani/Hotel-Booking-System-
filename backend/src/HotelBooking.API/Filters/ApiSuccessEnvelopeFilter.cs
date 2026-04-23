using HotelBooking.API.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace HotelBooking.API.Filters;

public sealed class ApiSuccessEnvelopeFilter : IAsyncResultFilter
{
    public async Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
    {
        if (context.Result is ObjectResult objectResult &&
            objectResult.StatusCode is >= 200 and < 300 &&
            objectResult.Value is not null &&
            objectResult.Value is not ApiResponse &&
            objectResult.Value is not ProblemDetails)
        {
            objectResult.Value = ApiResponseFactory.Success(
                context.HttpContext,
                objectResult.Value);
        }

        await next();
    }
}
