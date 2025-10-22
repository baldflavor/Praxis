namespace Praxis;

#nullable disable

using System.Collections.ObjectModel;
using System.IO.Compression;
using System.Reflection;
using System.Text.Json;
using System.Xml.Linq;
using IO;

/// <summary>
/// Class used for working with geographical / locational data
/// </summary>
public static class Location {

	/// <summary>
	/// Static variable that holds country definitions
	/// </summary>
	private static readonly Lazy<ReadOnlyCollection<Country>> _countries = new(() => new ReadOnlyCollection<Country>([

		#region Country Instantiation

				new Country("Afghanistan","AF","AFG",4),
				new Country("Aland Islands","AX","ALA",248),
				new Country("Albania","AL","ALB",8),
				new Country("Algeria","DZ","DZA",12),
				new Country("American Samoa","AS","ASM",16),
				new Country("Andorra","AD","AND",20),
				new Country("Angola","AO","AGO",24),
				new Country("Anguilla","AI","AIA",660),
				new Country("Antarctica","AQ","ATA",10),
				new Country("Antigua and Barbuda","AG","ATG",28),
				new Country("Argentina","AR","ARG",32),
				new Country("Armenia","AM","ARM",51),
				new Country("Aruba","AW","ABW",533),
				new Country("Australia","AU","AUS",36),
				new Country("Austria","AT","AUT",40),
				new Country("Azerbaijan","AZ","AZE",31),
				new Country("Bahamas","BS","BHS",44),
				new Country("Bahrain","BH","BHR",48),
				new Country("Bangladesh","BD","BGD",50),
				new Country("Barbados","BB","BRB",52),
				new Country("Belarus","BY","BLR",112),
				new Country("Belgium","BE","BEL",56),
				new Country("Belize","BZ","BLZ",84),
				new Country("Benin","BJ","BEN",204),
				new Country("Bermuda","BM","BMU",60),
				new Country("Bhutan","BT","BTN",64),
				new Country("Bolivia, Plurinational State of","BO","BOL",68),
				new Country("Bosnia and Herzegovina","BA","BIH",70),
				new Country("Botswana","BW","BWA",72),
				new Country("Bouvet Island","BV","BVT",74),
				new Country("Brazil","BR","BRA",76),
				new Country("British Indian Ocean Territory","IO","IOT",86),
				new Country("Brunei Darussalam","BN","BRN",96),
				new Country("Bulgaria","BG","BGR",100),
				new Country("Burkina Faso","BF","BFA",854),
				new Country("Burundi","BI","BDI",108),
				new Country("Cambodia","KH","KHM",116),
				new Country("Cameroon","CM","CMR",120),
				new Country("Canada","CA","CAN",124),
				new Country("Cape Verde","CV","CPV",132),
				new Country("Cayman Islands","KY","CYM",136),
				new Country("Central African Republic","CF","CAF",140),
				new Country("Chad","TD","TCD",148),
				new Country("Chile","CL","CHL",152),
				new Country("China","CN","CHN",156),
				new Country("Christmas Island","CX","CXR",162),
				new Country("Cocos (Keeling) Islands","CC","CCK",166),
				new Country("Colombia","CO","COL",170),
				new Country("Comoros","KM","COM",174),
				new Country("Congo","CG","COG",178),
				new Country("Congo, the Democratic Republic of the","CD","COD",180),
				new Country("Cook Islands","CK","COK",184),
				new Country("Costa Rica","CR","CRI",188),
				new Country("Cote d'Ivoire","CI","CIV",384),
				new Country("Croatia","HR","HRV",191),
				new Country("Cuba","CU","CUB",192),
				new Country("Cyprus","CY","CYP",196),
				new Country("Czech Republic","CZ","CZE",203),
				new Country("Denmark","DK","DNK",208),
				new Country("Djibouti","DJ","DJI",262),
				new Country("Dominica","DM","DMA",212),
				new Country("Dominican Republic","DO","DOM",214),
				new Country("Ecuador","EC","ECU",218),
				new Country("Egypt","EG","EGY",818),
				new Country("El Salvador","SV","SLV",222),
				new Country("Equatorial Guinea","GQ","GNQ",226),
				new Country("Eritrea","ER","ERI",232),
				new Country("Estonia","EE","EST",233),
				new Country("Ethiopia","ET","ETH",231),
				new Country("Falkland Islands (Malvinas)","FK","FLK",238),
				new Country("Faroe Islands","FO","FRO",234),
				new Country("Fiji","FJ","FJI",242),
				new Country("Finland","FI","FIN",246),
				new Country("France","FR","FRA",250),
				new Country("French Guiana","GF","GUF",254),
				new Country("French Polynesia","PF","PYF",258),
				new Country("French Southern Territories","TF","ATF",260),
				new Country("Gabon","GA","GAB",266),
				new Country("Gambia","GM","GMB",270),
				new Country("Georgia","GE","GEO",268),
				new Country("Germany","DE","DEU",276),
				new Country("Ghana","GH","GHA",288),
				new Country("Gibraltar","GI","GIB",292),
				new Country("Greece","GR","GRC",300),
				new Country("Greenland","GL","GRL",304),
				new Country("Grenada","GD","GRD",308),
				new Country("Guadeloupe","GP","GLP",312),
				new Country("Guam","GU","GUM",316),
				new Country("Guatemala","GT","GTM",320),
				new Country("Guernsey","GG","GGY",831),
				new Country("Guinea","GN","GIN",324),
				new Country("Guinea-Bissau","GW","GNB",624),
				new Country("Guyana","GY","GUY",328),
				new Country("Haiti","HT","HTI",332),
				new Country("Heard Island and McDonald Islands","HM","HMD",334),
				new Country("Holy See (Vatican City State)","VA","VAT",336),
				new Country("Honduras","HN","HND",340),
				new Country("Hong Kong","HK","HKG",344),
				new Country("Hungary","HU","HUN",348),
				new Country("Iceland","IS","ISL",352),
				new Country("India","IN","IND",356),
				new Country("Indonesia","ID","IDN",360),
				new Country("Iran, Islamic Republic of","IR","IRN",364),
				new Country("Iraq","IQ","IRQ",368),
				new Country("Ireland","IE","IRL",372),
				new Country("Isle of Man","IM","IMN",833),
				new Country("Israel","IL","ISR",376),
				new Country("Italy","IT","ITA",380),
				new Country("Jamaica","JM","JAM",388),
				new Country("Japan","JP","JPN",392),
				new Country("Jersey","JE","JEY",832),
				new Country("Jordan","JO","JOR",400),
				new Country("Kazakhstan","KZ","KAZ",398),
				new Country("Kenya","KE","KEN",404),
				new Country("Kiribati","KI","KIR",296),
				new Country("Korea, Democratic People's Republic of","KP","PRK",408),
				new Country("Korea, Republic of","KR","KOR",410),
				new Country("Kuwait","KW","KWT",414),
				new Country("Kyrgyzstan","KG","KGZ",417),
				new Country("Lao People's Democratic Republic","LA","LAO",418),
				new Country("Latvia","LV","LVA",428),
				new Country("Lebanon","LB","LBN",422),
				new Country("Lesotho","LS","LSO",426),
				new Country("Liberia","LR","LBR",430),
				new Country("Libyan Arab Jamahiriya","LY","LBY",434),
				new Country("Liechtenstein","LI","LIE",438),
				new Country("Lithuania","LT","LTU",440),
				new Country("Luxembourg","LU","LUX",442),
				new Country("Macao","MO","MAC",446),
				new Country("Macedonia, the former Yugoslav Republic of","MK","MKD",807),
				new Country("Madagascar","MG","MDG",450),
				new Country("Malawi","MW","MWI",454),
				new Country("Malaysia","MY","MYS",458),
				new Country("Maldives","MV","MDV",462),
				new Country("Mali","ML","MLI",466),
				new Country("Malta","MT","MLT",470),
				new Country("Marshall Islands","MH","MHL",584),
				new Country("Martinique","MQ","MTQ",474),
				new Country("Mauritania","MR","MRT",478),
				new Country("Mauritius","MU","MUS",480),
				new Country("Mayotte","YT","MYT",175),
				new Country("Mexico","MX","MEX",484),
				new Country("Micronesia, Federated States of","FM","FSM",583),
				new Country("Moldova, Republic of","MD","MDA",498),
				new Country("Monaco","MC","MCO",492),
				new Country("Mongolia","MN","MNG",496),
				new Country("Montenegro","ME","MNE",499),
				new Country("Montserrat","MS","MSR",500),
				new Country("Morocco","MA","MAR",504),
				new Country("Mozambique","MZ","MOZ",508),
				new Country("Myanmar","MM","MMR",104),
				new Country("Namibia","NA","NAM",516),
				new Country("Nauru","NR","NRU",520),
				new Country("Nepal","NP","NPL",524),
				new Country("Netherlands","NL","NLD",528),
				new Country("Netherlands Antilles","AN","ANT",530),
				new Country("New Caledonia","NC","NCL",540),
				new Country("New Zealand","NZ","NZL",554),
				new Country("Nicaragua","NI","NIC",558),
				new Country("Niger","NE","NER",562),
				new Country("Nigeria","NG","NGA",566),
				new Country("Niue","NU","NIU",570),
				new Country("Norfolk Island","NF","NFK",574),
				new Country("Northern Mariana Islands","MP","MNP",580),
				new Country("Norway","NO","NOR",578),
				new Country("Oman","OM","OMN",512),
				new Country("Pakistan","PK","PAK",586),
				new Country("Palau","PW","PLW",585),
				new Country("Palestinian Territory, Occupied","PS","PSE",275),
				new Country("Panama","PA","PAN",591),
				new Country("Papua New Guinea","PG","PNG",598),
				new Country("Paraguay","PY","PRY",600),
				new Country("Peru","PE","PER",604),
				new Country("Philippines","PH","PHL",608),
				new Country("Pitcairn","PN","PCN",612),
				new Country("Poland","PL","POL",616),
				new Country("Portugal","PT","PRT",620),
				new Country("Puerto Rico","PR","PRI",630),
				new Country("Qatar","QA","QAT",634),
				new Country("Reunion","RE","REU",638),
				new Country("Romania","RO","ROU",642),
				new Country("Russian Federation","RU","RUS",643),
				new Country("Rwanda","RW","RWA",646),
				new Country("Saint Bartholemy","BL","BLM",652),
				new Country("Saint Helena","SH","SHN",654),
				new Country("Saint Kitts and Nevis","KN","KNA",659),
				new Country("Saint Lucia","LC","LCA",662),
				new Country("Saint Martin (French part)","MF","MAF",663),
				new Country("Saint Pierre and Miquelon","PM","SPM",666),
				new Country("Saint Vincent and the Grenadines","VC","VCT",670),
				new Country("Samoa","WS","WSM",882),
				new Country("San Marino","SM","SMR",674),
				new Country("Sao Tome and Principe","ST","STP",678),
				new Country("Saudi Arabia","SA","SAU",682),
				new Country("Senegal","SN","SEN",686),
				new Country("Serbia","RS","SRB",688),
				new Country("Seychelles","SC","SYC",690),
				new Country("Sierra Leone","SL","SLE",694),
				new Country("Singapore","SG","SGP",702),
				new Country("Slovakia","SK","SVK",703),
				new Country("Slovenia","SI","SVN",705),
				new Country("Solomon Islands","SB","SLB",90),
				new Country("Somalia","SO","SOM",706),
				new Country("South Africa","ZA","ZAF",710),
				new Country("South Georgia and the South Sandwich Islands","GS","SGS",239),
				new Country("Spain","ES","ESP",724),
				new Country("Sri Lanka","LK","LKA",144),
				new Country("Sudan","SD","SDN",736),
				new Country("Suriname","SR","SUR",740),
				new Country("Svalbard and Jan Mayen","SJ","SJM",744),
				new Country("Swaziland","SZ","SWZ",748),
				new Country("Sweden","SE","SWE",752),
				new Country("Switzerland","CH","CHE",756),
				new Country("Syrian Arab Republic","SY","SYR",760),
				new Country("Taiwan, Province of China","TW","TWN",158),
				new Country("Tajikistan","TJ","TJK",762),
				new Country("Tanzania, United Republic of","TZ","TZA",834),
				new Country("Thailand","TH","THA",764),
				new Country("Timor-Leste","TL","TLS",626),
				new Country("Togo","TG","TGO",768),
				new Country("Tokelau","TK","TKL",772),
				new Country("Tonga","TO","TON",776),
				new Country("Trinidad and Tobago","TT","TTO",780),
				new Country("Tunisia","TN","TUN",788),
				new Country("Turkey","TR","TUR",792),
				new Country("Turkmenistan","TM","TKM",795),
				new Country("Turks and Caicos Islands","TC","TCA",796),
				new Country("Tuvalu","TV","TUV",798),
				new Country("Uganda","UG","UGA",800),
				new Country("Ukraine","UA","UKR",804),
				new Country("United Arab Emirates","AE","ARE",784),
				new Country("United Kingdom","GB","GBR",826),
				new Country("United States","US","USA",840),
				new Country("United States Minor Outlying Islands","UM","UMI",581),
				new Country("Uruguay","UY","URY",858),
				new Country("Uzbekistan","UZ","UZB",860),
				new Country("Vanuatu","VU","VUT",548),
				new Country("Venezuela, Bolivarian Republic of","VE","VEN",862),
				new Country("Viet Nam","VN","VNM",704),
				new Country("Virgin Islands, British","VG","VGB",92),
				new Country("Virgin Islands, U.S.","VI","VIR",850),
				new Country("Wallis and Futuna","WF","WLF",876),
				new Country("Western Sahara","EH","ESH",732),
				new Country("Yemen","YE","YEM",887),
				new Country("Zambia","ZM","ZMB",894),
				new Country("Zimbabwe","ZW","ZWE",716)

#endregion Country Instantiation

	]));

