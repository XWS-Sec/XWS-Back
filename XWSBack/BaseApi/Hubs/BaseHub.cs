using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using BaseApi.CustomAttributes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using SignalRSwaggerGen.Attributes;

namespace BaseApi.Hubs
{
    public class BaseHub : Hub
    {
        public static readonly ConcurrentDictionary<Guid, ConnectedUser> connections = new ConcurrentDictionary<Guid, ConnectedUser>();

        public async Task Connect(Guid userId)
        {
            var user = connections.GetOrAdd(userId, _ =>
            new ConnectedUser()
            {
                ConnectionIds = new List<string>()
            });

            lock (user.ConnectionIds)
            {
                user.ConnectionIds.Add(Context.ConnectionId);
            }

            await Clients.Caller.SendAsync("Response", $"Successfully connected!");
        }
    }
}