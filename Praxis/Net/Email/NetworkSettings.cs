namespace Praxis.Net.Email;

using System.Net.Mail;

public sealed class NetworkSettings {

	/// <summary>
	/// Gets or sets the directory to use when DeliveryMethod is SpecifiedPickupDirectory
	/// </summary>
	public string? DeliveryDirectory { get; set; }

	/// <summary>
	/// Gets or sets the method used for delivering email messages
	/// </summary>
	public SmtpDeliveryMethod DeliveryMethod { get; set; } = SmtpDeliveryMethod.Network;

	/// <summary>
	/// Gets or sets the domain to be used when sending email messages
	/// </summary>
	public required string Domain { get; set; }

	/// <summary>
	/// Gets or sets the Address placed in the from section of email messages
	/// </summary>
	public required string FromAddress { get; set; }

	/// <summary>
	/// Gets or sets the network address through which to send messages
	/// </summary>
	public required string HostAddress { get; set; }

	/// <summary>
	/// Gets or sets the password to use when accessing the SMTP server
	/// </summary>
	public string? Password { get; set; }

	/// <summary>
	/// Gets or sets the port through which mail is sent (Default is 25)
	/// </summary>
	public int Port { get; set; } = 25;

	/// <summary>
	/// Gets or sets the user name to use when accessing the SMTP server
	/// </summary>
	public required string UserName { get; set; }

	/// <summary>
	/// Gets or sets a value indicating whether or not to use SSL during transmission
	/// </summary>
	public bool UseSSL { get; set; }
}