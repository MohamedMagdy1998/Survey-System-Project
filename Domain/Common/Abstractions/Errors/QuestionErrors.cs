using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Common.Abstractions.Errors;

public static class QuestionErrors
{
    // HTTP 404
    public static readonly Error NotFound =
        Error.NotFound("Question.NotFound", "The requested question was not found.");

    // HTTP 409
    public static readonly Error DuplicateContent =
        Error.Conflict("Question.DuplicateContent", "A question with the same content already exists.");

}
