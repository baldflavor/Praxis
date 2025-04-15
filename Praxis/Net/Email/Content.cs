namespace Praxis.Net.Email;

/// <summary>
/// Small container used for holding the content sections of an email message
/// </summary>
public sealed class Content {

	/// <summary>
	/// Gets or sets the Friendly From address to use (if any) when sending a message
	/// </summary>
	public required string FriendlyFrom { get; set; }

	/// <summary>
	/// Gets or sets the HTML body of the message - note if this if left empty or null then when sending
	/// a message it will be implied that the mail.IsBodyHtml should be FALSE
	/// </summary>
	public required string HTMLBody { get; set; }

	/// <summary>
	/// Gets or sets the plain text body of the message
	/// </summary>
	public required string PlainBody { get; set; }

	/// <summary>
	/// Gets or sets the subject of a message
	/// </summary>
	public required string Subject { get; set; }

	/// <summary>
	/// Returns the string representation of this class
	/// </summary>
	/// <returns>A string containing pertinent information of this class as a string</returns>
	public override string ToString() {
		return
$@"Subject:
{this.Subject}
Plain Body:
{this.PlainBody}
Html Body:
{this.HTMLBody}";
	}
}