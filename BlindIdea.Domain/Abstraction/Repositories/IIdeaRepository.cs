using BlindIdea.Domain.Entities;

namespace BlindIdea.Domain.Abstraction.Repositories
{
    public interface IIdeaRepository:IGenericRepository<Idea>
    {
        Task<IEnumerable<Idea>> GetTeamIdeaAsync(string teamId);
        Task<Idea?> GetIdeaWithRatingsAsync(string ideaId);
        Task<IEnumerable<Idea>> GetTopRatedIdeasAsync(string teamId,int count = 5);
    }
}
