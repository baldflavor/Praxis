namespace Praxis.Net;

using System.Net;
using System.Net.NetworkInformation;
using System.Net.Security;
using System.Runtime.Versioning;
using System.Text;

/// <remarks>
/// Represents generic network tool methods
/// </remarks>
public static class Tools {
	/// <summary>
	/// A user agent string taken from a current browser
	/// </summary>
	public const string USERAGENT = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/131.0.0.0 Safari/537.36";

	/// <summary>
	/// Creates an HttpClient instance that can be used for network transmissions
	/// </summary>
	/// <remarks>
	/// You may wish to set additional headers, or user agents such as the following:
	/// <list type="bullet">
	/// <item>client.DefaultRequestHeaders.Add("Connection", "Closed");</item>
	/// <item>client.DefaultRequestHeaders.Add("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8");</item>
	/// <item>client.DefaultRequestHeaders.Add("Accept-Encoding", "gzip, deflate, br");</item>
	///	<item>client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/112.0.0.0 Safari/537.36");</item>
	///	</list>
	/// </remarks>
	/// <param name="connectTimeoutSeconds">Number of milliseconds to wait for preamble connection / retrieval of connection from pool and establishment</param>
	/// <param name="decompressionMethod">Compression and decompression encoding format to be used to compress the data received in response to an System.Net.HttpWebRequest</param>
	/// <param name="defaultRequestVersion">The HttpVersion to use when making requests; when null a default of <see cref="HttpVersion.Version20"/> is used</param>
	/// <param name="pooledConnectionLifetimeMinutes">How long each pooled connection should live before expiry</param>
	/// <param name="remoteCertificateValidationCallback">Delegate used for sub processing or altering SSL validation - useful for debugging purposes or when allowing a trusted host or serial number. Be very careful with this callback to avoid security malfeasance</param>
	/// <param name="timeoutSeconds">Seconds to wait before timing out a response from a request</param>
	/// <returns>An <see cref="HttpClient"/></returns>
	public static HttpClient CreateClient(int connectTimeoutSeconds = 15, DecompressionMethods decompressionMethod = DecompressionMethods.All, Version? defaultRequestVersion = null, int pooledConnectionLifetimeMinutes = 5, RemoteCertificateValidationCallback? remoteCertificateValidationCallback = null, int timeoutSeconds = 25) {
		var handler = new SocketsHttpHandler() {
			AutomaticDecompression = decompressionMethod,
			ConnectTimeout = TimeSpan.FromSeconds(connectTimeoutSeconds),
			PooledConnectionLifetime = TimeSpan.FromMinutes(pooledConnectionLifetimeMinutes)
		};

		if (remoteCertificateValidationCallback != null)
			handler.SslOptions.RemoteCertificateValidationCallback = remoteCertificateValidationCallback;

		return new HttpClient(handler) {
			Timeout = TimeSpan.FromSeconds(timeoutSeconds),
			DefaultRequestVersion = defaultRequestVersion ?? HttpVersion.Version20
		};
	}

	/// <summary>
	/// Creates an HttpClient instance that can be used for network transmissions configured to have request headers specifying application/json
	/// </summary>
	/// <remarks>
	/// You may wish to set additional headers, or user agents such as the following:
	/// <list type="bullet">
	/// <item>client.DefaultRequestHeaders.Add("Connection", "Closed");</item>
	/// <item>client.DefaultRequestHeaders.Add("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8");</item>
	/// <item>client.DefaultRequestHeaders.Add("Accept-Encoding", "gzip, deflate, br");</item>
	///	<item>client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/112.0.0.0 Safari/537.36");</item>
	///	</list>
	/// </remarks>
	/// <param name="connectTimeoutSeconds">Number of milliseconds to wait for preamble connection / retrieval of connection from pool and establishment</param>
	/// <param name="decompressionMethod">Compression and decompression encoding format to be used to compress the data received in response to an System.Net.HttpWebRequest</param>
	/// <param name="defaultRequestVersion">The HttpVersion to use when making requests; when null a default of <see cref="HttpVersion.Version20"/> is used</param>
	/// <param name="pooledConnectionLifetimeMinutes">How long each pooled connection should live before expiry</param>
	/// <param name="remoteCertificateValidationCallback">Delegate used for sub processing or altering SSL validation - useful for debugging purposes or when allowing a trusted host or serial number. Be very careful with this callback to avoid security malfeasance</param>
	/// <param name="timeoutSeconds">Seconds to wait before timing out a response from a request</param>
	/// <returns>An <see cref="HttpClient"/></returns>
	public static HttpClient CreateJsonClient(int connectTimeoutSeconds = 15, DecompressionMethods decompressionMethod = DecompressionMethods.All, Version? defaultRequestVersion = null, int pooledConnectionLifetimeMinutes = 5, RemoteCertificateValidationCallback? remoteCertificateValidationCallback = null, int timeoutSeconds = 25) {
		HttpClient client = CreateClient(connectTimeoutSeconds, decompressionMethod, defaultRequestVersion, pooledConnectionLifetimeMinutes, remoteCertificateValidationCallback, timeoutSeconds);

		client.DefaultRequestHeaders.Accept.Clear();
		client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

		return client;
	}

