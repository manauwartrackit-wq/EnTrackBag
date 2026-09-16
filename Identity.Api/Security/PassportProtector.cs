using System.Security.Cryptography;
using System.Text;

namespace Identity.Api.Security;

public class PassportProtector : IPassportProtector
{
    private const byte FormatVersion = 1;
    private static readonly byte[] AssociatedData = Encoding.UTF8.GetBytes("EnTrackBag.Passport.v1");
    private readonly byte[] _key;

    public PassportProtector(IConfiguration configuration)
    {
        var configuredSecret = configuration["Security:EncryptionKey"];
        if (string.IsNullOrWhiteSpace(configuredSecret))
            throw new InvalidOperationException("Security:EncryptionKey is required for passport protection.");

        _key = SHA256.HashData(Encoding.UTF8.GetBytes($"EnTrackBag.Passport.v1\n{configuredSecret}"));
    }

    public byte[] Protect(string passportNumber)
    {
        var clearText = Encoding.UTF8.GetBytes(passportNumber);
        var nonce = RandomNumberGenerator.GetBytes(12);
        var tag = new byte[16];
        var cipherText = new byte[clearText.Length];
        try
        {
            using var aes = new AesGcm(_key, tag.Length);
            aes.Encrypt(nonce, clearText, cipherText, tag, AssociatedData);
            var payload = new byte[1 + nonce.Length + tag.Length + cipherText.Length];
            payload[0] = FormatVersion;
            nonce.CopyTo(payload, 1);
            tag.CopyTo(payload, 13);
            cipherText.CopyTo(payload, 29);
            return payload;
        }
        finally
        {
            CryptographicOperations.ZeroMemory(clearText);
        }
    }

    public string Unprotect(byte[] payload)
    {
        if (payload.Length < 30 || payload[0] != FormatVersion)
            throw new CryptographicException("The encrypted passport value has an unsupported format.");

        var clearText = new byte[payload.Length - 29];
        try
        {
            using var aes = new AesGcm(_key, 16);
            aes.Decrypt(payload.AsSpan(1, 12), payload.AsSpan(29), payload.AsSpan(13, 16), clearText, AssociatedData);
            return Encoding.UTF8.GetString(clearText);
        }
        finally
        {
            CryptographicOperations.ZeroMemory(clearText);
        }
    }
}
