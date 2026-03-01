using BlindIdea.Application.Services.Interfaces;
using BlindIdea.Core.Entities;
using BlindIdea.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace BlindIdea.Application.Services.Implementations
{
   
    public class RatingService : IRatingService
    {
        private readonly AppDbContext _context;

        public RatingService(AppDbContext context)
        {
            _context = context;
        }

        public async Task RateIdeaAsync(Guid ideaId, int value, string userId)
        {
            var alreadyRated = await _context.Ratings
                .AnyAsync(r => r.IdeaId == ideaId && r.UserId == userId);

            if (alreadyRated)
                throw new Exception("User already rated this idea.");
                
            var rating = new Rating
            {
                IdeaId = ideaId,
                UserId = userId,
                Value = value
            };
            _context.Ratings.Add(rating);
            await _context.SaveChangesAsync();
        }
    }
}
