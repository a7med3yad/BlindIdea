namespace BlindIdea.Core.Constants
{
    /// <summary>
    /// Application-wide constants for roles, error messages, and validation rules.
    /// Centralized definitions to prevent magic strings and ensure consistency.
    /// </summary>
    public static class AppConstants
    {
        // ===== ROLES =====

        /// <summary>
        /// Administrator role with full system access.
        /// </summary>
        public const string ROLE_ADMIN = "Admin";

        /// <summary>
        /// Team Administrator role with team-specific permissions.
        /// Can manage team members and team ideas.
        /// </summary>
        public const string ROLE_TEAM_ADMIN = "TeamAdmin";

        /// <summary>
        /// Regular user role with standard permissions.
        /// Can create ideas, rate ideas, join teams.
        /// </summary>
        public const string ROLE_USER = "User";

        // ===== VALIDATION =====

        /// <summary>
        /// Minimum password length (must match AspNetCore Identity requirements).
        /// </summary>
        public const int PASSWORD_MIN_LENGTH = 8;

        /// <summary>
        /// Maximum password length.
        /// </summary>
        public const int PASSWORD_MAX_LENGTH = 256;

        /// <summary>
        /// Minimum name length.
        /// </summary>
        public const int NAME_MIN_LENGTH = 2;

        /// <summary>
        /// Maximum name length.
        /// </summary>
        public const int NAME_MAX_LENGTH = 100;

        /// <summary>
        /// Minimum team name length.
        /// </summary>
        public const int TEAM_NAME_MIN_LENGTH = 3;

        /// <summary>
        /// Maximum team name length.
        /// </summary>
        public const int TEAM_NAME_MAX_LENGTH = 100;

        /// <summary>
        /// Minimum idea title length.
        /// </summary>
        public const int IDEA_TITLE_MIN_LENGTH = 5;

        /// <summary>
        /// Maximum idea title length.
        /// </summary>
        public const int IDEA_TITLE_MAX_LENGTH = 200;

        /// <summary>
        /// Minimum idea description length.
        /// </summary>
        public const int IDEA_DESCRIPTION_MIN_LENGTH = 10;

        /// <summary>
        /// Maximum idea description length.
        /// </summary>
        public const int IDEA_DESCRIPTION_MAX_LENGTH = 2000;

        /// <summary>
        /// Minimum rating value (stars).
        /// </summary>
        public const int RATING_MIN = 1;

        /// <summary>
        /// Maximum rating value (stars).
        /// </summary>
        public const int RATING_MAX = 5;

        // ===== TOKEN DURATIONS =====

        /// <summary>
        /// Access token expiration time in minutes.
        /// Should be short-lived (15 minutes recommended).
        /// </summary>
        public const int ACCESS_TOKEN_EXPIRY_MINUTES = 15;

        /// <summary>
        /// Refresh token expiration time in days (default).
        /// Can be extended for "Remember Me" functionality.
        /// </summary>
        public const int REFRESH_TOKEN_EXPIRY_DAYS = 7;

        /// <summary>
        /// Refresh token expiration time in days (extended).
        /// Used for "Remember Me" or "Stay Logged In" feature.
        /// </summary>
        public const int REFRESH_TOKEN_EXPIRY_DAYS_REMEMBER = 30;

        /// <summary>
        /// Email verification token expiration time in hours.
        /// </summary>
        public const int EMAIL_VERIFICATION_TOKEN_EXPIRY_HOURS = 24;

        // ===== EMAIL =====

        /// <summary>
        /// Email address format for "From" in email notifications.
        /// </summary>
        public const string EMAIL_FROM_ADDRESS = "noreply@blindidea.com";

        /// <summary>
        /// Display name for email "From" field.
        /// </summary>
        public const string EMAIL_FROM_NAME = "BlindIdea";

        // ===== ERROR CODES =====

        /// <summary>
        /// Generic validation error code.
        /// </summary>
        public const string ERROR_CODE_VALIDATION = "ERR_VALIDATION";

        /// <summary>
        /// Authentication failure error code.
        /// </summary>
        public const string ERROR_CODE_UNAUTHORIZED = "ERR_UNAUTHORIZED";

        /// <summary>
        /// Authorization failure error code.
        /// </summary>
        public const string ERROR_CODE_FORBIDDEN = "ERR_FORBIDDEN";

        /// <summary>
        /// Not found error code.
        /// </summary>
        public const string ERROR_CODE_NOT_FOUND = "ERR_NOT_FOUND";

        /// <summary>
        /// Entity already exists error code.
        /// </summary>
        public const string ERROR_CODE_CONFLICT = "ERR_CONFLICT";

        /// <summary>
        /// Server error code.
        /// </summary>
        public const string ERROR_CODE_SERVER_ERROR = "ERR_SERVER_ERROR";

        // ===== CLAIM TYPES =====

        /// <summary>
        /// Custom claim type for user roles.
        /// </summary>
        public const string CLAIM_TYPE_ROLE = "role";

        /// <summary>
        /// Claim type for JWT ID (jti).
        /// </summary>
        public const string CLAIM_TYPE_JWT_ID = "jti";

        /// <summary>
        /// Claim type for team ID.
        /// </summary>
        public const string CLAIM_TYPE_TEAM_ID = "team_id";

        /// <summary>
        /// Claim type for email verified status.
        /// </summary>
        public const string CLAIM_TYPE_EMAIL_VERIFIED = "email_verified";
    }
}
