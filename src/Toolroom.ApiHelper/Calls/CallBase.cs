using System.Collections.Generic;
using System.Threading.Tasks;

namespace Toolroom.ApiHelper
{
    public abstract class CallBase<TData, TPermissionParameter> : IErrorCollection where TPermissionParameter : IPermissionValidation
    {
        public TPermissionParameter PermissionParameter { get; }

        private readonly CallResult<TData> _result = new CallResult<TData>();

        #region Errors

        public void AddParameterError(ApiErrorType type, string parameter, string additionalmessage = null)
        {
            AddError(new ApiParameterError(type, parameter, additionalmessage));
        }

        public void AddRelationError(ApiErrorType type, string relationshipName, string additionalmessage = null)
        {
            AddError(new ApiRelationshipError(type, relationshipName, additionalmessage));
        }

        public void AddAttributeError(ApiErrorType type, string attributeName,
            string additionalmessage = null)
        {
            AddError(new ApiAttributeError(type, attributeName, additionalmessage));
        }

        public void AddError(ApiErrorType type, string additionalmessage = null)
        {
            AddError(new ApiError(type, additionalmessage));
        }

        protected void AddError(ApiError error)
        {
            _result.AddError(error);
        }
        public bool HasErrors => _result.HasErrors;
        public IEnumerable<ApiError> Errors => _result.Errors;
        #endregion

        public virtual async Task<CallResult<TData>> ExecuteAsync()
        {
            if (HasErrors)
            {
                return _result;
            }
            bool isPermitted = await PermissionParameter.IsPermitted();
            if (!isPermitted)
            {
                return _result;
            }
            _result.Data = await GetData();
            return _result;
        }

        protected abstract Task<TData> GetData();

        protected CallBase(TPermissionParameter permissionParameter)
        {
            PermissionParameter = permissionParameter;
        }
    }
}
