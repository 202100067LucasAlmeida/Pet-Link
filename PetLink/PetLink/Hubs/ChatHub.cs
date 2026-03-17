using Microsoft.AspNetCore.SignalR;

namespace PetLink.Hubs
{
    // A classe tem de herdar de "Hub" do SignalR
    public class ChatHub : Hub
    {
        // Este é o método que o JavaScript vai chamar quando alguém clicar em "Enviar"
        public async Task SendMessage(string user, string message)
        {
            // O Hub pega na mensagem e envia para TODOS os browsers que estão ligados
            await Clients.All.SendAsync("ReceiveMessage", user, message);
        }
    }
}