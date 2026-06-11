namespace HookSentry.Api.Common.Security;

public interface IPasswordHasher
{
    string Hash(string plainTextPassword);
    bool Verify(string plainTextPassword, string storedHash);
}
