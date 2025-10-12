﻿using Microsoft.AspNetCore.SignalR;

namespace Foraria.Hubs
{
    public class PollHub : Hub
    {
        public override async Task OnConnectedAsync()
        {
            await base.OnConnectedAsync();
            Console.WriteLine($"Cliente conectado: {Context.ConnectionId}");
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            await base.OnDisconnectedAsync(exception);
            Console.WriteLine($"Cliente desconectado: {Context.ConnectionId}");
        }

        // ✅ Enviar una nueva votación a todos los clientes
        public async Task SendNewPoll(object poll)
        {
            Console.WriteLine("📢 Enviando nueva votación a los clientes");
            await Clients.All.SendAsync("NewPollCreated", poll);
        }

        // ✅ Enviar un nuevo voto a todos los clientes
        public async Task SendNewVote(object vote)
        {
            Console.WriteLine("📢 Enviando nuevo voto a los clientes");
            await Clients.All.SendAsync("VoteAdded", vote);
        }
    }
}