	/// <summary>
	/// Holds a dictionary of countries keyed by the numeric country code
	/// </summary>
	private static readonly Lazy<Dictionary<short, Country>> _countryDict = new(() => _countries.Value.ToDictionary(c => c.Numeric));

	/// <summary>
	/// Dictionary of city and states by 5 digit zip codes
	/// </summary>
	private static readonly Lazy<Dictionary<string, ZipCityState>> _locationDict = new(() => {
		#region Initialization Of Zip City State Data

		using Stream inputStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Praxis.EmbeddedResource.ZipCityState.dat");
		using BrotliStream compStream = new(inputStream, CompressionMode.Decompress);

		using MemoryStream output = new();
		compStream.CopyTo(output);
		output.Position = 0;

		return
			JsonSerializer
				.Deserialize<ZipCityState[]>(output, Json.DefaultOptions)
			.ToDictionary(zcs => zcs.ZipCode, StringComparer.OrdinalIgnoreCase);

		#endregion Initialization Of Zip City State Data
	});

	/// <summary>
	/// Static variable that holds province abbreviations
	/// </summary>
	private static readonly Lazy<ReadOnlyCollection<string>> _provinceAbbreviations = new(() => new ReadOnlyCollection<string>([

		#region Province Definition

		"AB",
		"BC",
		"MB",
		"NB",
		"NL",
		"NT",
		"NS",
		"NU",
		"ON",
		"PE",
		"QC",
		"SK",
		"YT"

		#endregion Province Definition

	]));

