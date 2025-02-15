using System.Text.Json.Serialization;

namespace Domain.Shared;

public readonly struct Error
{
    public string Code { get; }
    public string Description { get; }
    [JsonIgnore]
    public ErrorType ErrorType { get; }

    public Error(string code, string description, ErrorType errorType)
    {
        Code = code;
        Description = description;
        ErrorType = errorType;
    }
    public Error()
    {

    }

    // Predefined common errors
    public static readonly Error None = new(string.Empty, string.Empty, ErrorType.Failure);
    public static readonly Error NullValue = new("Error.NullValue", "Null value was provided", ErrorType.Validation);

    // Default error messages
    private static class DefaultErrors
    {
        // Basic errors
        public const string FailureCode = "Failure";
        public const string FailureDescription = "An unexpected error occurred";

        public const string ValidationCode = "Validation";
        public const string ValidationDescription = "The provided data is invalid";

        // Authentication & Authorization
        public const string UnauthorizedCode = "Unauthorized";
        public const string UnauthorizedDescription = "Authentication is required to access this resource";

        public const string ForbiddenCode = "Forbidden";
        public const string ForbiddenDescription = "You have no permission to access the resource";

        // Resource errors
        public const string NotFoundCode = "NotFound";
        public const string NotFoundDescription = "Resource not found";

        public const string ConflictCode = "Conflict";
        public const string ConflictDescription = "A conflict occurred while processing the request";

        // Server errors
        public const string CriticalCode = "Critical";
        public const string CriticalDescription = "A critical error occurred";

        public const string TimeoutCode = "Timeout";
        public const string TimeoutDescription = "The request timed out";

        public const string ServiceUnavailableCode = "ServiceUnavailable";
        public const string ServiceUnavailableDescription = "The service is currently unavailable";

        // Business logic errors
        public const string InvalidOperationCode = "InvalidOperation";
        public const string InvalidOperationDescription = "The requested operation is invalid";

        public const string DuplicateCode = "Duplicate";
        public const string DuplicateDescription = "The resource already exists";

        public const string BadRequestCode = "BadRequest";
        public const string BadRequestDescription = "The request is invalid or malformed";
    }

    // Implicit conversion for easier error handling
    public static implicit operator string(Error error) => error.Code ?? string.Empty;

    //public static implicit operator Result(Error error) => Result.Failure(error);

    /// <summary>
    /// Creates a general failure error when an unexpected error occurs in the system.
    /// </summary>
    /// <param name="code">Custom error code. If null, uses default "Failure" code.</param>
    /// <param name="description">Custom error description. If null, uses default description.</param>
    /// <returns>An Error instance with ErrorType.Failure</returns>
    /// <example>
    /// <code>
    /// // Example 1: System-wide failure
    /// var error = Error.Failure("System.Fatal", "Critical system failure occurred");
    /// 
    /// // Example 2: Database connection failure
    /// var error = Error.Failure("DB.Connection", "Failed to connect to database");
    /// 
    /// // Example 3: Using default message
    /// var error = Error.Failure();
    /// </code>
    /// </example>
    public static Error Failure(string? code = null, string? description = null)
        => new(
            code ?? DefaultErrors.FailureCode,
            description ?? DefaultErrors.FailureDescription,
            ErrorType.Failure);

    /// <summary>
    /// Creates a validation error when input data fails business rules or data constraints.
    /// Use for invalid input data, format violations, or business rule violations.
    /// </summary>
    /// <param name="code">Custom error code. If null, uses default "Validation" code.</param>
    /// <param name="description">Custom error description. If null, uses default description.</param>
    /// <returns>An Error instance with ErrorType.Validation</returns>
    /// <example>
    /// <code>
    /// // Example 1: Invalid email format
    /// var error = Error.Validation("User.InvalidEmail", "Email format is invalid");
    /// 
    /// // Example 2: Invalid age range
    /// var error = Error.Validation("User.InvalidAge", "Age must be between 18 and 100");
    /// 
    /// // Example 3: Invalid order amount
    /// var error = Error.Validation("Order.InvalidAmount", "Order amount must be greater than zero");
    /// </code>
    /// </example>
    public static Error Validation(string? code = null, string? description = null)
        => new(
            code ?? DefaultErrors.ValidationCode,
            description ?? DefaultErrors.ValidationDescription,
            ErrorType.Validation);

    /// <summary>
    /// Creates an unauthorized error when authentication is required but not provided or is invalid.
    /// Use this when the user is not authenticated at all.
    /// </summary>
    /// <param name="code">Custom error code. If null, uses default "Unauthorized" code.</param>
    /// <param name="description">Custom error description. If null, uses default description.</param>
    /// <returns>An Error instance with ErrorType.Unauthorized</returns>
    /// <example>
    /// <code>
    /// // Example 1: Missing authentication token
    /// var error = Error.Unauthorized("Auth.MissingToken", "Authentication token is required");
    /// 
    /// // Example 2: Expired token
    /// var error = Error.Unauthorized("Auth.ExpiredToken", "Authentication token has expired");
    /// 
    /// // Example 3: Invalid credentials
    /// var error = Error.Unauthorized("Auth.InvalidCredentials", "Username or password is incorrect");
    /// </code>
    /// </example>
    public static Error Unauthorized(string? code = null, string? description = null)
        => new(
            code ?? DefaultErrors.UnauthorizedCode,
            description ?? DefaultErrors.UnauthorizedDescription,
            ErrorType.Unauthorized);

    /// <summary>
    /// Creates a forbidden error when the authenticated user lacks necessary permissions.
    /// Use this when the user is authenticated but doesn't have required access rights.
    /// </summary>
    /// <param name="code">Custom error code. If null, uses default "Forbidden" code.</param>
    /// <param name="description">Custom error description. If null, uses default description.</param>
    /// <returns>An Error instance with ErrorType.Forbidden</returns>
    /// <example>
    /// <code>
    /// // Example 1: Insufficient role
    /// var error = Error.Forbidden("Admin.Access", "Administrator privileges required");
    /// 
    /// // Example 2: Missing document permission
    /// var error = Error.Forbidden("Document.ReadAccess", "User lacks read permission for this document");
    /// 
    /// // Example 3: Resource access denied
    /// var error = Error.Forbidden("Resource.Denied", "Access to this resource is restricted");
    /// </code>
    /// </example>
    public static Error Forbidden(string? code = null, string? description = null)
        => new(
            code ?? DefaultErrors.ForbiddenCode,
            description ?? DefaultErrors.ForbiddenDescription,
            ErrorType.Forbidden);

    /// <summary>
    /// Creates a not found error when the requested resource doesn't exist.
    /// Use this when a requested entity or resource cannot be found in the system.
    /// </summary>
    /// <param name="code">Custom error code. If null, uses default "NotFound" code.</param>
    /// <param name="description">Custom error description. If null, uses default description.</param>
    /// <returns>An Error instance with ErrorType.NotFound</returns>
    /// <example>
    /// <code>
    /// // Example 1: User not found
    /// var error = Error.NotFound("User.NotFound", "User with specified ID does not exist");
    /// 
    /// // Example 2: Product not found
    /// var error = Error.NotFound("Product.NotFound", "Product is no longer available");
    /// 
    /// // Example 3: Order not found
    /// var error = Error.NotFound("Order.NotFound", "Order with given reference number does not exist");
    /// </code>
    /// </example>
    public static Error NotFound(string? code = null, string? description = null)
        => new(
            code ?? DefaultErrors.NotFoundCode,
            description ?? DefaultErrors.NotFoundDescription,
            ErrorType.NotFound);

    /// <summary>
    /// Creates a conflict error when the request conflicts with current system state.
    /// Use this for concurrent modification issues or state conflicts.
    /// </summary>
    /// <param name="code">Custom error code. If null, uses default "Conflict" code.</param>
    /// <param name="description">Custom error description. If null, uses default description.</param>
    /// <returns>An Error instance with ErrorType.Conflict</returns>
    /// <example>
    /// <code>
    /// // Example 1: Concurrent modification
    /// var error = Error.Conflict("Order.Stale", "Order was modified by another user");
    /// 
    /// // Example 2: Stock conflict
    /// var error = Error.Conflict("Stock.Unavailable", "Requested quantity exceeds available stock");
    /// 
    /// // Example 3: Version conflict
    /// var error = Error.Conflict("Document.Version", "Document version mismatch");
    /// </code>
    /// </example>
    public static Error Conflict(string? code = null, string? description = null)
        => new(
            code ?? DefaultErrors.ConflictCode,
            description ?? DefaultErrors.ConflictDescription,
            ErrorType.Conflict);

    /// <summary>
    /// Creates a critical error for severe system errors that require immediate attention.
    /// Use this for system-level failures that affect multiple users or core functionality.
    /// </summary>
    /// <param name="code">Custom error code. If null, uses default "Critical" code.</param>
    /// <param name="description">Custom error description. If null, uses default description.</param>
    /// <returns>An Error instance with ErrorType.Critical</returns>
    /// <example>
    /// <code>
    /// // Example 1: Database failure
    /// var error = Error.Critical("Database.Down", "Database server is not responding");
    /// 
    /// // Example 2: Cache corruption
    /// var error = Error.Critical("Cache.Corrupted", "System cache is corrupted and needs reset");
    /// 
    /// // Example 3: System overload
    /// var error = Error.Critical("System.Overload", "System is experiencing critical load");
    /// </code>
    /// </example>
    public static Error Critical(string? code = null, string? description = null)
        => new(
            code ?? DefaultErrors.CriticalCode,
            description ?? DefaultErrors.CriticalDescription,
            ErrorType.Critical);

    /// <summary>
    /// Creates an invalid operation error when the requested operation is not allowed in the current context.
    /// Use this for business rule violations or invalid state transitions.
    /// </summary>
    /// <param name="code">Custom error code. If null, uses default "InvalidOperation" code.</param>
    /// <param name="description">Custom error description. If null, uses default description.</param>
    /// <returns>An Error instance with ErrorType.Validation</returns>
    /// <example>
    /// <code>
    /// // Example 1: Invalid order cancellation
    /// var error = Error.InvalidOperation("Order.Cancel", "Cannot cancel an already shipped order");
    /// 
    /// // Example 2: Invalid account closure
    /// var error = Error.InvalidOperation("Account.Close", "Cannot close account with pending transactions");
    /// 
    /// // Example 3: Invalid state transition
    /// var error = Error.InvalidOperation("Status.Change", "Cannot change status from Completed to Pending");
    /// </code>
    /// </example>
    public static Error InvalidOperation(string? code = null, string? description = null)
        => new(
            code ?? DefaultErrors.InvalidOperationCode,
            description ?? DefaultErrors.InvalidOperationDescription,
            ErrorType.Validation);

    /// <summary>
    /// Creates a duplicate error when attempting to create a resource that already exists.
    /// Use this for unique constraint violations or duplicate entry attempts.
    /// </summary>
    /// <param name="code">Custom error code. If null, uses default "Duplicate" code.</param>
    /// <param name="description">Custom error description. If null, uses default description.</param>
    /// <returns>An Error instance with ErrorType.Conflict</returns>
    /// <example>
    /// <code>
    /// // Example 1: Duplicate email
    /// var error = Error.Duplicate("User.Email", "Email address is already registered");
    /// 
    /// // Example 2: Duplicate product
    /// var error = Error.Duplicate("Product.SKU", "Product with this SKU already exists");
    /// 
    /// // Example 3: Duplicate order
    /// var error = Error.Duplicate("Order.Reference", "Order reference number already exists");
    /// </code>
    /// </example>
    public static Error Duplicate(string? code = null, string? description = null)
        => new(
            code ?? DefaultErrors.DuplicateCode,
            description ?? DefaultErrors.DuplicateDescription,
            ErrorType.Conflict);

    /// <summary>
    /// Creates a bad request error when the client's request is malformed or invalid.
    /// Use this for request parsing errors or invalid input format.
    /// </summary>
    /// <param name="code">Custom error code. If null, uses default "BadRequest" code.</param>
    /// <param name="description">Custom error description. If null, uses default description.</param>
    /// <returns>An Error instance with ErrorType.Validation</returns>
    /// <example>
    /// <code>
    /// // Example 1: Invalid JSON
    /// var error = Error.BadRequest("Request.InvalidJson", "Request body contains invalid JSON");
    /// 
    /// // Example 2: Invalid query format
    /// var error = Error.BadRequest("Query.InvalidFormat", "Search query format is invalid");
    /// 
    /// // Example 3: Missing required field
    /// var error = Error.BadRequest("Request.MissingField", "Required field 'name' is missing");
    /// </code>
    /// </example>
    public static Error BadRequest(string? code = null, string? description = null)
        => new(
            code ?? DefaultErrors.BadRequestCode,
            description ?? DefaultErrors.BadRequestDescription,
            ErrorType.Validation);
}

public enum ErrorType
{
    Validation,
    Failure,
    Unauthorized,
    Forbidden,
    NotFound,
    Conflict,
    Critical
}
