using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;

namespace HookSentry.Worker.Infrastructure.Security;

public sealed class AesCredentialDecryptionService
{
    private readonly byte[] _key;

    public AesCredentialDecryptionService(IOptions<CredentialEncryptionSettings> options)
    {
        _key = Convert.FromBase64String(options.Value.Key);

        if (_key.Length != 32)
            throw new InvalidOperationException(
                "CredentialEncryption:Key deve ser uma chave AES-256 de exatamente 32 bytes (base64).");
    }

    public string Decrypt(string encryptedBase64)
    {
        var data = Convert.FromBase64String(encryptedBase64);

        var nonce = data[..12];
        var tag = data[12..28];
        var ciphertext = data[28..];
        var plaintext = new byte[ciphertext.Length];

        using var aes = new AesGcm(_key, AesGcm.TagByteSizes.MaxSize);
        aes.Decrypt(nonce, ciphertext, tag, plaintext);

        return Encoding.UTF8.GetString(plaintext);
    }
}
