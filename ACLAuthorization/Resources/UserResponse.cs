
using ACLAuthorization.Models;
using System.Data;

namespace ACLAuthorization.Resources
{
    public class UserResponse
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public IEnumerable<RoleResponse> Roles { get; set; }

        public UserResponse(User user)
        {
            this.Id = user.Id;
            this.Username = user.Username;
            this.Roles = user.roles.Select(role => new RoleResponse(role)).ToList();
        }
    }
}
