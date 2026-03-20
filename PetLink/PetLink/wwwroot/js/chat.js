"use strict";

var connection = new signalR.HubConnectionBuilder().withUrl("/chatHub").build();

connection.on("ReceiveMessage", function (senderId, content, time) {
    // 1. Verifica se esta mensagem pertence à conversa aberta
    var activeChatId = $("input[name='receiverId']").val();
    var currentUserId = $("#currentUserId").val(); // Precisas de um input hidden com o teu ID

    var isMine = senderId == currentUserId;

    // 2. Cria o HTML do balão (usa as mesmas classes que já tens no CSHTML)
    var alignment = isMine ? "justify-content-end" : "justify-content-start";
    var bg = isMine ? "background-color: var(--petlink-primary); color: white;" : "background-color: white; color: var(--petlink-dark);";

    var msgHtml = `
        <div class="message-sent d-flex justify-content-end mb-3">
            <div class="message-bubble text-white p-3 rounded-4 shadow-sm" 
                style="max-width: 85%; background-color: var(--petlink-primary);">
                <div class="fw-bold small text-primary-white mb-1">${user}</div>
                <p class="mb-0 small">${message}</p>
            </div>
        </div>`;

    // 3. Adiciona ao chat e faz scroll
    $("#chatWindow").append(messageHtml);
    $("#chatWindow").scrollTop($("#chatWindow")[0].scrollHeight);
});

connection.start();

// Intercetar o formulário para enviar via SignalR em vez de POST normal
$('#sendMessageForm').on('submit', function (e) {
    e.preventDefault();
    var receiverId = $("input[name='receiverId']").val();
    var content = $("#messageInput").val();

    if (content.trim() !== "") {
        connection.invoke("SendChatMessage", parseInt(receiverId), content);
        $("#messageInput").val("").focus();
    }
});