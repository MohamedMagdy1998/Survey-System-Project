using Domain.Common.Abstractions;
using Domain.Common.Abstractions.Errors;
using Domain.Contracts;
using Domain.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure_Layer.Implementations;

public class QuestionRepository : IQuestionRespository
{
    private readonly ApplicationDbContext Context;

    public QuestionRepository(ApplicationDbContext context)
    {
        Context = context;
    }

    public async Task AddAsync(Question question, CancellationToken cancellationToken = default)
    {

        await Context.Questions.AddAsync(question, cancellationToken);
        await Context.SaveChangesAsync(cancellationToken);
    }


    public async Task<bool> ExistsAsync(Expression<Func<Question, bool>> predicate, CancellationToken cancellationToken = default) =>
        await Context.Questions.AnyAsync(predicate, cancellationToken);

    public async Task<IEnumerable<Question>> GetAllAsync(int pollId, CancellationToken cancellationToken = default) =>
        await Context.Questions.Where(q => q.PollId == pollId).Include(q => q.Answers).AsNoTracking().ToListAsync(cancellationToken);

    public async Task<Question?> GetAsync(int pollId, int id, CancellationToken cancellationToken = default)
    {
        return await Context.Questions.Include(q => q.Answers).AsNoTracking().FirstOrDefaultAsync(q => q.PollId == pollId && q.Id == id, cancellationToken);
    }


    public async Task UpdateAsync(Question question, CancellationToken cancellationToken = default)
    {
        Context.Questions.Update(question);
    }

    public async Task<Result> ToggleStatusAsync(int pollId, int id, CancellationToken cancellationToken = default)
    {
        var question = await Context.Questions.FirstOrDefaultAsync(q => q.PollId == pollId && q.Id == id, cancellationToken);
        
        if (question is null)
            return QuestionErrors.NotFound;

        question.IsActive = !question.IsActive;

        Context.Questions.Update(question);

        return Result.Success();
    }


}
