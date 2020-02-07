namespace IpcProtocol.Domain
{
    public interface IProtocolEncryptor
    {
        string Encrypt(string plainText);
        string Decrypt(string encryptedText);
    }
}
