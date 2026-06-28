using System.Security.Cryptography;
using System.Text;

namespace FYPSampleApplication;

public class HashUtils
{
    public static string ComputeSha256Hash(byte[] data)
    {
        using (var sha256Hash = SHA256.Create())
        {
            byte[] bytes = sha256Hash.ComputeHash(data);

            StringBuilder stringBuilder = new (bytes.Length * 2);
            foreach (byte b in bytes)
            {
                stringBuilder.Append(b.ToString("x2"));
            }
            
            return stringBuilder.ToString();
        }
    }
}