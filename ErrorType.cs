namespace MatrixIdent
{
    public class ErrorType
    {
        private string _KEY = "M_UNKNOWN";
        private string _TEXT = "An unknown error has occurred.";

        public ErrorType(string? key, string? text)
        {
            if (key!=null)
                _KEY = key;
            if (text!=null)
                _TEXT = text;
        }

        public override string ToString()
        {
            return System.Text.Json.JsonSerializer.Serialize(new { errcode = _KEY, error = _TEXT });
        }

        public object ToJSON()
        {
            return new { errcode = _KEY, error = _TEXT };
        }

        public static ErrorType M_NOT_FOUND = new ErrorType("M_NOT_FOUND", "The resource requested could not be located.");

        public static ErrorType M_MISSING_PARAMS = new ErrorType("M_MISSING_PARAMS", "The request was missing one or more parameters.");

        public static ErrorType M_INVALID_PARAM = new ErrorType("M_INVALID_PARAM", "The request contained one or more invalid parameters.");

        public static ErrorType M_SESSION_NOT_VALIDATED = new ErrorType("M_SESSION_NOT_VALIDATED", "The session has not been validated.");

        public static ErrorType M_NO_VALID_SESSION = new ErrorType("M_NO_VALID_SESSION", "A session could not be located for the given parameters.");

        public static ErrorType M_SESSION_EXPIRED = new ErrorType("M_SESSION_EXPIRED", "The session has expired and must be renewed.");

        public static ErrorType M_INVALID_EMAIL = new ErrorType("M_INVALID_EMAIL", "The email address provided was not valid.");

        public static ErrorType M_EMAIL_SEND_ERROR = new ErrorType("M_EMAIL_SEND_ERROR", "There was an error sending an email.Typically seen when attempting to verify ownership of a given email address.");

        public static ErrorType M_INVALID_ADDRESS = new ErrorType("M_INVALID_ADDRESS", "The provided third party address was not valid.");

        public static ErrorType M_SEND_ERROR = new ErrorType("M_SEND_ERROR", "There was an error sending a notification. Typically seen when attempting to verify ownership of a given third party address.");

        public static ErrorType M_UNRECOGNIZED = new ErrorType("M_UNRECOGNIZED", "The request contained an unrecognised value, such as an unknown token or medium.");

        public static ErrorType M_THREEPID_IN_USE = new ErrorType("M_THREEPID_IN_USE", "The third party identifier is already in use by another user. Typically this error will have an additional mxid property to indicate who owns the third party identifier.");

        public static ErrorType M_UNKNOWN = new ErrorType(null, null);
    }
}
