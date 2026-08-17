using Domain.Common.Abstractions;
using Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Contracts;

public interface IQuestionRespository
{

    public Task<IEnumerable<Question>> GetAllAsync(int pollId, CancellationToken cancellationToken = default);

    public Task<IEnumerable<Question>> GetAvailableAsync(int pollId,string userId , CancellationToken cancellationToken = default);

    public Task<IEnumerable<int>> GetActiveQuestionIdsAsync(int pollId, CancellationToken cancellationToken = default);

    public Task<bool> ExistsAsync(Expression<Func<Question, bool>> predicate, CancellationToken cancellationToken = default);

    public Task AddAsync(Question request, CancellationToken cancellationToken = default);

    public Task<Question?> GetAsync(int pollId, int id, CancellationToken cancellationToken = default);

    public Task UpdateAsync(Question question, CancellationToken cancellationToken = default);


    public Task<Result> ToggleStatusAsync(int pollId, int id, CancellationToken cancellationToken = default);



}
