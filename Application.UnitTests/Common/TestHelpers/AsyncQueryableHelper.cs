using Microsoft.EntityFrameworkCore.Query;
using System.Collections;
using System.Linq.Expressions;

namespace Application.UnitTests.Common.TestHelpers;

public static class AsyncQueryableHelper
{
    public static IQueryable<T> BuildMock<T>(this IEnumerable<T> source)
    {
        return new TestAsyncEnumerable<T>(source);
    }
}

public class TestAsyncEnumerable<T> : EnumerableQuery<T>, IAsyncEnumerable<T>, IQueryable<T>
{
    public IEnumerable<T> InnerEnumerable { get; }

    public TestAsyncEnumerable(IEnumerable<T> enumerable)
        : base(enumerable)
    {
        InnerEnumerable = enumerable;
    }

    public TestAsyncEnumerable(Expression expression)
        : base(expression)
    {
        InnerEnumerable = Enumerable.Empty<T>();
    }

    public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default)
    {
        var rewriter = new ExpressionTreeRewriter();
        var replacedExpression = rewriter.Visit(((IQueryable)this).Expression);
        var lambda = Expression.Lambda<Func<IQueryable<T>>>(replacedExpression);
        var queryable = lambda.Compile()();
        return new TestAsyncEnumerator<T>(queryable.GetEnumerator());
    }

    IQueryProvider IQueryable.Provider => new TestAsyncQueryProvider<T>(this);
}

public class TestAsyncEnumerator<T> : IAsyncEnumerator<T>
{
    private readonly IEnumerator<T> _inner;

    public TestAsyncEnumerator(IEnumerator<T> inner)
    {
        _inner = inner;
    }

    public T Current => _inner.Current;

    public ValueTask<bool> MoveNextAsync()
    {
        return ValueTask.FromResult(_inner.MoveNext());
    }

    public ValueTask DisposeAsync()
    {
        _inner.Dispose();
        return ValueTask.CompletedTask;
    }
}

public class TestAsyncQueryProvider<TEntity> : IAsyncQueryProvider
{
    private readonly IQueryable _queryable;

    public TestAsyncQueryProvider(IQueryable queryable)
    {
        _queryable = queryable;
    }

    public IQueryable CreateQuery(Expression expression)
    {
        var elementType = expression.Type.GetGenericArguments().FirstOrDefault() ?? typeof(TEntity);
        var type = typeof(TestAsyncEnumerable<>).MakeGenericType(elementType);
        return (IQueryable)Activator.CreateInstance(type, expression)!;
    }

    public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
    {
        return new TestAsyncEnumerable<TElement>(expression);
    }

    public object? Execute(Expression expression)
    {
        var rewriter = new ExpressionTreeRewriter();
        var replacedExpression = rewriter.Visit(expression);
        var lambda = Expression.Lambda(replacedExpression);
        return lambda.Compile().DynamicInvoke();
    }

    public TResult Execute<TResult>(Expression expression)
    {
        var rewriter = new ExpressionTreeRewriter();
        var replacedExpression = rewriter.Visit(expression);
        var lambda = Expression.Lambda<Func<TResult>>(replacedExpression);
        return lambda.Compile()();
    }

    public TResult ExecuteAsync<TResult>(Expression expression, CancellationToken cancellationToken = default)
    {
        var expectedResultType = typeof(TResult);
        if (expectedResultType.IsGenericType && expectedResultType.GetGenericTypeDefinition() == typeof(Task<>))
        {
            var itemType = expectedResultType.GetGenericArguments()[0];
            var executeMethod = typeof(TestAsyncQueryProvider<TEntity>)
                .GetMethods()
                .First(m => m.Name == nameof(Execute) && m.IsGenericMethod)
                .MakeGenericMethod(itemType);

            var executionResult = executeMethod.Invoke(this, new object[] { expression });
            var fromResultMethod = typeof(Task)
                .GetMethod(nameof(Task.FromResult))!
                .MakeGenericMethod(itemType);

            return (TResult)fromResultMethod.Invoke(null, new[] { executionResult })!;
        }

        return Execute<TResult>(expression);
    }
}

internal class ExpressionTreeRewriter : ExpressionVisitor
{
    protected override Expression VisitConstant(ConstantExpression node)
    {
        if (node.Value != null)
        {
            var nodeType = node.Value.GetType();
            if (nodeType.IsGenericType && nodeType.GetGenericTypeDefinition() == typeof(TestAsyncEnumerable<>))
            {
                var elementType = nodeType.GetGenericArguments()[0];
                var innerProp = nodeType.GetProperty("InnerEnumerable");
                if (innerProp != null)
                {
                    var innerVal = innerProp.GetValue(node.Value);
                    if (innerVal is IEnumerable enumerable)
                    {
                        var toListMethod = typeof(Enumerable).GetMethods()
                            .First(m => m.Name == nameof(Enumerable.ToList) && m.IsGenericMethod)
                            .MakeGenericMethod(elementType);
                        var list = toListMethod.Invoke(null, new object[] { enumerable });

                        var asQueryableMethod = typeof(Queryable).GetMethods()
                            .First(m => m.Name == nameof(Queryable.AsQueryable) && m.IsGenericMethod)
                            .MakeGenericMethod(elementType);
                        var standardQueryable = asQueryableMethod.Invoke(null, new object?[] { list });

                        return Expression.Constant(standardQueryable);
                    }
                }
            }
        }
        return base.VisitConstant(node);
    }
}
