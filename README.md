
# PetLink 🐾

> Uma plataforma web centralizada para promover a adoção responsável de animais em Portugal, garantindo transparência, segurança e centralização de informação.

## 📖 Sobre o Projeto

O **PetLink** é uma aplicação web desenvolvida em **ASP.NET Core MVC** (.NET 9) que visa combater a informação dispersa sobre a adoção animal. A plataforma permite que indivíduos, associações e abrigos publiquem anúncios de adoção de forma estruturada. Conta com um sistema robusto de validação de anúncios por parte de administradores, pesquisa com filtros avançados, gestão de eventos, serviços de *Petsitting* com agendamento, chat em tempo real, avaliações, favoritos, e um assistente virtual integrado.

---

## ✨ Principais Funcionalidades

* **Adoção Responsável:** Pesquisa de animais por localização, espécie, idade e porte, com vista em mapa interativo.
* **Sistema de Perfis:** Gestão de utilizadores com diferentes roles (Adotante, Associação/Abrigo, Pet Sitter, Administrador).
* **Validação de Anúncios:** Fluxo de aprovação de anúncios pelo administrador com verificação de documentos de saúde (vacinas, desparasitação, esterilização).
* **Chat em Tempo Real:** Mensagens instantâneas via SignalR entre utilizadores e tutores/associações, contextualizadas por anúncio.
* **Assistente Virtual:** Chatbot integrado ("PetLink Buddy") com respostas automáticas sobre adoção, petsitting, eventos, e mais.
* **Gestão de Eventos:** Criação, pesquisa e registo de interesse em eventos de adoção, feiras e campanhas.
* **Petsitting:** Listagem e pesquisa de perfis de sitters com sistema de agendamento, confirmação e avaliação.
* **Avaliações:** Sistema de classificação (1-5 estrelas) para adoções e serviços de petsitting.
* **Favoritos:** Marcar animais e petsitters como favoritos para acompanhamento.
* **Recursos Educativos:** Artigos e vídeos sobre cuidados básicos, organizados por espécie e categoria.
* **Notificações:** Sistema de notificações internas para alterações de estado, novos eventos, e lembretes.
* **Autenticação:** Login por email/password com BCrypt ou via Google OAuth 2.0, com recuperação de password por email.
* **Painel de Administração:** Gestão de utilizadores, anúncios, eventos, recursos e documentos de saúde.

---

## 🛠️ Tecnologias Utilizadas

* **Backend:** C# com ASP.NET Core 9 MVC
* **Base de Dados:** SQL Server (Azure) & Entity Framework Core 9
* **Frontend:** HTML5, CSS3, JavaScript (Vanilla), Bootstrap 5, jQuery
* **Tempo Real:** SignalR (chat)
* **Autenticação:** Cookies + Google OAuth 2.0
* **Password Hashing:** BCrypt (BCrypt.Net-Next)
* **Email:** MailKit (SMTP Gmail)
* **Background Services:** EventReminderService (IHostedService)
* **Ferramentas:** Visual Studio 2022, VS Code, Git/GitHub, Azure

---

## 🏗️ Arquitetura

O sistema segue o padrão **MVC (Model-View-Controller)** do ASP.NET Core, complementado com **SignalR Hubs** para comunicação em tempo real e **Background Services** para tarefas agendadas.

### Principais componentes:

* **Models** – Representação dos dados e entidades (Entity Framework Core)
* **Views** – Interface do utilizador (Razor Pages .cshtml)
* **Controllers** – Lógica de negócio e controlo das rotas
* **Hubs** – Comunicação em tempo real via SignalR (ChatHub)
* **Services** – Lógica de domínio reutilizável (Email, Chatbot, Notificações)
* **Data** – Contexto da base de dados (ApplicationDbContext) e migrações
* **wwwroot** – Ficheiros estáticos (CSS, JavaScript, imagens)

---

## ⚙️ Pré-requisitos

Antes de começares, certifica-te de que tens as seguintes ferramentas instaladas na tua máquina:

