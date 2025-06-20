namespace ChatAI_Assistant.Shared.Enums;

public enum AIErrorCode
{
    None = 0,
    InvalidRequest = 1000,
    AuthenticationFailed = 1001,
    RateLimitExceeded = 1002,
    ModelNotAvailable = 1003,
    ContentFiltered = 1004,
    ContextTooLong = 1005,
    TimeoutExpired = 1006,
    ServiceUnavailable = 1007,
    InsufficientTokens = 1008,
    InvalidModel = 1009,
    NetworkError = 2000,
    ParseError = 2001,
    UnknownError = 9999
}