using System.Threading.Tasks;

namespace Toolroom.ApiHelper
{
    public interface IPermissionValidation
    {
        Task<bool> IsPermitted();
    }
}