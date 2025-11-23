using System.Security.Cryptography;

namespace ForariaDomain.Application.UseCase;
public interface IGeneratePassword
{
    Task<string> Generate();
}
public class GeneratePassword : IGeneratePassword
{

    private const string Upper = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
    private const string Lower = "abcdefghijklmnopqrstuvwxyz";
    private const string Digits = "0123456789";
    private const string Special = "!@#$%^&*()-_=+";

    public Task<string> Generate()
    {
        const int length = 10;
        string all = Upper + Lower + Digits + Special;

        var chars = new List<char>(length)
        {
            GetRandomChar(Upper),
            GetRandomChar(Lower),
            GetRandomChar(Digits),
            GetRandomChar(Special)
        };

        while (chars.Count < length)
            chars.Add(GetRandomChar(all));

        Shuffle(chars);

        var password = new string(chars.ToArray());
        return Task.FromResult(password);
    }

    private static char GetRandomChar(string set)
    {
        int idx = RandomNumberGenerator.GetInt32(set.Length);
        return set[idx];
    }

    private static void Shuffle(IList<char> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = RandomNumberGenerator.GetInt32(i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }
}
