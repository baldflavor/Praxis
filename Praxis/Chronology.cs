namespace Praxis;

/// <summary>
/// Static class for validating various date constructs
/// </summary>
public static class Chronology {

	/// <summary>
	/// Gets the age in years of the passed dob against the passed date
	/// </summary>
	/// <param name="dob">The date of birth to use for comparison</param>
	/// <param name="compDate">The date to use as comparison against; if null then DateTimeOffset.UtcNow will be used</param>
	/// <returns>The number of years representing age</returns>
	public static int GetAgeInYears(DateTimeOffset dob, DateTimeOffset? compDate = null) {
		DateTimeOffset compDateVal = compDate ?? DateTimeOffset.UtcNow;

		// get the difference in years
		int years = compDateVal.Year - dob.Year;

		// subtract another year if we're before the birth day in the current year
		if (compDateVal.Month < dob.Month || (compDateVal.Month == dob.Month && compDateVal.Day < dob.Day))
			--years;

		return years;
	}

	/// <summary>
	/// Gets the quarter of the year this month is in based on even calendar distribution
	/// </summary>
	/// <param name="arg">The arg month to get a quarter for</param>
	/// <returns>The quarter (1-4) of the current month</returns>
	/// <exception cref="ArgumentOutOfRangeException">If month is less than 1 or greater than 12</exception>
	public static int GetCalendarQuarter(int arg) {
		ArgumentOutOfRangeException.ThrowIfLessThan(arg, 1);
		ArgumentOutOfRangeException.ThrowIfGreaterThan(arg, 12);

		return arg switch {
			<= 3 => 1,
			<= 6 => 2,
			<= 9 => 3,
			_ => 4
		};
	}
}