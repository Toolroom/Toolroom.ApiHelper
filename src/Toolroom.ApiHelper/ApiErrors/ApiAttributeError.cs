namespace Toolroom.ApiHelper
{
    public class ApiAttributeError : ApiError
    {
        public string AttributeName { get; }

        public ApiAttributeError(ApiErrorType type, string attributeName,
            string additionalmessage = null) : base(type, additionalmessage)
        {
            AttributeName = attributeName;
        }
    }
}