namespace ACLAuthorization.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string password {  get; set; }
        public ICollection<Role> roles { get; set; } = new List<Role>();

        public string RolesToString()
        {
            return string.Join(",", roles.Select(role => role.Name).ToList());
        }
    }
}
