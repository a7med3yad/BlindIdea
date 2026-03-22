namespace BlindIdea.Application.Common
{
    public static class CacheDurations
    {
        public static TimeSpan Dashboard => TimeSpan.FromMinutes(5);
        public static TimeSpan Ideas => TimeSpan.FromMinutes(2);
        public static TimeSpan Team => TimeSpan.FromMinutes(30);
        public static TimeSpan TeamMembers => TimeSpan.FromMinutes(10);
        public static TimeSpan UserProfile => TimeSpan.FromHours(1);
        public static TimeSpan TopIdeas => TimeSpan.FromMinutes(5);
    }
}