using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

public class EncryptionUtility
{
    private static readonly string encryptionKey = "Hsuaoi3j4o8JNs8dOkLaisj8YfjQmAH9";

    public static string Encrypt(string plainText)
    {
        byte[] key = Encoding.UTF8.GetBytes(encryptionKey);
        using (Aes aesAlg = Aes.Create())
        {
            aesAlg.Key = key;
            aesAlg.GenerateIV();
            ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

            using (MemoryStream msEncrypt = new MemoryStream())
            {
                msEncrypt.Write(aesAlg.IV, 0, aesAlg.IV.Length); // IVを先頭に書き込む
                using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                {
                    using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                    {
                        swEncrypt.Write(plainText);
                    }
                }
                return Convert.ToBase64String(msEncrypt.ToArray());
            }
        }
    }

    public static string Decrypt(string cipherText)
    {
        byte[] fullCipher = Convert.FromBase64String(cipherText);
        byte[] key = Encoding.UTF8.GetBytes(encryptionKey);

        using (Aes aesAlg = Aes.Create())
        {
            byte[] iv = new byte[16];
            Array.Copy(fullCipher, 0, iv, 0, iv.Length);
            aesAlg.Key = key;
            aesAlg.IV = iv;

            ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

            using (MemoryStream msDecrypt = new MemoryStream(fullCipher, iv.Length, fullCipher.Length - iv.Length))
            {
                using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                {
                    using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                    {
                        return srDecrypt.ReadToEnd();
                    }
                }
            }
        }
    }
}
