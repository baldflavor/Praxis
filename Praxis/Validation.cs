namespace Praxis;

using System.Text.RegularExpressions;

/// <summary>
/// Static class for validating various string related patterns of information
/// </summary>
public static partial class Validation {

	/// <summary>
	/// Regular expression pattern that matches valid characters for a postal street address
	/// </summary>
	public const string ADDRESS = @"^[a-zA-Z0-9\'\,\.\- #/]+$";

	/// <summary>
	/// Description for the ADDRESS regular expression: Alphanumeric characters, space, apostrophe, comma, period, dash, # and /
	/// </summary>
	public const string ADDRESSDESCRIPTION = "Alphanumeric characters, space, apostrophe, comma, period, dash, # and /";


	/// <summary>
	/// Regular expression pattern that matches alphabetic characters
	/// </summary>
	public const string ALPHA = @"^[a-zA-Z]+$";

	/// <summary>
	/// Description for the ALPHA regular expression: Alphabetic characters
	/// </summary>
	public const string ALPHADESCRIPTION = "Alphabetic characters";


	/// <summary>
	/// Regular expression pattern that matches alphanumeric characters
	/// </summary>
	public const string ALPHANUMERIC = @"^[a-zA-Z0-9]+$";

	/// <summary>
	/// Description for ALPHANUMERIC regular expression: Alphanumeric characters
	/// </summary>
	public const string ALPHANUMERICDESCRIPTION = "Alphanumeric characters";


	/// <summary>
	/// Regular expression pattern that matches common bracket and delimiter characters
	/// </summary>
	public const string BRACKETSDELIMITERS = @"[\[\]\<\>\{\}\|]";

	/// <summary>
	/// Description for the allowed BRACKETSDELIMITERS regular expression: <![CDATA[<, >, [, ], {, }, |]]>
	/// <para>Warning: using this as ErrorMessage value will throw an exception due to the { and } characters</para>
	/// </summary>
	public const string BRACKETSDELIMITERSDESCRIPTION = "<, >, [, ], {, }, |";


	/// <summary>
	/// Regular expression pattern that matches valid characters for a city
	/// </summary>
	public const string CITY = @"^[a-zA-Z\'\-\. ]+$";

	/// <summary>
	/// Description for the CITY regular expression: Letters, apostrophe, dash, period and space
	/// </summary>
	public const string CITYDESCRIPTION = "Letters, apostrophe, dash, period and space";


	/// <summary>
	/// Regular expression pattern that matches numeric digits
	/// </summary>
	public const string DIGIT = @"^\d+$";

	/// <summary>
	/// Description for the DIGIT regular expression: Numeric digits
	/// </summary>
	public const string DIGITDESCRIPTION = "Numeric digits";

	/// <summary>
	/// Regular expression pattern that matches E.164 formatted phone numbers
	/// <para>https://en.wikipedia.org/wiki/E.164</para>
	/// </summary>
	public const string E164PHONE = @"^\+[1-9]\d{1,14}$";

	/// <summary>
	/// Description for the E164PHONE regular expression: Leading + and then at least two numeric digits and no more than 15 digits total
	/// <para>https://en.wikipedia.org/wiki/E.164</para>
	/// </summary>
	public const string E164PHONEDESCRIPTION = "Leading + and then at least two numeric digits and no more than 15 digits total";


	/// <summary>
	/// Provides a pattern for regular expression matching email addresses. Note though, that there is a prevailing opinion that
	/// you almost can't 100% accurately determine this, that either most are too loose or too strict. That really allowing a user
	/// to use whatever address "they want" and then send them a message and validate it that way is the "only real way".
	/// <para>254 characters *should* be the limit</para>
	/// <para>You may also wish to use http://msdn.microsoft.com/en-us/library/system.net.mail.mailaddress.aspx</para>
	/// <para>http://haacked.com/archive/2007/08/21/i-knew-how-to-validate-an-email-address-until-i.aspx</para>
	/// <para>http://www.troyhunt.com/2013/11/dont-trust-net-web-forms-email-regex.html</para>
	/// <para>http://en.wikipedia.org/wiki/Email_address#Local_part</para>
	/// </summary>
	public const string EMAIL = @"^[^@]+@[^@]+\.[^@]+$";

	/// <summary>
	/// Description for the EMAIL regular expression: One @, at least one character before the @, and then a character before the period and after it
	/// </summary>
	public const string EMAILDESCRIPTION = "One @, at least one character before the @, and then a character before the period and after it";


