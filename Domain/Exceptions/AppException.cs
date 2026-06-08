namespace Domain.Exceptions;

public abstract class AppException : Exception
{
    public int StatusCode { get; }

    protected AppException(string message, int statusCode) : base(message)
    {
        StatusCode = statusCode;
    }
}

public class NotFoundException : AppException
{
    public NotFoundException(string entity, object key)
        : base($"'{entity}' с идентификатором '{key}' не найден.", 404) { }

    public NotFoundException(string message) : base(message, 404) { }
}

public class ValidationException : AppException
{
    public ValidationException(string message) : base(message, 400) { }
}

public class UnauthorizedException : AppException
{
    public UnauthorizedException(string message = "Необходима авторизация.") : base(message, 401) { }

    public UnauthorizedException(string message, int statusCode) : base(message, statusCode) { }
}

public class ForbiddenException : AppException
{
    public ForbiddenException(string message = "У вас нет прав для выполнения этого действия.") : base(message, 403) { }
}

public class BusinessRuleException : AppException
{
    public BusinessRuleException(string message) : base(message, 409) { }
}
