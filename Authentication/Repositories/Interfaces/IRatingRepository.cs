using BlindIdea.API.Entities;

namespace BlindIdea.API.Repositories.Interfaces
{
    public interface IRatingRepository:IGenericRepository<Rating>
    {
        Task<Rating?> GetUserRatingAsync(string userId,string ideaId);
        Task<double> GetAverageRatingAsync(string ideaId);
        Task<IEnumerable<Rating>> GetIdeaRatingsAsync (string ideaId);
    }
}
