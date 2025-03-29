namespace Api;

public interface IFireAndForgetJob<in TContext> where TContext : class
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="context"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task Execute(TContext context, CancellationToken cancellationToken);
}