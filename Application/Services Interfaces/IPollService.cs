using Application.DTOs.Requests.Polls;
using Application.DTOs.Responses.Polls;
using Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services_Interfaces;

public interface IPollService
{
    Task<IEnumerable<PollResponse>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<PollResponse?> GetAsync(int id, CancellationToken cancellationToken = default);
    Task<PollResponse> AddAsync(PollRequest request, CancellationToken cancellationToken = default);
    Task<bool> UpdateAsync(int id, PollRequest request, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);
    Task<bool> TogglePublishStatusAsync(int id, CancellationToken cancellationToken = default);
}
