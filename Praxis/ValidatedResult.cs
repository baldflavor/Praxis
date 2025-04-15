namespace Praxis;

using System.ComponentModel.DataAnnotations;

/// <summary>
/// Object used for signaling whether an operation was valid or invalid and exposes information describing whether
/// an operation was invalid and with messages describing the validity violations.
/// <para>When the call is valid, should contain resulting data</para>
/// </summary>
/// <remarks>
/// This should not take the place of throwing an exception when truly exceptional circumstances arise and is instead
/// meant to facilitate method calls which "expectantly" should not proceeed
/// <para>Instances of this class should primarily come through the static methods in <see cref="ValidatedResult"/> as
/// the <see cref="ValidatedResult.IsValid"/> property can only be set privately by that class</para>
/// </remarks>
/// <typeparam name="TData">Type of data which should be present when an operation is otherwise valid</typeparam>
public sealed class ValidatedResult<TData> : ValidatedResult
{
    /// <summary>
    /// Data that represents a successful operation
    /// </summary>
    /// <remarks>
    /// Depending on use case this may be null
    /// </remarks>
    private TData? _data;

    /// <summary>
    /// Returns data resultant from successful operation
    /// </summary>
    /// <param name="throwIfInvalid">Indicates whether to throw an exception if the <see cref="ValidatedResult.IsValid"/> property is
    /// not <see langword="true"/></param>
    /// <returns><typeparamref name="TData"/> (may be null)</returns>
    /// <exception cref="Exception">Thrown if <paramref name="throwIfInvalid"/> is <see langword="true"/> and <see cref="ValidatedResult.IsValid"/> is false</exception>
    public TData? Data(bool throwIfInvalid = true)
    {
        if (!this.IsValid && throwIfInvalid)
            throw new Exception("Result is invalid");

        return _data;
    }

    /// <summary>
    /// Returns (not null) data resultant from successful operation
    /// </summary>
    /// <param name="throwIfInvalid">Indicates whether to throw an exception if the <see cref="ValidatedResult.IsValid"/> property is
    /// not <see langword="true"/></param>
    /// <returns><typeparamref name="TData"/></returns>
    /// <exception cref="ArgumentNullException">Thrown if <see cref="_data"/> is null</exception>
    /// <exception cref="Exception">Thrown if <paramref name="throwIfInvalid"/> is <see langword="true"/> and <see cref="ValidatedResult.IsValid"/> is false</exception>
    public TData DataCogent(bool throwIfInvalid = true) => Assert.IsNotNull(Data(throwIfInvalid));

    /// <summary>
    /// Initializes a new instance of the <see cref="ValidatedResult" /> class
    /// </summary>
    public ValidatedResult()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ValidatedResult" /> class
    /// </summary>
    /// <param name="data">Data to set against the instance</param>
    public ValidatedResult(TData data)
    {
        _data = data;
    }

    /// <summary>
    /// Sets data to this instance
    /// </summary>
    /// <param name="data">Data to set</param>
    /// <returns><see langword="this"/></returns>
    public ValidatedResult<TData> WithData(TData data)
    {
        _data = data;
        return this;
    }
}

/// <summary>
/// Object used for signaling whether an operation was valid or invalid and exposes information describing whether
/// an operation was invalid and with messages describing the validity violations
/// </summary>
/// <remarks>
/// This should not take the place of throwing an exception when truly exceptional circumstances arise and is instead
/// meant to facilitate method calls which "expectantly" should not proceeed
/// <para>Instances of this class should primarily come through owned static methods as the
/// <see cref="ValidatedResult.IsValid"/> property can only be set privately</para>
/// </remarks>
public class ValidatedResult
{
    /// <summary>
    /// Static reference to an instance signaling the operation completed
    /// </summary>
    private static readonly ValidatedResult _ok = new() { IsValid = true };

    /// <summary>
    /// Holds validation results when an operation could not be completed
    /// </summary>
    private IEnumerable<ValidationResult>? _validationResults;

    /// <summary>
    /// Holds a dictionary of errors either composed from <see cref="_validationResults"/> or set explicitly
    /// through <see cref="Invalid(IDictionary{string, string[]})"/> or <see cref="Invalid{T}(IDictionary{string, string[]})"/>
    /// </summary>
    private IDictionary<string, string[]>? _errors;

