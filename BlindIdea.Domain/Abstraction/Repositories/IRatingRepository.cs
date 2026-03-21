using BlindIdea.Domain.Entities;

namespace BlindIdea.Domain.Abstraction.Repositories
{
    public interface IRatingRepository:IGenericRepository<Rating>
    {
        Task<Rating?> GetUserRatingAsync(string userId,string ideaId);
        Task<double> GetAverageRatingAsync(string ideaId);
        Task<IEnumerable<Rating>> GetIdeaRatingsAsync (string ideaId);
    }
}
