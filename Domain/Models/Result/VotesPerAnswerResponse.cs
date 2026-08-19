using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Models.Result;

public record VotesPerAnswerResponse(
    string Answer,
    int Count
);
