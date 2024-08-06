using Microsoft.AspNetCore.Mvc;

namespace api_csharp_uplink.DirException;

public class ErrorManager : ControllerBase
{
    public static IActionResult HandleError(Exception exception)
    {
        return exception switch
        {
            AlreadyCreateException => new ConflictObjectResult(exception.Message),
            NotFoundException => new NotFoundObjectResult(exception.Message),
            ValueNotCorrectException => new BadRequestObjectResult(exception.Message),
            ArgumentOutOfRangeException => new BadRequestObjectResult(exception.Message),
            ArgumentNullException => new BadRequestObjectResult(exception.Message),
            ArgumentException => new BadRequestObjectResult(exception.Message),
            DbException => new ObjectResult(new ProblemDetails
            {
                Detail = exception.Message, Status = 500, Title = "Error DB."
            }),
            _ => new ObjectResult(new ProblemDetails
            {
                Detail = exception.Message,
                Status = 500,
                Title = "An error occurred while processing your request."
            })
        };
    }
}
