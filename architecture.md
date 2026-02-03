# Arquitetura do Projeto (DeveloperStore)

## 1. Introdução

Este é um projeto de estudo e avaliação técnica, com foco em arquitetura, boas práticas, qualidade de código e capacidade de entrega incremental (commits por etapa).

Competências e tópicos abordados:
- Desenvolvimento em C# e .NET 8
- Separação de camadas (DDD / Clean Architecture)
- PostgreSQL e MongoDB
- Padrões de projeto (ex.: Mediator)
- ORM com EF Core
- Testes com xUnit, mocks com NSubstitute e dados com Bogus (Faker)
- Mapeamento de objetos com AutoMapper
- Design de API REST
- Paginação, filtragem e ordenação
- Tratamento de erros e padronização de respostas
- Programação assíncrona
- Observabilidade básica (logs) e eventos de domínio/aplicação

## 2. Visão Geral da Solução

O domínio implementado é o de **Vendas (Sales)**, incluindo itens, descontos por quantidade e operações de cancelamento (venda e item).

Além do CRUD, há suporte a eventos (diferencial) para rastrear mudanças relevantes:
- `SaleCreated`
- `SaleModified`
- `SaleCancelled`
- `ItemCancelled`

## 3. Modelo de Separação de Camadas

A solução adota um modelo em camadas inspirado em DDD e Clean Architecture, com dependências apontando para o núcleo (Domain).

### 3.1 Domain (`src/DeveloperStore.Domain`)

Responsabilidades:
- Entidades e regras de negócio (ex.: cálculo de desconto, cancelamento, totalização)
- Value Objects e invariantes
- Exceções de domínio

Características:
- Não depende de infraestrutura, banco, API ou frameworks
- Mantém regras síncronas e determinísticas (sem I/O)

Principais tipos:
- Entidades: `Sale`, `SaleItem`
- Value Objects: `Money`, `Quantity`, `Discount`, `ExternalIdentity`
- Exceções: `DomainException`

### 3.2 Application (`src/DeveloperStore.Application`)

Responsabilidades:
- Casos de uso via CQRS (Commands/Queries) usando MediatR
- Orquestração entre domínio, persistência e eventos
- Contratos (DTOs) e padronização de resultados (`Result<T>`, `PagedResult<T>`)

Características:
- Depende apenas de `DeveloperStore.Domain`
- Define interfaces para persistência e publicação de eventos

Principais elementos:
- Commands/Queries: `CreateSaleCommand`, `UpdateSaleCommand`, `GetSalesPagedQuery` etc.
- Handlers: `CreateSaleCommandHandler` etc.
- Interfaces: `ISaleRepository`, `ISaleReadRepository`, `IEventPublisher`
- Mapeamento: `SaleMappingProfile` (AutoMapper)

### 3.3 Infrastructure (`src/DeveloperStore.Infra`)

Responsabilidades:
- Implementar persistência e integrações técnicas
- EF Core + PostgreSQL para escrita (modelo relacional)
- MongoDB para leitura (modelo denormalizado/read model)

Principais componentes:
- EF Core: `SalesDbContext` + configurações (`SaleConfiguration`, `SaleItemConfiguration`)
- Repositório SQL: `SaleRepository`
- Read model Mongo: `SaleReadModel` + `SaleReadRepository`

### 3.4 API (`src/DeveloperStore.Api`)

Responsabilidades:
- Endpoints REST e contrato HTTP
- Injeção de dependências (DI) e wiring das camadas
- Middleware de tratamento de erros (formato padronizado)
- Documentação via Swagger e Scalar

Principais componentes:
- Controller: `SalesController`
- Base controller: `ApiControllerBase` (conversão `Result<T>` -> HTTP)
- Publicação de eventos via log: `LogEventPublisher`

### 3.5 Tests (`tests/DeveloperStore.Tests`)

Responsabilidades:
- Testes unitários de regras do domínio e handlers da aplicação
- Mocks com NSubstitute
- Geração de dados (quando necessário) com Bogus

## 4. Fluxo de Dependências

Dependências (sempre para dentro):

```
API -> Application -> Domain
API -> Infrastructure -> Application -> Domain
Tests -> Application/Domain
```

## 5. Padrões de Projeto Utilizados

### 5.1 Mediator (MediatR)

O padrão Mediator é aplicado com MediatR para desacoplar API e casos de uso:
- A API envia `Commands/Queries` para o MediatR
- Handlers executam o caso de uso e retornam `Result<T>`

Benefícios:
- Baixo acoplamento entre camadas
- Facilidade de testes unitários dos handlers
- Organização por caso de uso

### 5.2 Repository

