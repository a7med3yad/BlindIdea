using BlindIdea.Domain.Abstraction.Repositories;

namespace BlindIdea.Domain.Abstraction.UnitOfWorks
{
    public interface IUnitOfWork : IDisposable
    {
        ITeamRepository Teams { get; }
        IIdeaRepository Ideas { get; }
        IRatingRepository Ratings { get; }
        IUserTeamRepository UserTeams { get; }
        Task<int> SaveChangesAsync();
    }
}