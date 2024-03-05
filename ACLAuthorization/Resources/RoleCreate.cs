using ACLAuthorization.Helper;
using ACLAuthorization.Models;
using System.Text.Json.Serialization;

namespace ACLAuthorization.Resources
{
    public class RoleCreate
    {
        public string Name { get; set; }
        public ICollection<int>? permissions { get; set; }

        public Role convertToModel(Context context)
        {
            var permissionsEntity = context.permissions.Where(permission => permissions.Contains(permission.Id)).ToList();
            return new Role
            {
                Name = this.Name,
                Permissions = permissionsEntity
            };
        }
    }
}