	/// <summary>
	/// Static variable that holds state abbreviations
	/// </summary>
	private static readonly Lazy<ReadOnlyCollection<string>> _stateAbbreviations = new(() => new ReadOnlyCollection<string>([

		#region State Definitions

		"AK",
		"AL",
		"AR",
		"AZ",
		"CA",
		"CO",
		"CT",
		"DC",
		"DE",
		"FL",
		"GA",
		"GU",
		"HI",
		"IA",
		"ID",
		"IL",
		"IN",
		"KS",
		"KY",
		"LA",
		"MA",
		"MD",
		"ME",
		"MI",
		"MN",
		"MO",
		"MS",
		"MT",
		"NC",
		"ND",
		"NE",
		"NH",
		"NJ",
		"NM",
		"NV",
		"NY",
		"OH",
		"OK",
		"OR",
		"PA",
		"PR",
		"RI",
		"SC",
		"SD",
		"TN",
		"TX",
		"UT",
		"VA",
		"VT",
		"WA",
		"WI",
		"WV",
		"WY"

#endregion State Definitions

	]));

	/// <summary>
	/// Gets a read only collection of countries
	/// </summary>
	public static ReadOnlyCollection<Country> Countries {
		get {
			return _countries.Value;
		}
	}

	/// <summary>
	/// Returns a read only collection of abbreviations for Canadian provinces
	/// <para>All abbreviations are in upper case</para>
	/// </summary>
	public static ReadOnlyCollection<string> ProvinceAbbreviations {
		get {
			return _provinceAbbreviations.Value;
		}
	}

