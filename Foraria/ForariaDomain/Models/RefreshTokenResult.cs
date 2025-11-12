namespace ForariaDomain.Models;

public class RefreshTokenResult
{
    public bool Success { get; private set; }
    public string Message { get; private set; }
    public string AccessToken { get; private set; }
    public string RefreshToken { get; private set; }

    private RefreshTokenResult() { }

    public static RefreshTokenResult SuccessResult(string accessToken, string refreshToken)
    {
        return new RefreshTokenResult
        {
            Success = true,
            Message = "Token refreshed successfully",
            AccessToken = accessToken,
            RefreshToken = refreshToken
        };
    }

    public static RefreshTokenResult FailureResult(string message)
    {
        return new RefreshTokenResult
        {
            Success = false,
            Message = message,
            AccessToken = string.Empty,
            RefreshToken = string.Empty
        };
    }
}