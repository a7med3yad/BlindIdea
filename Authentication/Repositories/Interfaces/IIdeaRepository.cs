using BlindIdea.API.Entities;

namespace BlindIdea.API.Repositories.Interfaces
{
    public interface IIdeaRepository:IGenericRepository<Idea>
    {
        Task<IEnumerable<Idea>> GetTeamIdeaAsync(string teamId);
        Task<Idea?> GetIdeaWithRatingsAsync(string ideaId);
        Task<IEnumerable<Idea>> GetTopRatedIdeasAsync(string teamId,int count = 5);
    }
}