	/// <summary>
	/// Regular expression pattern that matches valid characters for an individual's name
	/// </summary>
	public const string NAME = @"^[a-zA-Z\'\-\. ]+$";

	/// <summary>
	/// Description for the NAME regular expression: Letters, apostrophe, dash, period and space
	/// </summary>
	public const string NAMEDESCRIPTION = "Letters, apostrophe, dash, period and space";


	/// <summary>
	/// Regular expression pattern that matches valid characters for the name of a commercial organization
	/// </summary>
	public const string ORGANIZATIONNAME = @"^[0-9a-zA-Z\'\-\. &,/]+$";

	/// <summary>
	/// Description for the ORGANIZATIONNAME regular expression: Alphanumeric characters, apostrophe, dash, period, space, ampersand, commas and forward slash
	/// </summary>
	public const string ORGANIZATIONNAMEDESCRIPTION = "Alphanumeric characters, apostrophe, dash, period, space, &, commas, and /";


	/// <summary>
	/// Regular expression pattern that matches social security numbers
	/// </summary>
	public const string SSN = @"^([0-6]\d{2}|7[0-6]\d|77[0-2])([ \-]?)(\d{2})\2(\d{4})$";

	/// <summary>
	/// Description for the SSN regular expression: Numeric characters in the form of 999-99-9999
	/// </summary>
	public const string SSNDESCRIPTION = "Numeric characters in the form of 999-99-9999";

	/// <summary>
	/// Regular expression pattern that matches valid US states and canadian provinces. Uppercase.
	/// </summary>
	public const string STATE = @"^(?:(A[BKLRZ]|BC|C[AOT]|D[CE]|FL|GA|HI|I[ADLN]|K[SY]|LA|M[ABDEINOST]|N[BCDEHJLMSTUVY]|O[HKNR]|P[AER]|QC|RI|S[CDK]|T[NX]|UT|V[AIT]|W[AIVY]|YT))$";

	/// <summary>
	/// Description for the STATE regular expression: Two alphabetic characters matching a valid US state or Canadian province
	/// </summary>
	public const string STATEDESCRIPTION = "Two alphabetic characters matching a valid US state or Canadian province";

	/// <summary>
	/// Regular expression pattern that matches valid U.S. / N.A. phone numbers
	/// </summary>
	public const string USPHONE = @"^(?<area>\d{3})(?<prefix>[\d]{3})(?<suffix>[\d]{4})$";

	/// <summary>
	/// Description for the USPHONE regular expression: 3 numeric characters, followed by 7 numeric characters
	/// </summary>
	public const string USPHONEDESCRIPTION = "10 numeric characters";

	/// <summary>
	/// Regular expression pattern that matches us postal/zip codes. 5 and 9 digit types supported
	/// </summary>
	public const string ZIP = @"^\d{5}(\-\d{4})?$";

	/// <summary>
	/// Description for the ZIP regular expression: Either 5 numeric characters, or 5 numeric characeters a dash and 4 numeric characters
	/// </summary>
	public const string ZIPDESCRIPTION = "Either 5 numeric characters, or 5 numeric characeters a dash and 4 numeric characters";




	/// <summary>
	/// Gets a regex that matches addresses
	/// <para>Allows letters, digits, space, apostrophe, comma, period, dash, #, /</para>
	/// </summary>
	[GeneratedRegex(ADDRESS)]
	public static partial Regex Address();

	/// <summary>
	/// Gets a regex used for scrubbing invalid characters from addresses
	/// <para>Removes all that are not letters, digits, space, apostrophe, comma, period, dash, #, /</para>
	/// </summary>
	[GeneratedRegex(@"[^a-zA-Z0-9\'\,\.\- #/]")]
	public static partial Regex AddressScrubber();

	/// <summary>
	/// Gets a regex that matches any number of alphanumeric characters (letters) -- upper or lowercase
	/// </summary>
	[GeneratedRegex(ALPHA)]
	public static partial Regex Alpha();



	/// <summary>
	/// Gets a regex that matches any number of alphanumeric characters (letters) -- upper or lowercase, or numeric digits 0-9
	/// </summary>
	[GeneratedRegex(ALPHANUMERIC)]
	public static partial Regex AlphaNumeric();



