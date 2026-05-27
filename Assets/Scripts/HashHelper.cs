using UnityEngine;

using System.Security.Cryptography;
using System.Text;

/// <summary>
/// Berekent SHA256 hash van een wachtwoord.
/// Puur educatief — toont hoe hashing werkt.
/// </summary>
public static class HashHelper
{
    public static string ComputeSHA256(string input)
    {
        using var sha256 = SHA256.Create();
        byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(input));
        
        var sb = new StringBuilder();
        foreach (byte b in bytes)
            sb.Append(b.ToString("x2"));
        
        return sb.ToString();
    }
}
