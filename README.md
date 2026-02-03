# Developer Evaluation Project (DeveloperStore)

## Introdução

Este é um projeto de estudo e avaliação técnica, com foco em arquitetura, boas práticas, qualidade de código e capacidade de entrega incremental (commits por etapa).

Tecnologias e práticas utilizadas:
- .NET 8 / C#
- DDD e separação de camadas (Domain, Application, Infrastructure, API)
- PostgreSQL (EF Core) e MongoDB (read model)
- MediatR (Mediator), AutoMapper, xUnit, NSubstitute e Bogus (Faker)
- Paginação, filtragem e ordenação em endpoints
- Tratamento de erros padronizado
- Documentação via Swagger e Scalar

Documentação técnica detalhada: `architecture.md`.

## Use Case

Você é um desenvolvedor do time DeveloperStore e precisa implementar protótipos de API.

Como trabalhamos com DDD, para referenciar entidades de outros domínios usamos o padrão **External Identities**, com denormalização de descrições.

Portanto, este projeto implementa uma API (CRUD completo) para registros de vendas (Sales), informando:
- Número da venda
- Data da venda
- Cliente (External Identity)
- Filial (External Identity)
- Produtos (External Identity)
- Quantidades
- Preços unitários
- Descontos
- Total por item
- Total da venda
- Cancelado / Não cancelado (venda e item)

## Eventos (diferencial)

Não é obrigatório, mas é um diferencial publicar eventos (sem broker real; apenas log):
- `SaleCreated`
- `SaleModified`
- `SaleCancelled`
- `ItemCancelled`

## Regras de Negócio (descontos)

- Compras com **4 a 9** itens idênticos: **10%** de desconto
- Compras com **10 a 20** itens idênticos: **20%** de desconto
- Não é permitido vender **acima de 20** itens idênticos
- Compras com **menos de 4** itens: **sem desconto**

## Referências (.doc)

- Overview: `.doc/overview.md`
- Tech Stack: `.doc/tech-stack.md`
- Frameworks: `.doc/frameworks.md`
- Definições gerais de API: `.doc/general-api.md`
- Estrutura do projeto: `.doc/project-structure.md`

## Arquitetura (resumo)

Modelo em camadas com dependências apontando para o núcleo:
- Domain: regras e entidades (puro, sem dependências externas)
- Application: casos de uso (MediatR), contratos e resultados
- Infrastructure: persistência (EF Core/PostgreSQL) e leitura (MongoDB)
- API: endpoints, DI, middleware de erro e documentação

Detalhes completos: `architecture.md`.

## Como executar

Pré-requisitos:
- .NET SDK 8
- PostgreSQL e MongoDB

1) Crie o banco no PostgreSQL (se necessário)
- Database: `developer_store`

2) Ajuste as connection strings

Arquivo: `src/DeveloperStore.Api/appsettings.json`

3) Restore e run
```
dotnet restore
dotnet run --project src/DeveloperStore.Api
```

4) Aplicar migrations (PostgreSQL)
```
dotnet ef migrations add Inicial --project src/DeveloperStore.Infra --startup-project src/DeveloperStore.Api --context DeveloperStore.Infra.Persistence.SalesDbContext
dotnet ef database update --project src/DeveloperStore.Infra --startup-project src/DeveloperStore.Api --context DeveloperStore.Infra.Persistence.SalesDbContext
```

5) Rodar testes
```
dotnet test
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

### Paginação, ordenação e filtros

- `_page` (padrão: 1)
- `_size` (padrão: 10)
- `_order` (ex.: `saleDate desc, totalAmount asc`)

Filtros:
- `campo=valor` (match exato)
- strings com `*` para match parcial (ex.: `customer=Maria*`)
- `_minCampo` / `_maxCampo` para numérico/data (ex.: `_minSaleDate=2024-01-01`)

### Formato de erro

```json
{
  "type": "string",
  "error": "string",
  "detail": "string"
}
```

## Documentação da API

- Swagger UI: `/swagger`
- Scalar UI: `/scalar/v1`

