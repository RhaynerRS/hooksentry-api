namespace HookSentry.Infrastructure.Security;

public interface ICredentialEncryptionService
{
    string Encrypt(string plaintext);
    string Decrypt(string encryptedBase64);
}
