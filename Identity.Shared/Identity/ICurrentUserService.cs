using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Identity.Shared.Identity
{
    public interface ICurrentUserService
    {
        Guid? GetCurrentUserId();
        string GetCurrentUserName();
        string GetCurrentUserRole();
        string GetCurrentCulture();
        bool IsAuthenticated();
        int? GetCurrentUserCountryId();
    }
}
