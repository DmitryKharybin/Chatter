

namespace AuthenticationApi.Hubs
{
    public class ChatterHub : Hub
    {
        //static Dictionary<string, string> users = new Dictionary<string, string>();
        //static Dictionary<string, string> usersConnections = new Dictionary<string, string>();
        private IFileHolder fileStorage;

        public ChatterHub(IFileHolder fileStorage)
        {
            this.fileStorage = fileStorage;
        }

        //Username must be unique, unique check will be performed
        public void Login(string id)
        {
            if (id == null)
            {
                return;
            }

            if (!fileStorage.Users.ContainsKey(this.Context.ConnectionId))
            {
                fileStorage.Users.Add(this.Context.ConnectionId, id);
                fileStorage.UsersConnections.Add(id, this.Context.ConnectionId);
            }
        }

        //Return true if both username and connectionId deleted succesfully
        public bool Logout(string username)
        {
            if (username == null)
            {
                return false;
            }

            if (fileStorage.UsersConnections.Count() > 0 && fileStorage.Users.ContainsKey(this.Context.ConnectionId))
            {
                return fileStorage.UsersConnections.Remove(fileStorage.Users[this.Context.ConnectionId])
                    && fileStorage.Users.Remove(this.Context.ConnectionId);
            }

            return false;
        }

        //If new Friend reqeust made , send to client the new reqeust id
        //The client can use the id to fetch this new reqeust, in order to render data more efficiently
        //public void SendRequest(string toUsername, int requestId)
        //{
        //    Clients.Client(usersConnections[toUsername]).SendAsync("newFriendRequest", requestId);
        //}

        public void SendRequest(string toId, string message)
        {
            if (fileStorage.UsersConnections.Count() > 0 && fileStorage.Users.ContainsKey(this.Context.ConnectionId))
            {
                Clients.Client(fileStorage.UsersConnections[toId]).SendAsync("RequestUpdate",message);
            }
        }



        //When a broadcast is sent , send all clients the message
        public void SendMessageToAll(string message)
        {
            Clients.All.SendAsync("broadcastMessage", message);
        }

        //Send the client a feedback , that a new message is received from username
        public void SendMessage(string toUserId, string senderId)
        {
            //Send to receiver , include the sender id
            Clients.Client(fileStorage.UsersConnections[this.Context.ConnectionId]).SendAsync("newMessage", senderId);
        }

        //On connection termination , delete username & connectionId
        public override Task OnDisconnectedAsync(Exception? exception)
        {
            if (fileStorage.UsersConnections.Count() > 0 && fileStorage.Users.ContainsKey(this.Context.ConnectionId))
            {
                Logout(fileStorage.Users[this.Context.ConnectionId]);
            }

            return base.OnDisconnectedAsync(exception);
        }
    }
}
