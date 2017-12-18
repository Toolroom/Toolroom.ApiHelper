namespace Toolroom.ApiHelper
{
    public class ApiRelationshipError : ApiError
    {
        public string RelationshipName { get; }

        public ApiRelationshipError(ApiErrorType type, string relationshipName,
            string additionalmessage = null) : base(type, additionalmessage)
        {
            RelationshipName = relationshipName;
        }
    }
}