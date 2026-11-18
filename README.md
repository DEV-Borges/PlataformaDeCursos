# Plataforma de Cursos API (.NET 8)

API RESTful para gerenciamento de cursos, estudantes e matrículas, construída com **ASP.NET Core 8**, **Entity Framework Core (MySQL/Pomelo)**, **ASP.NET Core Identity**, **JWT Bearer**, **AutoMapper** e **FluentValidation**.

## Como Rodar o Projeto

### 1. Pré-requisitos
* [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
* Servidor **MySQL** ativo localmente
* Ferramenta `dotnet-ef` instalada (`dotnet tool install --global dotnet-ef --version 8.0.*`)

### 2. Configurar Segredos (`user-secrets`)
Na pasta do projeto (`PlataformaDeCursos/`), configure a string de conexão do MySQL e a chave JWT:

```bash
dotnet user-secrets set "ConnectionStrings:CursoConnection" "Server=localhost;Database=usuariodb;Uid=root;Pwd=SUA_SENHA;"
dotnet user-secrets set "Jwt:Key" "SuaSuperChaveSecretaMuitoSeguraParaOProjeto2024!"
dotnet user-secrets set "Jwt:Issuer" "PlataformaDeCursosAPI"
dotnet user-secrets set "Jwt:Audience" "PlataformaDeCursosApp"
