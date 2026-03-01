using BlindIdea.Application.Dtos;
using System;
using System.Collections.Generic;
using System.Text;

namespace BlindIdea.Application.Services.Interfaces
{
    public interface IIdeaService
    {
        Task<Guid> CreateAsync(CreateIdeaDto dto, string userId);
        Task<IdeaDto?> GetByIdAsync(Guid id);
        Task<List<IdeaDto>> GetAllAsync();
        Task RateAsync(Guid ideaId, int value, string userId);
    }

}
