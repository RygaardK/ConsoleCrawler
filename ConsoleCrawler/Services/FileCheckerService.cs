using System.Collections;
using System.Security.Cryptography;

namespace Service;


public class FileCheckerService
{
    public Task<bool> AreFilesEqual(Stream stream, string filePath)
    {
        byte[] hash1 = CalculateFileHash(filePath);
        byte[] hash2 = CalculateFileHash(stream);

        return Task.FromResult(StructuralComparisons.StructuralEqualityComparer.Equals(hash1, hash2));
    }

    public Task<bool> AreFilesEqual(string filePath1, string filePath2)
    {
        byte[] hash1 = CalculateFileHash(filePath1);
        byte[] hash2 = CalculateFileHash(filePath2);

        return Task.FromResult(StructuralComparisons.StructuralEqualityComparer.Equals(hash1, hash2));
    }

    #region Private
    private byte[] CalculateFileHash(string filePath)
    {
        using (var sha256 = SHA256.Create())
        using (var stream = File.OpenRead(filePath))
        {
            return sha256.ComputeHash(stream);
        }
    }

    private byte[] CalculateFileHash(Stream stream)
    {
        using (var sha256 = SHA256.Create())
            return sha256.ComputeHash(stream);
    }
    #endregion
}
