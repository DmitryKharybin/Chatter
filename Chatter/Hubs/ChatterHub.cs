

namespace AuthenticationApi.Hubs
{
    public class ChatterHub : Hub
    {

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
                if (!fileStorage.UsersConnections.ContainsKey(id))
                {
                    fileStorage.Users.Add(this.Context.ConnectionId, id);
                    fileStorage.UsersConnections.Add(id, this.Context.ConnectionId);
                }
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

      
        //Send friend request to another user
        public void SendRequest(string toId, string message)
        {
            if (fileStorage.UsersConnections.Count() > 0 && fileStorage.Users.ContainsKey(this.Context.ConnectionId))
            {
                Clients.Client(fileStorage.UsersConnections[toId]).SendAsync("RequestUpdate", message);
            }
        }



        //When a broadcast is sent , send all clients the message
        public void SendMessageToAll(string message)
        {
            Clients.All.SendAsync("broadcastMessage", message);
        }

        //Send the client a feedback , that a new message is received from username
        public void SendMessage(string toUserId, string senderId, string messageId, Message message)
        {
            //Send to receiver , include the sender id
            Clients.Client(fileStorage.UsersConnections[this.Context.ConnectionId]).SendAsync("newMessage", message);
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
