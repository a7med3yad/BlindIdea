using BlindIdea.Core.Entities;

namespace BlindIdea.Core.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        IRepository<User> Users { get; }
        IRepository<Team> Teams { get; }
        IRepository<Idea> Ideas { get; }
        IRepository<Rating> Ratings { get; }

        Task<int> CommitAsync();
    }
}
