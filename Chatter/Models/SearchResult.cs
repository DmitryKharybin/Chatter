namespace AuthenticationApi.Models
{
    //This class will contain the search result. 
    public class SearchResult
    {
        public IEnumerable<User>? Users { get; set; }

        public IEnumerable<Group>? Groups { get; set; }
    }
}
