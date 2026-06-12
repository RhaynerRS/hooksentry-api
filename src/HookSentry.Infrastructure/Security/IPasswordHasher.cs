namespace HookSentry.Infrastructure.Security;

public interface IPasswordHasher
{
    string Hash(string plainTextPassword);
    bool Verify(string plainTextPassword, string storedHash);
}
