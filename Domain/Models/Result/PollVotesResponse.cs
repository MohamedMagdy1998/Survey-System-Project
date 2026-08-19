using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Responses.Result;

public record PollVotesResponse(string Title,IEnumerable<VoteResponse> Votes);
