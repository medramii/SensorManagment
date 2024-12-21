using System.Security.Cryptography;
using System.Text;

namespace ServerApp.API.Extensions;
public class StringExtentions
{
    /// <summary>
    /// Generate MD5 Hash string from a string input(ex:password)
    /// </summary>
    /// <param name="input">ex:password</param>
    /// <returns>ex:Hashed Password</returns>
    public static string GenerateMD5Hash(string input)
    {
        // step 1, calculate MD5 hash from input
        MD5 md5 = System.Security.Cryptography.MD5.Create();
        byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(input);
        byte[] hash = md5.ComputeHash(inputBytes);

        // step 2, convert byte array to hex string
        StringBuilder sb = new StringBuilder();
        for (int i = 0; i < hash.Length; i++)
        {
            sb.Append(hash[i].ToString("x2"));
        }
        return sb.ToString();
    }
}

