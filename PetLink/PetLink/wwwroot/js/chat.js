"use strict";

// 1. Criar a ligação ao Hub
const connection = new signalR.HubConnectionBuilder()
    .withUrl("/chatHub")
    .build();

// Desativar o botão de enviar até a ligação estar estabelecida
document.getElementById("sendButton").disabled = true;

// 2. O que fazer quando RECEBEMOS uma mensagem
connection.on("ReceiveMessage", function (user, message) {
    const chatBox = document.getElementById("chatBox");

    // Criar o balão de mensagem (Estilo Recebido - Branco)
    const msgHtml = `
        <div class="message-sent d-flex justify-content-end mb-3">
            <div class="message-bubble text-white p-3 rounded-4 shadow-sm" 
                style="max-width: 85%; background-color: var(--petlink-primary);">
                <div class="fw-bold small text-primary-white mb-1">${user}</div>
                <p class="mb-0 small">${message}</p>
            </div>
        </div>`;

    chatBox.innerHTML += msgHtml;
    chatBox.scrollTop = chatBox.scrollHeight; // Fazer scroll automático para o fundo
});

// 3. Iniciar a ligação
connection.start().then(function () {
    document.getElementById("sendButton").disabled = false;
}).catch(function (err) {
    return console.error(err.toString());
});

// 4. O que fazer quando CLICAMOS NO BOTÃO de Enviar
document.getElementById("sendButton").addEventListener("click", function (event) {
    const user = "You"; // Mais tarde será com o nome do utilizador
    const messageInput = document.getElementById("messageInput");
    const message = messageInput.value;

    if (message.trim() === "") return; // Não enviar mensagens vazias

    // Enviar para o C#
    connection.invoke("SendMessage", user, message).catch(function (err) {
        return console.error(err.toString());
    });

    // Limpar a caixa de texto
    messageInput.value = "";
    event.preventDefault();
});

// (Extra) Enviar mensagem ao carregar no "Enter"
document.getElementById("messageInput").addEventListener("keypress", function (e) {
    if (e.key === 'Enter') {
        document.getElementById("sendButton").click();
    }
});