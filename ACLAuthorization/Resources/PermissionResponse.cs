using ACLAuthorization.Helper;
using ACLAuthorization.Models;

namespace ACLAuthorization.Resources
{
    public class PermissionResponse
    {
        public int Id { get; set; }
        public string Resource { get; set; }
        public string Method { get; set; }

        public PermissionResponse(Permission permission)
        {
            this.Id = permission.Id;
            this.Resource = permission.Resource;
            this.Method = MethodConverter.ConvertToString(permission.Method);
        }
    }
}
