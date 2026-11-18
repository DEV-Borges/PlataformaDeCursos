# Changelog

Todas as mudanças notáveis neste projeto serão documentadas neste arquivo.

## [v0.1.0] - 2026-11-17

### Adicionado
- Modelagem de domínio com EF Core e MySQL (`Cursos`, `Estudantes` e `Inscricoes`) com suporte a *Soft Delete* e índices únicos.
- Integração com ASP.NET Core Identity, políticas de senha/lockout e autenticação via token JWT Bearer.
- `DbSeeder` automático para criação das roles (`Admin`, `Instructor`, `Student`) e conta de administrador inicial.
- CRUD completo de Cursos (`CursosController`) com paginação, filtros por categoria, busca textual, ordenação e cache HTTP.
- Gestão de Estudantes (`EstudantesController`) com validação de privacidade pelo token (`sub`) e rota `/api/Estudantes/me`.
- Fluxo transacional de Matrículas (`InscricoesController`) com prevenção de duplicidade (`409 Conflict`) e regra de cancelamento em até 7 dias.
- Validações de entrada com `FluentValidation` e `TratamentoExcecaoMiddleware` global retornando `ProblemDetails` (RFC 7807) com `traceId`.
- Documentação interativa no Swagger (OpenAPI) com suporte a Bearer Token e coleção de requisições em `PlataformaDeCursos.http`.