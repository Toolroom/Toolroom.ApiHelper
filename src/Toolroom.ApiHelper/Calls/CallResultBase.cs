using System.Collections.Generic;
using System.Linq;

namespace Toolroom.ApiHelper
{
    public class CallResultBase
    {
        private readonly IList<ApiError> _errors = new List<ApiError>();
        public IEnumerable<ApiError> Errors => _errors;

        public bool HasErrors => _errors.Any();

        public void AddError(ApiError error)
        {
            _errors.Add(error);
        }
    }
}