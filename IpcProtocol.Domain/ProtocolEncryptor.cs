using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;

namespace IpcProtocol.Domain
{
    public class ProtocolEncryptor : IProtocolEncryptor
    {
        public string Key => "75a901d3e0b319194ec3c2993653256d0f91ee46d2693d1db1c5f75ed9b70f18";
        public string IV => "da592eb6f5dc954b0a796cf3d0aae690";

        public string Encrypt(string plainText)
        {
            byte[] encrypted = null;
            string result = null;
            
            var key = StringToByteArray(Key);
            var iv = StringToByteArray(IV);

            using (var rijndaelManaged = new RijndaelManaged { Mode = CipherMode.CBC })
            {
                rijndaelManaged.BlockSize = 128;
                rijndaelManaged.KeySize = 256;
                rijndaelManaged.Key = key;
                rijndaelManaged.IV = iv;

                ICryptoTransform encryptor = rijndaelManaged.CreateEncryptor(rijndaelManaged.Key, rijndaelManaged.IV);

                using (MemoryStream msEncrypt = new MemoryStream())
                {
                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                        {
                            swEncrypt.Write(plainText);
                        }

                        encrypted = msEncrypt.ToArray();
                        result = Convert.ToBase64String(encrypted);
                    }
                }
            }

            return result;
        }

        public string Decrypt(string encryptedText)
        {
            var key = StringToByteArray(Key);
            var iv = StringToByteArray(IV);

            using (var rijndaelManaged = new RijndaelManaged { Mode = CipherMode.CBC })
            {
                rijndaelManaged.BlockSize = 128;
                rijndaelManaged.KeySize = 256;
                rijndaelManaged.Key = key;
                rijndaelManaged.IV = iv;

                using (var memoryStream = new MemoryStream(Convert.FromBase64String(encryptedText)))
                {
                    using (var cryptoStream = new CryptoStream(memoryStream, rijndaelManaged.CreateDecryptor(), CryptoStreamMode.Read))
                    {
                        using (var streamReader = new StreamReader(cryptoStream))
                        {
                            var result = streamReader.ReadToEnd();
                            return result;
                        }
                    }
                }
            }
        }

        public static byte[] StringToByteArray(string hex)
        {
            return Enumerable.Range(0, hex.Length)
                             .Where(x => x % 2 == 0)
                             .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                             .ToArray();
        }
    }
}
