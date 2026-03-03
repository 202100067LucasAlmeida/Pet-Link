# PetLink 🐾

> Uma plataforma web centralizada para promover a adoção responsável de animais em Portugal, garantindo transparência, segurança e centralização de informação.

## 📖 Sobre o Projeto

O **PetLink** é uma aplicação web desenvolvida em **ASP.NET Core MVC** que visa combater a informação dispersa sobre a adoção animal. A plataforma permite que indivíduos, associações e abrigos publiquem anúncios de adoção de forma estruturada. Conta com um sistema robusto de validação por parte de administradores, pesquisa com filtros avançados, e uma secção dedicada a serviços de *Petsitting*.

## ✨ Principais Funcionalidades

* **Adoção Responsável:** Pesquisa de animais por localização, espécie, idade e porte.
* **Sistema de Perfis:** Gestão de utilizadores (Adotantes, Associações e Pet Sitters).
* **Validação de Anúncios:** Fluxo de aprovação de anúncios pelo administrador (garantindo veracidade e preenchimento de checklist de saúde).
* **Mensagens Internas:** Comunicação direta e segura entre utilizadores e tutores/associações.
* **Recursos Educativos:** Artigos e tutoriais sobre os cuidados básicos de cada animal.
* **Petsitting:** Listagem de perfis de *sitters* para alojamento e passeios.

## 🛠️ Tecnologias Utilizadas

* **Backend:** C# com ASP.NET Core 8 MVC
* **Base de Dados:** Microsoft SQL Server & Entity Framework Core
* **Frontend:** HTML5, CSS3, JavaScript (Vanilla) e Bootstrap 5
* **Ferramentas:** Visual Studio 2022, VS Code, Git/GitHub

---

## ⚙️ Pré-requisitos

Antes de começares, certifica-te de que tens as seguintes ferramentas instaladas na tua máquina:

* [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) (ou superior)
* [SQL Server Express](https://www.microsoft.com/pt-pt/sql-server/sql-server-downloads) ou SQL Server LocalDB
* Ferramentas Entity Framework: Instala globalmente via terminal com `dotnet tool install --global dotnet-ef`
* Um editor de código: **Visual Studio 2022** ou **Visual Studio Code**

---

## 🚀 Guia de Instalação

### Opção 1: Usando o Visual Studio 2022 (Recomendado)

1. Clona este repositório para a tua máquina:

   ```bash
   git clone [https://github.com/teu-utilizador/PetLink.git](https://github.com/teu-utilizador/PetLink.git)
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
    git clone [https://github.com/teu-utilizador/PetLink.git](https://github.com/teu-utilizador/PetLink.git)
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

* `Controllers/`: Contém a lógica de negócio e as rotas da aplicação web.
* `Models/`: Contém as classes que representam os dados (Entidades do Entity Framework) e ViewModels.
* `Views/`: Contém as páginas Razor (`.cshtml`) da interface gráfica de utilizador.
* `Data/`: Contém o contexto da base de dados (`ApplicationDbContext`) e as migrações.
* `wwwroot/`: Ficheiros estáticos públicos como CSS, JavaScript e Imagens.

---

## 👥 Equipa de Desenvolvimento

Projeto desenvolvido no âmbito da unidade curricular de Gestão de Projetos Informáticos, Engenharia de Software Aplicada e Programação Visual.

* **Bruna Rossa, 202200603** - PM e Frontend Developer
* **Diana Francisco, 202100637** - Scrum Master e Frontend Developer
* **Lucas Almeida, 202100067** - Backend Developer
* **Rita Pereira, 202200170** - Scrum Master e Frontend Developer

---

## 📄 Licença

Este projeto tem fins académicos.

---
