namespace ForariaDomain.Models;

public class LoginResult
{
    public string AccessToken { get; private set; }
    public string RefreshToken { get; private set; }
    public bool RequiresPasswordChange { get; private set; }
    public User User { get; private set; }

    public int? ConsortiumId { get; private set; }

    private LoginResult() { }

    public static LoginResult SuccessResult(string accessToken, string refreshToken, User user, int? consortiumId)
    {
        return new LoginResult
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            RequiresPasswordChange = user.RequiresPasswordChange, 
            User = user,
            ConsortiumId = consortiumId
        };
    }
}