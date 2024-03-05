using ACLAuthorization.Helper;
using System.Text.Json.Serialization;

namespace ACLAuthorization.Models
{
    public enum Method
    {
        get,
        post,
        put,
        patch,
        delete,
    }

    public class Permission
    {
        public int Id { get; set; }
        public string Resource { get; set; }
        public Method Method { get; set; }
        public ICollection<Role> roles { get; set; } = new List<Role>();
    }
}
