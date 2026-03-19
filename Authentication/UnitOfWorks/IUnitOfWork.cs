using BlindIdea.API.Repositories.Interfaces;

namespace BlindIdea.API.UnitOfWorks
{
    public interface IUnitOfWork : IDisposable
    {
        ITeamRepository Teams { get; }
        IIdeaRepository Ideas { get; }
        IRatingRepository Ratings { get; }
        Task<int> SaveChangesAsync();
    }
}