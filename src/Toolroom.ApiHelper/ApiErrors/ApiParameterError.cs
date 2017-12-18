namespace Toolroom.ApiHelper
{
    public class ApiParameterError : ApiError
    {
        public string Parameter { get; }

        public ApiParameterError(ApiErrorType type, string parameter,
            string additionalmessage = null) : base(type, additionalmessage)
        {
            Parameter = parameter;
        }
    }
}