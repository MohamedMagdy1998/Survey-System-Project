using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Contracts;

public interface IUnitOfWork : IDisposable
{
    IPollRepository Polls { get; }
    IQuestionRespository Questions { get; }

    IUserRepository Users { get; }

    IVoteRepository Votes { get; }

    IRoleRepository Roles { get; }

    Task<int> CompleteAsync(CancellationToken cancellationToken = default);
}
