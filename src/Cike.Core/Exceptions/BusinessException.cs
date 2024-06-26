﻿using Microsoft.Extensions.Logging;

namespace Cike.Core.Exceptions;

public class BusinessException : Exception
{
    public string? Code { get; set; }

    public string? Details { get; set; }

    public LogLevel LogLevel { get; set; }

    public BusinessException(
        string? code = null,
        string? message = null,
        string? details = null,
        Exception? innerException = null,
        LogLevel logLevel = LogLevel.Warning)
        : base(message, innerException)
    {
        Code = code;
        Details = details;
        LogLevel = logLevel;
    }
}
