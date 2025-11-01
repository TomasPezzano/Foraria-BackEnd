namespace ForariaDomain.Repository;

public interface IPasswordResetTokenRepository
{
    Task Add(PasswordResetToken token);
    Task<PasswordResetToken?> GetByToken(string token);
    Task Update(PasswordResetToken token);
    Task<List<PasswordResetToken>> GetActiveTokensByUserId(int userId);
}
