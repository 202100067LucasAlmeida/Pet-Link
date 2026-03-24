"use strict";

if (typeof signalR === "undefined") {
  console.error("SignalR library not loaded! Check your script paths.");
}

let connection;

function initChat(currentUserId) {
  connection = new signalR.HubConnectionBuilder().withUrl("/chatHub").build();

  // 1. Prepara o ouvido para receber mensagens
  connection.on("ReceiveMessage", function (senderId, content, timeStr) {
    console.log("Recebi uma mensagem do servidor!", content); // Log para ajudar a testar
    const isMine = parseInt(senderId) === parseInt(currentUserId);
    const msgHtml = generateMessageHtml(content, timeStr, isMine);
    appendToChatContainer(msgHtml);
  });

  // 2. PRIMEIRO liga, e SÓ DEPOIS entra na sala
  connection
    .start()
    .then(() => {
      console.log("SignalR connected");

      // Verifica se usaste ID ou Name no HTML. Tenta os dois por segurança:
      const receiverId =
        $("#receiverId").val() || $('input[name="receiverId"]').val();

      if (receiverId) {
        // Agora sim, a ligação está aberta, podemos pedir para entrar na sala!
        connection
          .invoke("JoinChat", parseInt(currentUserId), parseInt(receiverId))
          .then(() => console.log("Entrei na sala de chat!"))
          .catch((err) => console.error("Erro ao entrar na sala:", err));
      }
    })
    .catch((err) => console.error("SignalR Error:", err));

  connection.onclose(() => setTimeout(() => connection.start(), 5000));
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

function appendToChatContainer(html) {
  const chatContainer =
    document.getElementById("chatWindow") ||
    document.querySelector(".chat-container, #chatBox");
  if (chatContainer) {
    chatContainer.insertAdjacentHTML("beforeend", html);
    chatContainer.scrollTop = chatContainer.scrollHeight;
  }
}

function sendMessage(receiverId, content, currentUserId) {
  if (!connection || !content.trim()) return;

  // Apenas invocamos o servidor.
  // O servidor responderá via "ReceiveMessage" para nós e para o outro.
  connection
    .invoke("SendChatMessage", parseInt(receiverId), content.trim())
    .then(() => {
      $("#messageInput").val("").focus();
    })
    .catch((err) => console.error("SignalR Invoke Error:", err));
}

// Form handler
$(document).on("submit", "#sendMessageForm", function (e) {
  e.preventDefault();
  const receiverId = $("#receiverId").val();
  const content = $("#messageInput").val();
  const currentUserId = $("#currentUserId").val();

  if (content.trim() && receiverId) {
    sendMessage(receiverId, content, currentUserId);
    $("#messageInput").val("").focus();
  }
});

// Init on load
$(document).ready(function () {
  const currentUserId = $("#currentUserId").val();
  if (currentUserId) {
    initChat(currentUserId);
  }
  // Scroll initial
  const chatCont =
    document.getElementById("chatWindow") || document.querySelector("#chatBox");
  if (chatCont) chatCont.scrollTop = chatCont.scrollHeight;
});
