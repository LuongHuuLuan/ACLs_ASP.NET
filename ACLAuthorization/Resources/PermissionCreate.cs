using ACLAuthorization.Helper;
using ACLAuthorization.Models;
using System.Text.Json.Serialization;

namespace ACLAuthorization.Resources
{
    public class PermissionCreate
    {
        public string Resource { get; set; }
        [ValidMethod("method allow is get, post, put, delete")]
        public string Method { get; set; }

        public Permission convertToModel()
        {
            return new Permission
            {
                Resource = this.Resource,
                Method = MethodConverter.ConvertToMethod(Method),
            };
        }
    }
}
