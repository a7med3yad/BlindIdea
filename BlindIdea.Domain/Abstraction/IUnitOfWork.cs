namespace BlindIdea.Domain.Abstraction
{
    public interface IUnitOfWork : IDisposable
    {
        ITeamRepository Teams { get; }
        IIdeaRepository Ideas { get; }
        IRatingRepository Ratings { get; }
        Task<int> SaveChangesAsync();
    }
}