    /// <summary>
    /// Indicates whether this instance represents a valid (successful) or invalid (not completed) result
    /// </summary>
    public bool IsValid { get; init; }

    /// <summary>
    /// Static method for an operation that is semantically invalid
    /// </summary>
    /// <typeparam name="T">Type of data that would have been assigned if the operation was successful</typeparam>
    /// <param name="validationResults"><see cref="ValidationResult"/> collection describing invalid state</param>
    /// <returns><see cref="ValidatedResult{TData}"/></returns>
    public static ValidatedResult<T> Invalid<T>(IEnumerable<ValidationResult> validationResults) => new() { _validationResults = validationResults };

    /// <summary>
    /// Static method for an operation that is semantically invalid
    /// </summary>
    /// <param name="validationResults"><see cref="ValidationResult"/> collection describing invalid state</param>
    /// <returns><see cref="ValidatedResult"/></returns>
    public static ValidatedResult Invalid(IEnumerable<ValidationResult> validationResults) => new() { _validationResults = validationResults };

    /// <summary>
    /// Static method for an operation that is semantically invalid
    /// </summary>
    /// <typeparam name="T">Type of data that would have been assigned if the operation was successful</typeparam>
    /// <param name="errors"><see cref="Dictionary{TKey, TValue}"/> of <see cref="string"/> and <see cref="string"/> array describing invalid state</param>
    /// <returns><see cref="ValidatedResult{TData}"/></returns>
    public static ValidatedResult<T> Invalid<T>(IDictionary<string, string[]> errors) => new() { _errors = errors };

    /// <summary>
    /// Static method for an operation that is semantically invalid
    /// </summary>
    /// <param name="errors"><see cref="Dictionary{TKey, TValue}"/> of <see cref="string"/> and <see cref="string"/> array describing invalid state</param>
    /// <returns><see cref="ValidatedResult"/></returns>
    public static ValidatedResult Invalid(IDictionary<string, string[]> errors) => new() { _errors = errors };

    /// <summary>
    /// Used when an operation completed successfully and has no data
    /// </summary>
    /// <returns><see cref="ValidatedResult"/> with <see cref="IsValid"/> set to <see langword="true"/></returns>
    public static ValidatedResult Ok() => _ok;


    /// <summary>
    /// Used when an operation completed successfully
    /// </summary>
    /// <typeparam name="T">Type of data (which should be assigned via <see cref="ValidatedResult{TData}.WithData(TData)"/></typeparam>
    /// <returns><see cref="ValidatedResult{TData}"/> with <see cref="IsValid"/> set to <see langword="true"/></returns>
    public static ValidatedResult<T> Ok<T>() => new() { IsValid = true };

    /// <summary>
    /// Used when an operation completed successfully
    /// </summary>
    /// <typeparam name="T">Type of data to assign to the created <see cref="ValidatedResult{TData}"/></typeparam>
    /// <returns><see cref="ValidatedResult{TData}"/> with <see cref="IsValid"/> set to <see langword="true"/> and containing
    /// <typeparamref name="T"/> data</returns>
    public static ValidatedResult<T> Ok<T>(T data) => new(data) { IsValid = true };

    /// <summary>
    /// Returns <see cref="Dictionary{TKey, TValue}"/> of <see cref="string"/> and <see cref="string"/> array describing invalid state
    /// </summary>
    /// <remarks>
    /// If not explicitly set, is internally stored from <see cref="GetValidationResults"/> when first requested
    /// </remarks>
    /// <returns><see cref="Dictionary{TKey, TValue}"/> of <see cref="string"/> and <see cref="string"/> array.
    /// <para>May be empty if called on an instance where <see cref="IsValid"/> is true</para></returns>
    public IDictionary<string, string[]> GetErrors() => _errors ??= GetValidationResults().ToPropertyErrorDictionary();

    /// <summary>
    /// Returns a collection of validation results describing invalid state
    /// </summary>
    /// <returns>A collection of <see cref="ValidationResult"/>.
    /// <para>May be empty if called on an instance where <see cref="IsValid"/> is true</para>
    /// </returns>
    public IEnumerable<ValidationResult> GetValidationResults() => _validationResults ?? [];
}