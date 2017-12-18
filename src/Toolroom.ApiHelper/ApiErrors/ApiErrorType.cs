using System.Net;

namespace Toolroom.ApiHelper
{
    public enum ApiErrorType
    {
        [HttpStatusCode(StatusCode = (int)HttpStatusCode.Forbidden, DefaultMessage = "Insufficient rights")]
        Forbidden,

        [HttpStatusCode(StatusCode = (int)HttpStatusCode.NotFound, DefaultMessage = "Element not found")]
        NotFound,

        [HttpStatusCode(StatusCode = (int)HttpStatusCode.InternalServerError, DefaultMessage = "Internal server error")]
        InternalError,

        [HttpStatusCode(StatusCode = (int)HttpStatusCode.Unauthorized, DefaultMessage = "Credentials invalid")]
        CredentialsAreInvalid,

        [HttpStatusCode(StatusCode = (int)HttpStatusCode.Unauthorized, DefaultMessage = "User is not valid")]
        UserIsNotValid,

        [HttpStatusCode(StatusCode = (int)HttpStatusCode.Unauthorized, DefaultMessage = "Authentication is not active")]
        AuthenticationIsNotActive,

        [HttpStatusCode(StatusCode = (int)HttpStatusCode.Unauthorized, DefaultMessage = "User is locked")]
        UserIsLocked,

        [HttpStatusCode(StatusCode = (int)HttpStatusCode.Unauthorized, DefaultMessage = "Token is invalid")]
        TokenIsInvalid,

        [HttpStatusCode(StatusCode = (int)HttpStatusCode.Unauthorized, DefaultMessage = "Token is expired")]
        TokenIsExpired,

        [HttpStatusCode(StatusCode = (int)HttpStatusCode.Unauthorized, DefaultMessage = "Token is required")]
        TokenIsRequired,

        [HttpStatusCode(StatusCode = (int)HttpStatusCode.NotImplemented, DefaultMessage = "Not implemented")]
        NotImplemented,

        [HttpStatusCode(StatusCode = HttpStatusCodeExtended.UnprocessableEntity, DefaultMessage = "Invalid value")]
        InvalidValue,

        [HttpStatusCode(StatusCode = HttpStatusCodeExtended.UnprocessableEntity, DefaultMessage = "Invalid geo format")]
        InvalidGeoFormat,

        [HttpStatusCode(StatusCode = HttpStatusCodeExtended.UnprocessableEntity, DefaultMessage = "Create requires model with id=0")]
        CreateRequiresModelWithIdZero,

        [HttpStatusCode(StatusCode = HttpStatusCodeExtended.UnprocessableEntity, DefaultMessage = "Update requires model with same id")]
        UpdateRequiresModelWithSameId,

        [HttpStatusCode(StatusCode = HttpStatusCodeExtended.UnprocessableEntity, DefaultMessage = "Is required")]
        IsRequired,

        [HttpStatusCode(StatusCode = HttpStatusCodeExtended.UnprocessableEntity, DefaultMessage = "Must be unique")]
        MustBeUnique,

        [HttpStatusCode(StatusCode = HttpStatusCodeExtended.UnprocessableEntity, DefaultMessage = "May not be changed")]
        MayNotBeChanged,

        [HttpStatusCode(StatusCode = HttpStatusCodeExtended.UnprocessableEntity, DefaultMessage = "Invalid time span")]
        InvalidTimeSpan,
    }
}