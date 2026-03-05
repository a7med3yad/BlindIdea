namespace BlindIdea.Core.Constants
{
    
    public static class AppConstants
    {
        
        public const string ROLE_ADMIN = "Admin";

        public const string ROLE_TEAM_ADMIN = "TeamAdmin";

        public const string ROLE_USER = "User";

        public const int PASSWORD_MIN_LENGTH = 8;

        public const int PASSWORD_MAX_LENGTH = 256;

        public const int NAME_MIN_LENGTH = 2;

        public const int NAME_MAX_LENGTH = 100;

        public const int TEAM_NAME_MIN_LENGTH = 3;

        public const int TEAM_NAME_MAX_LENGTH = 100;

        public const int IDEA_TITLE_MIN_LENGTH = 5;

        public const int IDEA_TITLE_MAX_LENGTH = 200;

        public const int IDEA_DESCRIPTION_MIN_LENGTH = 10;

        public const int IDEA_DESCRIPTION_MAX_LENGTH = 2000;

        public const int RATING_MIN = 1;

        public const int RATING_MAX = 5;

        public const int ACCESS_TOKEN_EXPIRY_MINUTES = 15;

        public const int REFRESH_TOKEN_EXPIRY_DAYS = 7;

        public const int REFRESH_TOKEN_EXPIRY_DAYS_REMEMBER = 30;

        public const int EMAIL_VERIFICATION_TOKEN_EXPIRY_HOURS = 24;

        public const string EMAIL_FROM_ADDRESS = "noreply@blindidea.com";

        public const string EMAIL_FROM_NAME = "BlindIdea";

        public const string ERROR_CODE_VALIDATION = "ERR_VALIDATION";

        public const string ERROR_CODE_UNAUTHORIZED = "ERR_UNAUTHORIZED";

        public const string ERROR_CODE_FORBIDDEN = "ERR_FORBIDDEN";

        public const string ERROR_CODE_NOT_FOUND = "ERR_NOT_FOUND";

        public const string ERROR_CODE_CONFLICT = "ERR_CONFLICT";

        public const string ERROR_CODE_SERVER_ERROR = "ERR_SERVER_ERROR";

        public const string CLAIM_TYPE_ROLE = "role";

        public const string CLAIM_TYPE_JWT_ID = "jti";

        public const string CLAIM_TYPE_TEAM_ID = "team_id";

        public const string CLAIM_TYPE_EMAIL_VERIFIED = "email_verified";
    }
}