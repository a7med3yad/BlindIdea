namespace BlindIdea.Application.Common
{
    public static class CacheKeys
    {
        // Dashboard
        public static string Dashboard(string teamId)
            => $"dashboard:{teamId}";

        // Ideas
        public static string TeamIdeas(string teamId)
            => $"ideas:{teamId}";

        // Team
        public static string Team(string teamId)
            => $"team:{teamId}";

        // Team Members
        public static string TeamMembers(string teamId)
            => $"team:members:{teamId}";

        // User Profile
        public static string UserProfile(string userId)
            => $"profile:{userId}";

        // Top Ideas
        public static string TopIdeas(string teamId)
            => $"ideas:top:{teamId}";
    }
}