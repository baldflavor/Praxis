namespace Praxis.Crypto;

using System.Security.Cryptography;
using System.Text;

/// <summary>
/// Helper class used for obtaining hashes
/// </summary>
public class Hash {

	/// <summary>
	/// Gets an MD5 hash
	/// </summary>
	/// <param name="arg">string to hash</param>
	/// <returns>hash bytes</returns>
	public static byte[] GetMD5Hash(string arg) {
		return GetMD5Hash(Encoding.UTF8.GetBytes(arg));
	}

	/// <summary>
	/// Gets an MD5 hash
	/// </summary>
	/// <param name="arg">byte array to hash</param>
	/// <returns>hash bytes</returns>
	public static byte[] GetMD5Hash(byte[] arg) {
		return MD5.HashData(arg);
	}

	/// <summary>
	/// Gets an MD5 hash
	/// </summary>
	/// <param name="arg">Stream to hash</param>
	/// <returns>hash bytes</returns>
	public static byte[] GetMD5Hash(Stream arg) {
		using var provider = MD5.Create();
		return provider.ComputeHash(arg);
	}


	/// <summary>
	/// Returns an SHA256 hash
	/// </summary>
	/// <param name="arg">string to hash</param>
	/// <returns>hash bytes</returns>
	public static byte[] GetSHA256Hash(string arg) {
		return GetSHA256Hash(Encoding.UTF8.GetBytes(arg));
	}

	/// <summary>
	/// Returns an SHA256 hash
	/// </summary>
	/// <param name="arg">byte array to hash</param>
	/// <returns>hash bytes</returns>
	public static byte[] GetSHA256Hash(byte[] arg) {
		return SHA256.HashData(arg);
	}

	/// <summary>
	/// Returns an SHA256 hash
	/// </summary>
	/// <param name="arg">Stream to hash</param>
	/// <returns>hash bytes</returns>
	public static byte[] GetSHA256Hash(Stream arg) {
		using var provider = SHA256.Create();
		return provider.ComputeHash(arg);
	}


	/// <summary>
	/// Returns an SHA512 hash
	/// </summary>
	/// <param name="arg">string to hash</param>
	/// <returns>hash bytes</returns>
	public static byte[] GetSHA512Hash(string arg) {
		return GetSHA512Hash(Encoding.UTF8.GetBytes(arg));
	}

	/// <summary>
	/// Returns an SHA512 hash
	/// </summary>
	/// <param name="arg">byte array to hash</param>
	/// <returns>hash bytes</returns>
	public static byte[] GetSHA512Hash(byte[] arg) {
		return SHA512.HashData(arg);
	}

	/// <summary>
	/// Returns an SHA512 hash
	/// </summary>
	/// <param name="arg">Stream to hash</param>
	/// <returns>hash bytes</returns>
	public static byte[] GetSHA512Hash(Stream arg) {
		using var provider = SHA512.Create();
		return provider.ComputeHash(arg);
	}

	/// <summary>
	/// Gets an Rfc2898 hash using passed arguments.
	/// </summary>
	/// <param name="value">Value to hash.</param>
	/// <param name="saltSize">Size of salt.</param>
	/// <param name="iterations">Number of iterations.</param>
	/// <param name="keySize">Size of key.</param>
	/// <returns><c>string</c></returns>
	public static string Rfc2898(string value, int saltSize = 50, int iterations = 300, int keySize = 60) {
		using Rfc2898DeriveBytes cAlg = new(value, saltSize, iterations, HashAlgorithmName.SHA512);

		string key = Convert.ToBase64String(cAlg.GetBytes(keySize));
		string salt = Convert.ToBase64String(cAlg.Salt);

		return $"{salt}¦{iterations}¦{key}";
	}

	/// <summary>
	/// Gets whether two hashes are equal for Rfc2898.
	/// </summary>
	/// <param name="value">Value to check against the passed <paramref name="hash"/>.</param>
	/// <param name="hash">Hash to check against <paramref name="value"/>.</param>
	/// <param name="keySize">Size of the key.</param>
	/// <returns><c>true</c> if equal, otherwise <c>false</c>.</returns>
	public static bool Rfc2898IsEqual(string value, string hash, int keySize = 60) {
		string[] hashSections = hash.Split('¦', 3);
		if (hashSections.Length != 3)
			return false;

		int iterations = int.Parse(hashSections[1]);
		byte[] salt = Convert.FromBase64String(hashSections[0]);

		using Rfc2898DeriveBytes cAlg = new(value, salt, iterations, HashAlgorithmName.SHA512);

		return cAlg.GetBytes(keySize).SequenceEqual(Convert.FromBase64String(hashSections[2]));
	}
}
