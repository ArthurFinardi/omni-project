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

### Publisher por configuração (opcional)

O publisher de eventos pode ser selecionado via configuração:
- Padrão: `Log` (registra eventos no console)
- Opcional: `RabbitMq` (simulado; não publica em broker real)

Arquivo: `src/DeveloperStore.Api/appsettings.json`
```json
{
  "EventPublisher": { "Provider": "Log" },
  "RabbitMq": { "Enabled": false, "Exchange": "developer_store.events" }
}
```

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
```bash
dotnet restore
dotnet run --project src/DeveloperStore.Api
```

4) Aplicar migrations (PostgreSQL)
```bash
dotnet ef migrations add Inicial --project src/DeveloperStore.Infra --startup-project src/DeveloperStore.Api --context DeveloperStore.Infra.Persistence.SalesDbContext
dotnet ef database update --project src/DeveloperStore.Infra --startup-project src/DeveloperStore.Api --context DeveloperStore.Infra.Persistence.SalesDbContext
```

5) Rodar testes
```bash
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

- `_page` (padrão: 1). Validação: `_page >= 1`
- `_size` (padrão: 10). Validação: `1 <= _size <= 100`
- `_order` (ex.: `saleDate desc, totalAmount asc`)
  - Campos permitidos: `saleNumber`, `saleDate`, `totalAmount`
  - Direções permitidas: `asc`, `desc`
  - Campo/direção inválidos: `400 ValidationError`

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

## Exemplos (request/response)

### Criar venda

`POST /sales`

Request:
```json
{
  "saleNumber": "S-100",
  "saleDate": "2026-02-03T12:00:00Z",
  "customer": { "externalId": "cust-1", "description": "Cliente 1" },
  "branch": { "externalId": "branch-1", "description": "Filial 1" },
  "items": [
    {
      "product": { "externalId": "prod-1", "description": "Produto 1" },
      "quantity": 4,
      "unitPrice": 10.0
    }
  ]
}
```

### Atualizar venda

`PUT /sales/{id}`

Observação: o `id` da rota deve ser o mesmo do corpo.

### Cancelar venda

`POST /sales/{id}/cancel`

### Cancelar item

`POST /sales/{saleId}/items/{itemId}/cancel`

### Listar vendas com paginação/ordenação/filtros

Exemplos:
- `GET /sales?_page=1&_size=10&_order="saleDate desc"`
- `GET /sales?_page=1&_size=10&customer=Maria*`
- `GET /sales?_page=1&_size=10&_minSaleDate=2026-02-01&_maxSaleDate=2026-02-03`
- `GET /sales?_page=1&_size=10&_minTotalAmount=10&_maxTotalAmount=200`

