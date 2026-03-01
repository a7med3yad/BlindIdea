using System;
using System.Collections.Generic;
using System.Text;

namespace BlindIdea.Application.Dtos
{
  
    public class CreateIdeaDto
    {
        public string Title { get; set; } = null!;
        public string Description { get; set; } = null!;
        public bool IsAnonymous { get; set; } = false;
        public Guid? TeamId { get; set; } 
    }
 

}
