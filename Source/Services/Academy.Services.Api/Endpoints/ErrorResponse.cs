namespace Academy.Services.Api.Endpoints
{
    public record ErrorResponse(
        int StatusCode,
        string Error,
        string Message,
        string? Details = null,
        string? TraceId = null
    );
}