	/// <summary>
	/// Gets a regular expression that matches brackets and delimiters: less than, greater than, [, ], {, }, or |
	/// </summary>
	[GeneratedRegex(BRACKETSDELIMITERS)]
	public static partial Regex BracketsDelimiters();

	/// <summary>
	/// Gets a regular expression used for matching cities
	/// <para>Allows letters, apostrophe, dash, space, period</para>
	/// </summary>
	[GeneratedRegex(CITY)]
	public static partial Regex City();

	/// <summary>
	/// Gets a regular expression used for scrubbing invalid characters from cities
	/// <para>Remove all that are not: letters, apostrophe, dash, space, period</para>
	/// </summary>
	[GeneratedRegex(@"[^a-zA-Z\'\-\. ]")]
	public static partial Regex CityScrubber();


	/// <summary>
	/// Gets a regular expression that will match anything comprised of numeric digits
	/// </summary>
	[GeneratedRegex(DIGIT)]
	public static partial Regex Digit();

	/// <summary>
	/// Gets a regular expression for matching E.164 phone numbers. Typically +[countrycode][number] with no other formatting
	/// <para>https://en.wikipedia.org/wiki/E.164</para>
	/// </summary>
	[GeneratedRegex(E164PHONE)]
	public static partial Regex E164Phone();

	/// <summary>
	/// Gets a regex that matches a simple email address patten - that at least one character, an @ and then a character a dot and then a character after it, exist
	/// <para>Consider .IsValidEmailAddress function use instead/if possible (very liberal)</para>
	/// <para>See the constant definition for more detail</para>
	/// </summary>
	[GeneratedRegex(EMAIL)]
	public static partial Regex Email();


	/// <summary>
	/// Gets a regular expression used for validating names
	/// <para>Allows letters (case insensitive), apostrophes, dashes, period, and space</para>
	/// </summary>
	[GeneratedRegex(NAME)]
	public static partial Regex Name();

	/// <summary>
	/// Gets a regular expression used for scrubbing invalid characters from names
	/// <para>Removes all that are not: letters (case insensitive), apostrophes, dashes, period, and space</para>
	/// </summary>
	[GeneratedRegex(@"[^a-zA-Z\'\-\. ]")]
	public static partial Regex NameScrubber();


	/// <summary>
	/// Gets a regular expression used for validating organization (company) names
	/// <para>Allows letters (case insensitive), numeric digits, apostrophes, dashes, period, ampersand, comma, and space</para>
	/// </summary>
	[GeneratedRegex(ORGANIZATIONNAME)]
	public static partial Regex OrganizationName();

	/// <summary>
	/// Gets a regular expression used for scrubbing invalid cahracters from organization (company) names
	/// <para>Removes all that are not: letters (case insensitive), numeric digits, apostrophes, dashes, period, ampersand, comma, and space</para>
	/// </summary>
	[GeneratedRegex(@"[^0-9a-zA-Z\'\-\. &,/]")]
	public static partial Regex OrganizationNameScrubber();


	/// <summary>
	/// Gets a SSN regex to match 001 - 772.   Can have punctuation or not.
	/// <para>Matches: 145470191 | 145 47 0191 | 145-47 0191</para>
	/// <para>Non-matches: 000470191 | 145-00-0191 | 145.47.0191</para>
	/// </summary>
	[GeneratedRegex(SSN)]
	public static partial Regex SocialSecurityNumber();


	/// <summary>
	/// Gets a regular expression that matches states and canadian provinces (uppercase)
	/// </summary>
	[GeneratedRegex(STATE, RegexOptions.IgnoreCase)]
	public static partial Regex State();

	/// <summary>
	/// Gets a regular expression for matching U.S. / N.A. phone numbers -- in this case the 10 character portion of a number:
	/// 3 numeric digits, then 3 alphanumeric characters, then 4 alphanumeric characters
	/// <para>Case insensitive</para>
	/// <para>Captured into groups: area, prefix, suffix</para>
	/// <para>Use Regex to get Match, check for success, and check that the groups collection contains exactly 4</para>
	/// </summary>
	[GeneratedRegex(USPHONE)]
	public static partial Regex USPhone();

	/// <summary>
	/// Gets a regular expression for matching zip codes -- will match 5 digits, or 5 digits a dash and 4 digits
	/// </summary>
	[GeneratedRegex(ZIP)]
	public static partial Regex Zip();
}