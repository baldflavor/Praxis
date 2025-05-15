namespace Praxis.Net.Email;

using System.Text;

/// <summary>
/// Container used for holding the recipients for sending mail to
/// </summary>
public sealed class Recipients {

	/// <summary>
	/// Gets or sets a list of MailAddress to BCC.
	/// </summary>
	public IEnumerable<Recipient>? BCCAddresses { get; set; }

	/// <summary>
	/// Gets or sets a list of MailAddress to CC.
	/// </summary>
	public IEnumerable<Recipient>? CCAddresses { get; set; }

	/// <summary>
	/// Gets or sets a list of MailAddress to Send to.
	/// </summary>
	public IEnumerable<Recipient>? ToAddresses { get; set; }

	/// <summary>
	/// Initializes a new instance of the <see cref="Recipients" /> class
	/// </summary>
	public Recipients() {
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="Recipients" /> class
	/// </summary>
	/// <param name="toAddresses">Values that are set to the <see cref="ToAddresses"/> property</param>
	public Recipients(params Recipient[] toAddresses) {
		if (toAddresses?.Length > 0)
			this.ToAddresses = toAddresses;
	}

	/// <summary>
	/// Returns the string representation of this class
	/// </summary>
	/// <returns>A string containing pertinent information of this class as a string</returns>
	public override string ToString() {
		StringBuilder sb = new();

		append("To:", this.ToAddresses!);
		append("CC:", this.CCAddresses!);
		append("BCC:", this.BCCAddresses!);

		return sb.ToString();

		void append(string prefix, IEnumerable<Recipient> list) {
			if (list?.Any() == true) {
				sb.AppendLine(prefix);
				sb.AppendLine(string.Join(",", list));
			}
		}
	}


	public sealed class Recipient {

		/// <summary>
		/// Gets or sets the full email address of a recipient
		/// </summary>
		public required string Address { get; set; }

		/// <summary>
		/// Gets or sets the (optional) display name to use with the corresponding recipient
		/// </summary>
		public string? Name { get; set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="Recipient" /> class
		/// </summary>
		public Recipient() {
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="Recipient" /> class
		/// </summary>
		public Recipient(string address) {
			this.Address = address;
		}

		public override string ToString() {
			if (string.IsNullOrWhiteSpace(this.Name))
				return this.Address;
			else
				return $"{this.Name} <{this.Address}>";
		}
	}
}
