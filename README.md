## Introducao do Projeto

Este projeto é um projeto de estudo, com foco em arquitetura, boas praticas e capacidade de entrega em etapas. 

Neste projeto, foram considerados os seguintes tópicos:
- Desenvolvimento em C# e .NET 8
- Separação de camadas e organização do projeto
- PostgreSQL e MongoDB
- Padrões de projeto (Mediator, Repository, CQRS e etc)
- EF Core
- Testes com xUnit, mocks com NSubstitute e dados com Bogus
- AutoMapper
- Design de API REST
- Git Flow e commits semanticos
- Paginação, filtros e ordenação
- Tratamento de erros e padrão de resposta
- Programação assíncrona
- Performance e qualidade de código

Para detalhes completos da arquitetura veja: `architecture.md`.

## Tech Stack
This section lists the key technologies used in the project, including the backend, testing, frontend, and database components. 

See [Tech Stack](/.doc/tech-stack.md)

## Frameworks
This section outlines the frameworks and libraries that are leveraged in the project to enhance development productivity and maintainability. 

See [Frameworks](/.doc/frameworks.md)

<!-- 
## API Structure
This section includes links to the detailed documentation for the different API resources:
- [API General](./docs/general-api.md)
- [Products API](/.doc/products-api.md)
- [Carts API](/.doc/carts-api.md)
- [Users API](/.doc/users-api.md)
- [Auth API](/.doc/auth-api.md)
-->

## Project Structure
This section describes the overall structure and organization of the project files and directories. 

See [Project Structure](/.doc/project-structure.md)

## Arquitetura (resumo)

Modelo em camadas com dependencias sempre apontando para o nucleo do dominio:

- Domain: regras e entidades (puro, sem dependencias externas)
- Application: casos de uso, comandos/queries e contratos
- Infrastructure: persistencia e integracoes (EF Core + Mongo)
- API: endpoints, DI e middleware

Fluxo de dependencias:
```
API -> Application -> Domain
API -> Infrastructure -> Application -> Domain
Tests -> Application/Domain
```

## Como executar

1) Suba os bancos
- PostgreSQL: `Host=localhost;Port=5432;Database=developer_store;Username=postgres;Password=postgres`
- MongoDB: `mongodb://localhost:27017` (database `developer_store`)

2) Execute a API
```
dotnet run --project src/DeveloperStore.Api
```

## Endpoints (Sales)

Base: `/sales`

- `GET /sales` (paginado)
- `GET /sales/{id}`
- `POST /sales`
- `PUT /sales/{id}`
- `DELETE /sales/{id}`
- `POST /sales/{id}/cancel`
- `POST /sales/{saleId}/items/{itemId}/cancel`

### Paginacao, ordenacao e filtros

- `_page` (default: 1)
- `_size` (default: 10)
- `_order` (ex: `saleDate desc, totalAmount asc`)

Filtros:
- `campo=valor` (match exato)
- strings com `*` para match parcial (ex: `customer=Maria*`)
- `_minCampo` / `_maxCampo` para numerico/data (ex: `_minSaleDate=2024-01-01`)

### Formato de erro

```json
{
  "type": "string",
  "error": "string",
  "detail": "string"
}
```

## Documentacao da API

- Swagger UI: `/swagger`
- Scalar UI: `/scalar/v1`
