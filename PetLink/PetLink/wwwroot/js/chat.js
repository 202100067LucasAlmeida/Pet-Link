"use strict";

if (typeof signalR === "undefined") {
    console.error("SignalR library not loaded! Check your script paths.");
}

let connection;

// Função centralizada para scroll
function scrollToBottom() {
    const chatContainer = document.getElementById("chatWindow");
    if (chatContainer) {
        // O timeout de 50ms garante que o browser já renderizou o novo HTML
        setTimeout(() => {
            chatContainer.scrollTo({
                top: chatContainer.scrollHeight,
                behavior: 'smooth'
            });
        }, 50);
    }
}

function initChat(currentUserId) {
    connection = new signalR.HubConnectionBuilder()
        .withUrl("/chatHub")
        .withAutomaticReconnect() // Religa sozinho se a net cair
        .build();

    // 1. Ouvinte de mensagens
    connection.on("ReceiveMessage", function (senderId, content, timeStr) {
        console.log("Mensagem recebida de:", senderId);
        const isMine = parseInt(senderId) === parseInt(currentUserId);
        const msgHtml = generateMessageHtml(content, timeStr, isMine);
        
        const chatContainer = document.getElementById("chatWindow");
        if (chatContainer) {
            chatContainer.insertAdjacentHTML("beforeend", msgHtml);
            scrollToBottom();
        }
    });

    // 2. Ligar ao Hub
    connection.start()
        .then(() => {
            console.log("SignalR ligado.");
            const receiverId = $("#receiverId").val();

            if (receiverId) {
                connection.invoke("JoinChat", parseInt(currentUserId), parseInt(receiverId))
                    .then(() => {
                        console.log("Entrei na sala.");
                        scrollToBottom(); // Scroll inicial ao entrar
                    })
                    .catch(err => console.error("Erro ao entrar na sala:", err));
            }
        })
        .catch(err => console.error("Erro na ligação SignalR:", err));
}

function generateMessageHtml(content, time, isMine) {
    const alignment = isMine ? "justify-content-end" : "justify-content-start";
    const bubbleStyle = isMine
        ? "background-color: var(--petlink-primary); color: white; border-bottom-right-radius: 4px;"
        : "background-color: white; color: var(--petlink-dark); border-bottom-left-radius: 4px;";

    return `
        <div class="d-flex mb-4 ${alignment}">
            ${!isMine ? '<img src="/images/default-avatar.jpg" class="rounded-circle me-2 align-self-end mb-1" width="28" height="28">' : ""}
            <div class="p-3 shadow-sm" style="max-width: 75%; border-radius: 18px; ${bubbleStyle}">
                <p class="mb-1" style="font-size: 0.9rem; line-height: 1.4;">${content}</p>
                <div class="text-end" style="font-size: 0.65rem; opacity: 0.7;">${time}</div>
            </div>
        </div>
    `;
}

// Handler do formulário
$(document).on("submit", "#sendMessageForm", function (e) {
    e.preventDefault();
    const receiverId = $("#receiverId").val();
    const content = $("#messageInput").val();
    
    if (content.trim() && receiverId && connection) {
        // Limpar input imediatamente para melhor UX
        $("#messageInput").val("").focus();

        connection.invoke("SendChatMessage", parseInt(receiverId), content.trim())
            .then(() => {
                scrollToBottom();
            })
            .catch(err => {
                console.error("Erro ao enviar:", err);
                alert("Could not send message. Try again.");
            });
    }
});

// Inicialização
$(document).ready(function () {
    const currentUserId = $("#currentUserId").val();
    if (currentUserId) {
        initChat(currentUserId);
    }
    
    // Garantir scroll no primeiro load
    scrollToBottom();
});