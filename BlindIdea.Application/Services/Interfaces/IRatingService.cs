using BlindIdea.Application.Dtos;
using System;
using System.Collections.Generic;
using System.Text;

namespace BlindIdea.Application.Services.Interfaces
{
    public interface IRatingService
    {
        Task RateIdeaAsync(Guid ideaId, int value, string userId);
    }
}
