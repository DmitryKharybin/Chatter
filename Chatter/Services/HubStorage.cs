namespace AuthenticationApi.Services
{
    public class HubStorage : IFileHolder
    {
        public Dictionary<string, string> Users { get; set; }
        public Dictionary<string, string> UsersConnections { get; set; }

        public HubStorage()
        {
            Users = new Dictionary<string, string>();
            UsersConnections = new Dictionary<string, string>();
        }
    }


}
