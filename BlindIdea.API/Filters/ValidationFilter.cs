using BlindIdea.Application.Dtos.Common;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace BlindIdea.API.Filters;

public class ValidationFilter<T> : IAsyncActionFilter where T : class
{
    private readonly IValidator<T> _validator;

    public ValidationFilter(IValidator<T> validator)
    {
        _validator = validator;
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        if (context.ActionArguments.Values.OfType<T>().FirstOrDefault() is not T request)
        {
            await next();
            return;
        }

        var result = await _validator.ValidateAsync(request);
        if (result.IsValid)
        {
            await next();
            return;
        }

        var errors = result.Errors.GroupBy(e => e.PropertyName)
            .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToList());

        context.Result = new BadRequestObjectResult(ApiResponse<object>.FailureResponse(
            "Validation failed",
            new ErrorDetails { FieldErrors = errors }));
    }
}
