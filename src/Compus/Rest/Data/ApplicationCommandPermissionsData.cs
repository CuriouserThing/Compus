using System.Collections.Generic;
using Compus.Models;

namespace Compus.Rest.Data
{
    public record ApplicationCommandPermissionsData
    {
        public IReadOnlyList<ApplicationCommandPermissions> Permissions { get; init; }
    }
}