	/// <summary>
	/// Returns a read only collection of abbreviations representing the United States of America
	/// <para>All abbreviations are in upper case</para>
	/// </summary>
	public static ReadOnlyCollection<string> StateAbbreviations {
		get {
			return _stateAbbreviations.Value;
		}
	}

	/// <summary>
	/// Returns a (cloned)country object by the passed numeric country code
	/// </summary>
	/// <param name="arg">Country code used for retrieval</param>
	/// <returns>A country</returns>
	/// <exception cref="KeyNotFoundException">Thrown if the requested arg short does not exist in the backing country dictionary</exception>
	public static Country CountryByNumeric(short arg) {
		return _countryDict.Value[arg];
	}

	/// <summary>
	/// Returns a list of all zipcodes stored in the location dictionary
	/// </summary>
	/// <returns>A list of strings that represent US zip codes</returns>
	public static List<string> GetAllZipCodes() {
		return _locationDict.Value.Values.Select(v => v.ZipCode).ToList();
	}

	/// <summary>
	/// Gets a location object from backing data store against the passed zip code
	/// </summary>
	/// <param name="zipCode">The zip code to search for</param>
	/// <returns>ZipCityState object</returns>
	public static ZipCityState GetZipCityState(string zipCode) {
		return _locationDict.Value[zipCode];
	}

	/// <summary>
	/// Attempts to get a location object from backing data store against the passed zip code
	/// </summary>
	/// <param name="zipCode">The zip code to search for</param>
	/// <param name="zipCityState">ZipCityState object if match is found</param>
	/// <returns>A value indicating whether a location was found for the passed zip code</returns>
	public static bool TryGetZipCityState(string zipCode, out ZipCityState zipCityState) {
		return _locationDict.Value.TryGetValue(zipCode, out zipCityState);
	}



	/// <summary>
	/// Class that represents the fields for identifying countries
	/// </summary>
	/// <param name="Name">Name of the country</param>
	/// <param name="Alpha2">2 character alphanumeric code</param>
	/// <param name="Alpha3">3 character alphanumeric code</param>
	/// <param name="Numeric">Integer code</param>
	public record class Country(string Name, string Alpha2, string Alpha3, short Numeric) {

		/// <inheritdoc/>
		public override string ToString() {
			return this.Name + " (" + this.Alpha3 + ")";
		}
	}


	/// <summary>
	/// Class that holds a zip code, a city, and a state
	/// </summary>
	public record class ZipCityState(string ZipCode, string City, string State);
}
