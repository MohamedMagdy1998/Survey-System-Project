using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Common.Abstractions.Errors;

public record Error(string Code, string Description, int? StatusCode)
{
    public static readonly Error None = new (String.Empty, String.Empty, default!);

    public static Error BadRequest(string code, string description) =>
        new(code, description, StatusCodes.Status400BadRequest); 

    public static Error Unauthorized(string code, string description) =>
        new(code, description, StatusCodes.Status401Unauthorized); 

    public static Error Forbidden(string code, string description) =>
        new(code, description, StatusCodes.Status403Forbidden); 

    public static Error NotFound(string code, string description) =>
        new(code, description, StatusCodes.Status404NotFound); 

    public static Error Conflict(string code, string description) =>
        new(code, description, StatusCodes.Status409Conflict); 

    public static Error Custom(string code, string description, int statusCode) =>
        new(code, description, statusCode);
}
