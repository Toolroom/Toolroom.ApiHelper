namespace Toolroom.ApiHelper
{
    public class ApiError
    {
        public ApiError(ApiErrorType type, string additionalmessage = null)
        {
            Type = type;
            Additionalmessage = additionalmessage;
        }

        public ApiErrorType Type { get; }
        public string Additionalmessage { get; }
    }
}