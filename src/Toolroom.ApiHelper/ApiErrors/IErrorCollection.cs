using System.Collections.Generic;

namespace Toolroom.ApiHelper
{
    public interface IErrorCollection
    {
        void AddParameterError(ApiErrorType type, string parameter, string additionalmessage = null);
        void AddRelationError(ApiErrorType type, string relationshipName, string additionalmessage = null);
        void AddAttributeError(ApiErrorType type, string attributeName, string additionalmessage = null);
        void AddError(ApiErrorType type, string additionalmessage = null);
        bool HasErrors { get; }
        IEnumerable<ApiError> Errors { get; }
    }
}