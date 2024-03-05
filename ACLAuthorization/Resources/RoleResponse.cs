using ACLAuthorization.Models;

namespace ACLAuthorization.Resources
{
    public class RoleResponse
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public ICollection<PermissionResponse> Permissions { get; set; } = new List<PermissionResponse>();

        public RoleResponse(Role role)
        {
            this.Id = role.Id;
            this.Name = role.Name;
            this.Permissions = role.Permissions.Select(permissions => new PermissionResponse(permissions)).ToList();
        }
    }
}
