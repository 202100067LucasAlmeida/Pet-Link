(function () {
    'use strict';

    const toggleBtn = document.getElementById('chatbot-toggle');
    const toggleIcon = document.getElementById('chatbot-toggle-icon');
    const windowEl = document.getElementById('chatbot-window');
    const closeBtn = document.getElementById('chatbot-close');
    const messagesEl = document.getElementById('chatbot-messages');
    const inputEl = document.getElementById('chatbot-input');
    const sendBtn = document.getElementById('chatbot-send');
    const avatarDisplay = document.getElementById('chatbot-avatar-display');
    const settingsBtn = document.getElementById('chatbot-settings');
    const avatarMenu = document.getElementById('chatbot-avatar-menu');
    const avatarOptions = document.querySelectorAll('.chatbot-avatar-option');
    const notifEl = document.getElementById('chatbot-notification');
    const notifCloseBtn = document.getElementById('chatbot-notif-close');

    let isOpen = false;
    let isWaiting = false;

    const savedAvatar = localStorage.getItem('petlink_bot_avatar');
    if (savedAvatar) {
        avatarDisplay.textContent = savedAvatar;
        toggleIcon.textContent = savedAvatar;
    }

    settingsBtn.addEventListener('click', function (e) {
        e.stopPropagation();
        avatarMenu.classList.toggle('open');
    });

    document.addEventListener('click', function () {
        avatarMenu.classList.remove('open');
    });

    avatarOptions.forEach(function (opt) {
        opt.addEventListener('click', function (e) {
            e.stopPropagation();
            const emoji = this.getAttribute('data-emoji');

            avatarDisplay.textContent = emoji;
            toggleIcon.textContent = emoji;

            localStorage.setItem('petlink_bot_avatar', emoji);
            avatarMenu.classList.remove('open');
        });
    });

    const savedHistory = sessionStorage.getItem('petlink_chat_history');
    if (savedHistory) {
        messagesEl.innerHTML = savedHistory;
    } else {

        saveHistory();
    }

    if (sessionStorage.getItem('petlink_chat_open') === 'true') {
        open(false);
    }

    function saveHistory() {
        sessionStorage.setItem('petlink_chat_history', messagesEl.innerHTML);
    }

    function open(focus = true) {
        isOpen = true;
        windowEl.classList.add('open');
        toggleBtn.style.display = 'none';

        notifEl.classList.remove('show-notif');
        sessionStorage.setItem('petlink_notif_shown', 'true');

        sessionStorage.setItem('petlink_chat_open', 'true');
        scrollToBottom();
        if (focus) inputEl.focus();
    }

    function close() {
        isOpen = false;
        windowEl.classList.remove('open');
        toggleBtn.style.display = 'flex';
        sessionStorage.setItem('petlink_chat_open', 'false');
    }

    function scrollToBottom() {
        messagesEl.scrollTo({
            top: messagesEl.scrollHeight,
            behavior: 'smooth'
        });
    }

    function parseMarkdown(md) {
        return md
            .replace(/\*\*(.*?)\*\*/g, '<strong>$1</strong>')
            .replace(/\[([^\]]+)\]\(([^)]+)\)/g, '<a href="$2" class="text-white text-decoration-underline">$1</a>')
            .replace(/\n/g, '<br>');
    }

    function addMessage(text, sender) {
        const div = document.createElement('div');
        div.className = 'chatbot-msg ' + sender;

        const bubble = document.createElement('div');
        bubble.className = 'chatbot-bubble ' + sender + '-bubble';
        bubble.innerHTML = parseMarkdown(text);

        div.appendChild(bubble);
        messagesEl.appendChild(div);
        scrollToBottom();

        saveHistory();
    }

    function addTypingIndicator() {
        const div = document.createElement('div');
        div.className = 'chatbot-msg bot';
        div.id = 'chatbot-typing';
        div.innerHTML = '<div class="chatbot-bubble bot-bubble typing">...</div>';
        messagesEl.appendChild(div);
        scrollToBottom();
    }

    function removeTypingIndicator() {
        const el = document.getElementById('chatbot-typing');
        if (el) el.remove();
    }

    async function sendMessage() {
        const text = inputEl.value.trim();
        if (!text || isWaiting) return;

        inputEl.value = '';
        addMessage(text, 'user');
        isWaiting = true;
        addTypingIndicator();

        try {
            const resp = await fetch('/api/chatbot/message', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ message: text })
            });

            const data = await resp.json();
            removeTypingIndicator();

            if (data.reply) {
                addMessage(data.reply, 'bot');
            }
        } catch (err) {
            removeTypingIndicator();
            addMessage('Sorry, I could not reach the server. Please try again.', 'bot');
        } finally {
            isWaiting = false;
        }
    }

    toggleBtn.addEventListener('click', () => open(true));
    closeBtn.addEventListener('click', close);
    sendBtn.addEventListener('click', sendMessage);

    inputEl.addEventListener('keydown', function (e) {
        if (e.key === 'Enter') {
            e.preventDefault();
            sendMessage();
        }
    });

    setTimeout(() => {
        const chatAlreadyOpen = sessionStorage.getItem('petlink_chat_open') === 'true';
        const notifAlreadyDismissed = sessionStorage.getItem('petlink_notif_shown') === 'true';

        if (!chatAlreadyOpen && !notifAlreadyDismissed && !isOpen) {
            notifEl.classList.add('show-notif');
        }
    }, 2000);

    notifCloseBtn.addEventListener('click', function (e) {
        e.stopPropagation();
        notifEl.classList.remove('show-notif');
        sessionStorage.setItem('petlink_notif_shown', 'true');
    });
})();