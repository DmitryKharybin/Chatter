namespace AuthenticationApi.Services
{
    public interface IFileHolder
    {
        public Dictionary<string, string> Users { get; set; }
        public Dictionary<string, string> UsersConnections { get; set; }
        
    }
}
