using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Requests.Votes;

public record VoteAnswerRequest(
    int QuestionId,
    int AnswerId
);