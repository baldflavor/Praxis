namespace Praxis.Net.Email;

using System.Net.Mail;

/// <summary>
/// Static class for sending email.
/// </summary>
public static class Sender {

	/// <summary>
	/// Sends an email message.
	/// </summary>
	/// <param name="content">Content to send.</param>
	/// <param name="netSettings">Settings to use.</param>
	/// <param name="recipients">Message recipients.</param>
	/// <param name="disposeAttachments">Whether to dispose of attachments.</param>
	/// <param name="attachments">Attachments to add to the message.</param>
	/// <exception cref="Exception">Thrown if the message cannot be sent.</exception>
	public static void Send(Content content, NetworkSettings netSettings, Recipients recipients, bool disposeAttachments = true, params Attachment[] attachments) {
		try {
			using MailMessage mail = new();

			mail.From = new MailAddress(netSettings.FromAddress, content.FriendlyFrom);

			if (recipients.ToAddresses?.Any() == true) {
				foreach (Recipients.Recipient ma in recipients.ToAddresses)
					mail.To.Add(new MailAddress(ma.Address, ma.Name));
			}

			if (recipients.CCAddresses?.Any() == true) {
				foreach (Recipients.Recipient ma in recipients.CCAddresses)
					mail.CC.Add(new MailAddress(ma.Address, ma.Name));
			}

			if (recipients.BCCAddresses?.Any() == true) {
				foreach (Recipients.Recipient ma in recipients.BCCAddresses)
					mail.Bcc.Add(new MailAddress(ma.Address, ma.Name));
			}

			mail.BodyEncoding = System.Text.Encoding.UTF8;

			if (!string.IsNullOrWhiteSpace(content.HTMLBody))
				mail.IsBodyHtml = true;

			mail.Subject = content.Subject;

			if (!string.IsNullOrEmpty(content.PlainBody)) {
				var plainView = AlternateView.CreateAlternateViewFromString(content.PlainBody, null, "text/plain");
				mail.AlternateViews.Add(plainView);
			}

			if (!string.IsNullOrEmpty(content.HTMLBody)) {
				var htmlView = AlternateView.CreateAlternateViewFromString(content.HTMLBody, null, "text/html");
				mail.AlternateViews.Add(htmlView);
			}

			if (attachments != null) {
				foreach (Attachment att in attachments)
					mail.Attachments.Add(att);
			}

			using SmtpClient smtp = new(netSettings.HostAddress, netSettings.Port) {
				EnableSsl = netSettings.UseSSL,
				DeliveryMethod = netSettings.DeliveryMethod
			};

			if (netSettings.DeliveryMethod == SmtpDeliveryMethod.SpecifiedPickupDirectory)
				smtp.PickupDirectoryLocation = netSettings.DeliveryDirectory;

			if (!string.IsNullOrEmpty(netSettings.UserName) && !string.IsNullOrEmpty(netSettings.Password)) {
				if (string.IsNullOrEmpty(netSettings.Domain))
					smtp.Credentials = new System.Net.NetworkCredential(netSettings.UserName, netSettings.Password);
				else
					smtp.Credentials = new System.Net.NetworkCredential(netSettings.UserName, netSettings.Password, netSettings.Domain);
			}

			smtp.Send(mail);
		}
		catch (Exception ex) {
			ex.AddData(new {
				content = content.ToString(),
				recipients = recipients.ToString(),
				netSettings = netSettings.ToStringProperties(),
				disposeAttachments
			});
			throw;
		}
		finally {
			if (disposeAttachments && attachments != null) {
				foreach (Attachment attachment in attachments) {
					attachment.Dispose();
				}
			}
		}
	}
}
