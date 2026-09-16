namespace Identity.Api.Security;

public interface IPassportProtector
{
    byte[] Protect(string passportNumber);
    string Unprotect(byte[] encryptedPassportNumber);
}
