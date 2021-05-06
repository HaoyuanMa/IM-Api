using IM_Api.Db;
using IM_Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace IM_Api.Hubs
{

    //[EnableCors("any")]
    //[Authorize]
    public class MessageHub : Hub
    {

        private UserManager<IdentityUser> userManager;
        
        private UserDbContext dbcontext;



        public MessageHub(UserDbContext _context, UserManager<IdentityUser> _userManager)
        {
            this.userManager = _userManager;
            this.dbcontext = _context;
        }

        public override Task OnConnectedAsync()
        {
            var email = Context.User.Identity.Name;
            if (!Channels.DataChannels.ContainsKey(email))
            {
                Channels.DataChannels.Add(email, Channel.CreateBounded<string>(new BoundedChannelOptions(1000)
                {
                    FullMode = BoundedChannelFullMode.DropOldest
                }));
            }
            return base.OnConnectedAsync();
        }

 

        public override Task OnDisconnectedAsync(Exception exception)
        {
            var email = Context.User.Identity.Name;
            if (Channels.DataChannels.ContainsKey(email))
            {
                Channels.DataChannels.Remove(email);
            }
                Clients.All.SendAsync("RemoveUser", email);
            if (UsersList.ChatUsers.Contains(email))
            {
                UsersList.ChatUsers.Remove(email);
            }
            if (UsersList.BroadcastUsers.Contains(email))
            {
                UsersList.BroadcastUsers.Remove(email);
            }
            if (UsersList.ChatRoomUsers.Contains(email))
            {
                UsersList.ChatRoomUsers.Remove(email);
            }
            Groups.RemoveFromGroupAsync(Context.ConnectionId, "mygroup");
            return base.OnDisconnectedAsync(exception);
        }

        public async Task SetOnline(string type)
        {
            string email = Context.User.Identity.Name;

            List<string> curuser = new List<string>();
            curuser.Add(email);


            if (type == "chat")
            {
                List<string> users = UsersList.ChatUsers;
                
                if (!UsersList.ChatUsers.Contains(email))
                {
                    await Clients.All.SendAsync("GetChatUsers", curuser);
                    await Clients.Caller.SendAsync("GetChatUsers", users);
                    UsersList.ChatUsers.Add(email);
                }
            }
            else if(type == "broadcast")
            {
                List<string> users = UsersList.BroadcastUsers;
               
                if (!UsersList.BroadcastUsers.Contains(email))
                {
                    await Clients.All.SendAsync("GetBroadcastUsers", curuser);
                    await Clients.Caller.SendAsync("GetBroadcastUsers", users);
                    UsersList.BroadcastUsers.Add(email);
                }
                
            }
            else
            {
                List<string> users = UsersList.ChatRoomUsers;
                
                if (!UsersList.ChatRoomUsers.Contains(email))
                {
                    await Clients.All.SendAsync("GetChatRoomUsers", curuser);
                    await Clients.Caller.SendAsync("GetChatRoomUsers", users);
                    UsersList.ChatRoomUsers.Add(email);
                }
                await Groups.AddToGroupAsync(Context.ConnectionId, "mygroup");
            }         
        }


        public async Task SendMessage(Message message)
         {
            //var target = userManager.FindByEmailAsync(message.To);
            string name = Context.User.Identity.Name;
            //Clients.All.SendAsync("ReceiveMessage", message);
            if(message.Type == "chatroom")
                await Clients.Group("mygroup").SendAsync("ReceiveMessage", message);
            else
                await Clients.Users(message.To).SendAsync("ReceiveMessage", message);
        }

        public async Task UploadFile(IAsyncEnumerable<BinData> stream)
        {
            var path = Directory.GetCurrentDirectory();
            FileStream fileStream = null;
            BinaryWriter binaryWriter = null;
            await foreach (var item in stream)
            {
                if(item.Order == 0)
                {
                    var dir = path + "\\wwwroot\\UploadFiles\\" + item.From;
                    //System.Diagnostics.Debug.WriteLine(dir);
                    if (!Directory.Exists(dir)) ;
                    {
                        Directory.CreateDirectory(dir);
                    }
                    var file = dir + "\\" + item.Name;
                    fileStream = new FileStream(file, FileMode.Append, FileAccess.Write);
                    binaryWriter = new BinaryWriter(fileStream);
                }
                var str = item.Data.Split(',')[1];
                byte[] bytes = Convert.FromBase64String(str);
                binaryWriter.Write(bytes);
            }
            binaryWriter.Close();
            fileStream.Close();
            return;
        }

        public async IAsyncEnumerable<string> DownLoadStream(int delay, [EnumeratorCancellation]CancellationToken cancellationToken)
        {
            string email = Context.User.Identity.Name;
            while (await Channels.DataChannels[email].Reader.WaitToReadAsync())
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (Channels.DataChannels[email].Reader.TryRead(out var message))
                {
                    System.Diagnostics.Debug.WriteLine("send: " + message);
                    yield return message;
                }
                //await Task.Delay(delay, cancellationToken);
            }

        }

        public async Task UploadStream(IAsyncEnumerable<string> stream)
        {
            await foreach (var item in stream)
            {
                //System.Diagnostics.Debug.WriteLine("recevie: " + item);
                foreach(var user in Channels.DataChannels.Keys)
                {
                    await Channels.DataChannels[user].Writer.WriteAsync(item);
                }
                
            }
        }


    }


}