	/// <summary>
	/// Returns a string detailing each network interface on the host machine
	/// </summary>
	/// <returns>A string; could be empty if none were found. On exception, the returned string will keep exception detail</returns>
	[SupportedOSPlatform("windows")]
	public static string GetNetworkInterfaceDetails() => GetNetworkInterfaceDetails(false);

	/// <summary>
	/// Returns a string detailing each network interface on the host machine
	/// </summary>
	/// <param name="operationalUpOnly">Indicates whether only network adapters that have OperationalStatus.Up should be recorded</param>
	/// <returns>A string; could be empty if none were found. On exception, the returned string will keep exception detail</returns>
	[SupportedOSPlatform("windows")]
	public static string GetNetworkInterfaceDetails(bool operationalUpOnly) {
		StringBuilder sb = new();
		try {
			foreach (NetworkInterface nif in NetworkInterface.GetAllNetworkInterfaces()) {
				if (operationalUpOnly && nif.OperationalStatus != OperationalStatus.Up)
					continue;

				sb.AppendLine("Network Interface:");
				sb.AppendLine("-Name: " + nif.Name);
				sb.AppendLine("-Description: " + nif.Description);
				sb.AppendLine("-Interface Type: " + nif.NetworkInterfaceType);
				sb.AppendLine("-Operational Status: " + nif.OperationalStatus);
				sb.AppendLine("-Speed: " + nif.Speed);
				sb.AppendLine("-Multicast Support: " + nif.SupportsMulticast);
				sb.AppendLine("-MAC: " + nif.GetPhysicalAddress().ToString());

				sb.AppendLine("-IP:");
				IPInterfaceProperties ip = nif.GetIPProperties();
				sb.AppendLine("--DNS Suffix: " + ip.DnsSuffix);
				sb.AppendLine("--DNS Enabled: " + ip.IsDnsEnabled);
				sb.AppendLine("--DNS Dynamic Enabled: " + ip.IsDynamicDnsEnabled);


				StringBuilder dnsSb = new();
				IPAddressCollection dnscol = ip.DnsAddresses;
				for (int dni = 0; dni < dnscol.Count; dni++) {
					dnsSb.AppendLine("--- " + dnscol[dni].ToString());
				}

				if (dnsSb.Length > 0) {
					sb.AppendLine("--DNS Addresses:" + Environment.NewLine + dnsSb.Remove(dnsSb.Length - 2, 2).ToString());
				}

				StringBuilder dhcpSb = new();
				IPAddressCollection dhcpcol = ip.DhcpServerAddresses;
				for (int dhi = 0; dhi < dhcpcol.Count; dhi++) {
					dhcpSb.AppendLine("--- " + dhcpcol[dhi].ToString());
				}

				if (dhcpSb.Length > 0) {
					sb.AppendLine("--DHCP Addresses:" + Environment.NewLine + dhcpSb.Remove(dhcpSb.Length - 2, 2).ToString());
				}


				StringBuilder gwSb = new();
				GatewayIPAddressInformationCollection gcol = ip.GatewayAddresses;
				for (int gwi = 0; gwi < gcol.Count; gwi++) {
					gwSb.AppendLine("--- " + gcol[gwi].Address.ToString());
				}

				if (gwSb.Length > 0) {
					sb.AppendLine("--Gateway Addresses: " + Environment.NewLine + gwSb.Remove(gwSb.Length - 2, 2).ToString());
				}


				StringBuilder mcSb = new();
				MulticastIPAddressInformationCollection mccol = ip.MulticastAddresses;
				for (int mci = 0; mci < gcol.Count; mci++) {
					mcSb.AppendLine("--- " + mccol[mci].Address.ToString());
				}

				if (mcSb.Length > 0) {
					sb.AppendLine("--MultiCast Addresses: " + Environment.NewLine + mcSb.Remove(mcSb.Length - 2, 2).ToString());
				}
			}
		}
		catch (Exception ex) {
			sb.AppendLine("EXCEPTION OCCURRED:" + Environment.NewLine + ex.ToString());
		}

		return sb.ToString();
	}

	/// <summary>
	/// Calls <c>DefaultRequestHeaders.UserAgent.TryParseAdd</c> on the passed <see cref="HttpClient"/> using the supplied or default user agent
	/// </summary>
	/// <param name="client">The client to add the user agent to</param>
	/// <param name="userAgent">The user agent string to attempt to add; if not supplied will use <see cref="USERAGENT"/></param>
	/// <returns><see cref="HttpClient"/></returns>
	public static HttpClient TryParseAddUserAgent(this HttpClient client, string userAgent = USERAGENT) {
		client.DefaultRequestHeaders.UserAgent.TryParseAdd(userAgent);
		return client;
	}
}