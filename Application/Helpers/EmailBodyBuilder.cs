using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Application.Helpers;

public static class EmailBodyBuilder
{
    public static string GenerateEmailBody(string template, Dictionary<string, string> templateModel)
    {
        var fileName = $"{template}.html";

        // Candidate paths to check in order:
        var candidatePaths = new[]
        {
            // 1. Output directory (bin/Debug/net8.0/Templates/...)
            Path.Combine(AppContext.BaseDirectory, "Templates", fileName),

            // 2. Project directory during development
            Path.Combine(Directory.GetCurrentDirectory(), "Templates", fileName),

            // 3. Solution Application layer folder
            Path.Combine(Directory.GetCurrentDirectory(), "..", "Application", "Templates", fileName),
            
            // 4. In case execution started from bin folder
            Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "Application", "Templates", fileName)
        };

        string? templatePath = null;

        foreach (var path in candidatePaths)
        {
            var fullPath = Path.GetFullPath(path);
            if (File.Exists(fullPath))
            {
                templatePath = fullPath;
                break;
            }
        }

        if (templatePath is null)
        {
            throw new FileNotFoundException(
                $"Template file '{fileName}' was not found. Searched in: \n" +
                string.Join("\n", candidatePaths.Select(Path.GetFullPath))
            );
        }

        using var streamReader = new StreamReader(templatePath);
        var body = streamReader.ReadToEnd();

        foreach (var (key, value) in templateModel)
        {
            body = body.Replace(key, value);
        }

        return body;
    }
}
