namespace Praxis.Crypto;

using System.Security.Cryptography;
using System.Text;

/// <summary>
/// Class used for simple encryption / decryption of clear text
/// </summary>
public static class Aes {

	/// <summary>
	/// Used for random IV generation
	/// </summary>
	private static readonly Random _rand = new();


	/// <summary>
	/// Decrypts bytes into its original cleartext
	/// </summary>
	/// <param name="key">The key to use for decryption</param>
	/// <param name="data">The data to use for de-cryption</param>
	/// <returns>The original clear text</returns>
	public static string Decrypt(string key, byte[] data) {
		byte[] tempBytes = new byte[15];

		using var crypto = System.Security.Cryptography.Aes.Create();

		Array.Copy(data, tempBytes, 15);

		using Rfc2898DeriveBytes pdb = new(key, data.Take(15).ToArray(), 1000, HashAlgorithmName.SHA512);
		crypto.Key = pdb.GetBytes(32);
		crypto.IV = pdb.GetBytes(16);

		int tempLength = data.Length - 15;
		tempBytes = new byte[tempLength];
		Array.Copy(data, 15, tempBytes, 0, tempLength);

		using MemoryStream ms = new();
		using CryptoStream cs = new(ms, crypto.CreateDecryptor(), CryptoStreamMode.Write);
		cs.Write(tempBytes, 0, tempBytes.Length);
		cs.Close();

		return Encoding.Unicode.GetString(ms.ToArray());
	}

	/// <summary>
	/// Decrypts a Base64 string into its original cleartext
	/// </summary>
	/// <param name="key">The key to use for decryption</param>
	/// <param name="encrypted">An encrypted string</param>
	/// <returns>The original clear text</returns>
	public static string DecryptFromBase64(string key, string encrypted) {
		byte[] iv = Convert.FromBase64String(encrypted[..20]);
		encrypted = encrypted[20..].Replace(' ', '+');
		byte[] ebytes = Convert.FromBase64String(encrypted);

		using var crypto = System.Security.Cryptography.Aes.Create();
		using Rfc2898DeriveBytes pdb = new(key, iv, 1000, HashAlgorithmName.SHA512);
		crypto.Key = pdb.GetBytes(32);
		crypto.IV = pdb.GetBytes(16);

		using MemoryStream ms = new();
		using CryptoStream cs = new(ms, crypto.CreateDecryptor(), CryptoStreamMode.Write);
		cs.Write(ebytes, 0, ebytes.Length);
		cs.Close();

		return Encoding.Unicode.GetString(ms.ToArray());
	}

	/// <summary>
	/// Encrypts clear text to a byte array that includes both data and IV specifications
	/// </summary>
	/// <param name="key">The key to use for encryption</param>
	/// <param name="arg">String to encrypt</param>
	/// <returns>Encrypted byte arrays</returns>
	public static byte[] Encrypt(string key, string arg) {
		using var crypto = System.Security.Cryptography.Aes.Create();
		byte[] iv = new byte[15];
		_rand.NextBytes(iv);

		using Rfc2898DeriveBytes pdb = new(key, iv, 1000, HashAlgorithmName.SHA512);
		crypto.Key = pdb.GetBytes(32);
		crypto.IV = pdb.GetBytes(16);

		using MemoryStream ms = new();

		using CryptoStream cs = new(ms, crypto.CreateEncryptor(), CryptoStreamMode.Write);
		byte[] dbytes = Encoding.Unicode.GetBytes(arg);
		cs.Write(dbytes, 0, dbytes.Length);
		cs.Close();

		byte[] msBytes = ms.ToArray();
		byte[] toReturn = new byte[iv.Length + msBytes.Length];
		iv.CopyTo(toReturn, 0);
		msBytes.CopyTo(toReturn, iv.Length);

		return toReturn;
	}

	/// <summary>
	/// Encrypts clear text to a Base64 string that includes both data and IV specifications
	/// </summary>
	/// <param name="key">The key to use for encryption</param>
	/// <param name="arg">String to encrypt</param>
	/// <returns>An encrypted string</returns>
	public static string EncryptToBase64(string key, string arg) {
		using var crypto = System.Security.Cryptography.Aes.Create();
		byte[] iv = new byte[15];
		_rand.NextBytes(iv);

		using Rfc2898DeriveBytes pdb = new(key, iv, 1000, HashAlgorithmName.SHA512);
		crypto.Key = pdb.GetBytes(32);
		crypto.IV = pdb.GetBytes(16);

		using MemoryStream ms = new();
		using CryptoStream cs = new(ms, crypto.CreateEncryptor(), CryptoStreamMode.Write);
		byte[] dbytes = Encoding.Unicode.GetBytes(arg);
		cs.Write(dbytes, 0, dbytes.Length);
		cs.Close();

		return Convert.ToBase64String(iv) + Convert.ToBase64String(ms.ToArray());
	}
}