* [.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet/9.0) (ou superior)
* [SQL Server Express](https://www.microsoft.com/pt-pt/sql-server/sql-server-downloads) ou SQL Server LocalDB
* Ferramentas Entity Framework: Instala globalmente via terminal com `dotnet tool install --global dotnet-ef`
* Um editor de código: **Visual Studio 2022** ou **Visual Studio Code**

---

## 🚀 Guia de Instalação

### Opção 1: Usando o Visual Studio 2022 (Recomendado)

1. Clona este repositório para a tua máquina:

   ```bash
   git clone https://github.com/teu-utilizador/PetLink.git
   ```

2. Navega até à pasta do projeto e abre o ficheiro da solução `PetLink.sln` com o Visual Studio 2022.
3. No painel **Solution Explorer**, abre o ficheiro `appsettings.json` e verifica se a `DefaultConnection` (Connection String) aponta para o teu SQL Server local.
4. Vai a **Tools > NuGet Package Manager > Package Manager Console**.
5. Executa as migrações para criar a base de dados e as tabelas:

    ```bash
    Update-Database
    ```

6. Pressiona `F5` ou clica no botão `Run` no topo para compilar e abrir a aplicação no navegador.

### Opção 2: Usando o Visual Studio Code (VS Code)

1. Clona este repositório:

    ```bash
    git clone https://github.com/teu-utilizador/PetLink.git
    ```

2. Abre a pasta raiz do projeto no VS Code:

    ```bash
    cd PetLink
    code .
    ```

3. Abre o ficheiro `appsettings.json` e ajusta a `DefaultConnection` para o teu servidor SQL local.
4. Abre o terminal integrado do VS Code (`` Ctrl + ` ``) e executa os seguintes comandos para restaurar os pacotes e atualizar a base de dados:

    ```bash
    dotnet restore
    dotnet ef database update
    ```

5. Para executar o projeto com Hot Reload (atualiza automaticamente o site quando fazes alterações no código HTML/CSS/C#), corre o seguinte comando:

    ```bash
    dotnet watch run
    ```

6. O terminal indicará o URL local `https://localhost:5150`. Abre esse link no teu navegador.

---

## 🗂️ Estrutura do Projeto

```
PetLink/
├── Controllers/       # Lógica de negócio e rotas
│   ├── HomeController.cs
│   ├── ProfileController.cs
│   ├── AnimalListingsController.cs
│   ├── ApplicationsController.cs
│   ├── EventsController.cs
│   ├── BookingsController.cs
│   ├── MessagesController.cs
│   ├── ReviewController.cs
│   ├── FavoritesController.cs
│   ├── PetsitterController.cs
│   ├── ResourcesController.cs
│   ├── UsersController.cs
│   └── ErrorsController.cs
├── Models/            # Entidades EF Core e ViewModels
│   ├── Enums/         # Enumerações do sistema
│   └── *.cs           (User, AnimalListing, Application, Booking, Event, etc.)
├── Views/             # Páginas Razor (.cshtml)
│   ├── Home/          # Landing page, políticas, how-it-works
│   ├── Profile/       # Login, registo, perfil, definições
│   ├── AnimalListings/ # CRUD, pesquisa, mapa, gestão
│   ├── Applications/  # Gestão de candidaturas
│   ├── Events/        # CRUD, pesquisa, gestão
│   ├── Bookings/      # Agendamentos de petsitting
│   ├── Messages/      # Chat em tempo real
│   ├── Petsitter/     # Pesquisa e detalhes de sitters
│   ├── Review/        # Avaliações
│   ├── Favorites/     # Favoritos
│   ├── Users/         # Gestão de utilizadores (admin)
│   ├── Resources/     # Recursos educativos
│   └── Errors/        # Páginas de erro (403, 404, 500)
├── Hubs/              # SignalR (ChatHub, NotificationService, EventReminderService)
├── Services/          # Lógica de domínio (Email, Chatbot)
├── Data/              # DbContext e SeedData
├── Migrations/        # Migrações EF Core
└── wwwroot/           # Ficheiros estáticos (CSS, JS, imagens)
```

---

## 👥 Equipa de Desenvolvimento

Projeto desenvolvido no âmbito das unidades curriculares de Gestão de Projetos Informáticos, Engenharia de Software Aplicada e Programação Visual.

* **Bruna Rossa, 202200603** - PM e Frontend Developer
* **Diana Francisco, 202100637** - Scrum Master e Frontend Developer
* **Lucas Almeida, 202100067** - Backend Developer
* **Rita Pereira, 202200170** - Scrum Master e Frontend Developer

---

## 📄 Licença

Este projeto tem fins académicos.

---
