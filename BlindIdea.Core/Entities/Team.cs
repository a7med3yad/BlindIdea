namespace BlindIdea.Core.Entities;

public class Team 
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string AdminId { get; set; } = null!;
    public virtual User Admin { get; set; } = null!;
    public string? Description { get; set; }
    public IReadOnlyCollection<User> Members = new List<User>();
    public IReadOnlyCollection<Idea> Ideas = new List<Idea>();

    public bool? IsDeleted { get; set; } = false;
    public bool IsAdmin { get; set; }

    public bool IsMember {  get; set; }
}