O acesso a dados é abstraído por interfaces na camada Application:
- `ISaleRepository`: escrita/leitura relacional (PostgreSQL)
- `ISaleReadRepository`: leitura (read model) no MongoDB

Benefícios:
- Infra substituível
- Domínio e casos de uso desacoplados de EF Core/MongoDB

### 5.3 CQRS (Command Query Responsibility Segregation)

Separação entre operações de escrita e leitura:
- Escrita: EF Core + PostgreSQL
- Leitura: MongoDB (modelo denormalizado)

Benefícios:
- Consultas de leitura mais simples e rápidas (denormalização)
- Evolução independente das preocupações de leitura e escrita

### 5.4 Value Objects e Domain Model

O domínio usa Value Objects para encapsular invariantes:
- `Quantity` valida limites e regras
- `Discount` representa a taxa de desconto
- `Money` padroniza valores monetários

As entidades (`Sale`, `SaleItem`) são modelos ricos, contendo comportamento de negócio.

### 5.5 Adapter (publicação de eventos)

Eventos são publicados por uma abstração:
- Interface: `IEventPublisher`
- Implementação: `LogEventPublisher` (log em vez de broker)

Isso permite substituir futuramente por um publisher real (ex.: Rebus) sem alterar a regra de negócio.

## 6. External Identities (Identidades Externas)

Para referenciar entidades de outros domínios (ex.: Customer, Branch, Product) sem acoplamento, é utilizado o padrão **External Identities**:

- Armazena `ExternalId` (identificador externo) e `Description` (descrição denormalizada/snapshot)
- Evita dependências diretas e “joins” com outros contextos
- Mantém consistência de leitura mesmo que a descrição mude no sistema de origem (é um snapshot)

No domínio:
- `Sale.Customer` e `Sale.Branch` usam `ExternalIdentity`
- `SaleItem.Product` usa `ExternalIdentity`

## 7. Eventos de Publicação (diferencial)

Eventos implementados:
- `SaleCreatedEvent`
- `SaleModifiedEvent`
- `SaleCancelledEvent`
- `ItemCancelledEvent`

Como funciona:
- Handlers publicam eventos via `IEventPublisher`
- A implementação atual (`LogEventPublisher`) escreve o evento no log da aplicação

Importante:
- Não há broker configurado (por requisito). A publicação é simulada por log.

## 8. Programação Síncrona e Assíncrona

- Síncrona:
  - Regras do domínio (cálculo de desconto, totalização e cancelamentos)

- Assíncrona:
  - API (controllers), handlers e repositórios usam `async/await`
  - Operações de I/O (PostgreSQL/MongoDB) são assíncronas
  - `CancellationToken` é propagado pelas chamadas de repositório/publisher quando aplicável

## 9. Persistência e Consultas

### 9.1 PostgreSQL (EF Core)

Uso principal:
- Persistência relacional das entidades de venda e itens

Observação:
- É necessário aplicar migrations para criar as tabelas no banco.

### 9.2 MongoDB (Read Model)

Uso principal:
- Consultas de leitura com modelo denormalizado (`sales_read`)
- Paginação, filtros e ordenação no repositório de leitura

Como é alimentado hoje:
- O read model **não é sincronizado automaticamente** a partir do PostgreSQL nesta versão.
- A aplicação possui eventos de aplicação (ex.: `SaleCreatedEvent`), publicados via `IEventPublisher`, porém o publisher atual apenas registra logs.

Como seria em produção (abordagem recomendada):
- Publicar eventos em um broker (ex.: RabbitMQ) e manter um consumidor responsável por:
  - Projetar eventos do domínio/aplicação em documentos MongoDB (read model)
  - Garantir idempotência por `SaleId` e versionamento/ordem de eventos quando necessário
  - Implementar retentativas, DLQ e observabilidade

## 10. Paginação, Ordenação, Filtros e Erros

Definições seguem `.doc/general-api.md`.

### 10.1 Paginação
- `_page` (padrão: 1)
- `_size` (padrão: 10)

### 10.2 Ordenação
- `_order` (ex.: `saleDate desc, totalAmount asc`)

### 10.3 Filtros
- `campo=valor`
- strings com `*` para match parcial (`customer=Maria*`, `branch=*Centro`)
- intervalos: `_minCampo` / `_maxCampo` (numérico/data)

### 10.4 Erros

Formato padrão:
```json
{
  "type": "string",
  "error": "string",
  "detail": "string"
}
```

## 11. Documentação da API

- Swagger UI: `/swagger`
- Scalar UI: `/scalar/v1`

## 12. Estrutura de Pastas

```
root
├── src/
├── tests/
└── README.md
```
