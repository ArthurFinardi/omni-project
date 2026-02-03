# Arquitetura do Projeto

## Introducao

Este projeto é para estudo. O foco e verificar dominio de arquitetura, padroes, boas praticas, qualidade de codigo e capacidade de entrega incremental.

Competencias abordadas:
- C# e .NET 8
- Separacao de camadas
- PostgreSQL e MongoDB
- Padroes de projeto (ex: Mediator)
- EF Core
- Testes com xUnit, mocks com NSubstitute e dados com Bogus
- AutoMapper
- Design de API REST
- Git Flow e commits semanticos
- Paginacao, filtros e ordenacao
- Tratamento de erros e formato padrao
- Programacao assincrona
- Performance e boas praticas

## Modelo de Separacao de Camadas

Arquitetura em camadas (DDD + Clean Architecture), com dependencias sempre apontando para o centro:

1) **Domain** (`src/DeveloperStore.Domain`)
   - Entidades e regras de negocio.
   - Value Objects e excecoes de dominio.
   - Nenhuma dependencia externa.

2) **Application** (`src/DeveloperStore.Application`)
   - Casos de uso (Commands/Queries + Handlers via MediatR).
   - Contratos (DTOs) e Result.
   - Interfaces de repositorio e publicacao de eventos.

3) **Infrastructure** (`src/DeveloperStore.Infra`)
   - Implementacoes tecnicas (EF Core, Postgres e Mongo).
   - Repositorios e mapeamentos de persistencia.

4) **API** (`src/DeveloperStore.Api`)
   - Endpoints REST, DI, configuracoes e middlewares.
   - Publicacao de eventos via logger.

5) **Tests** (`tests/DeveloperStore.Tests`)
   - Testes unitarios do dominio e application.

Fluxo de dependencias:
```
API -> Application -> Domain
API -> Infrastructure -> Application -> Domain
Tests -> Application/Domain
```

## Padroes de Projeto Utilizados

- **Mediator (MediatR)**
  - Comandos e queries isolam casos de uso.
  - Handlers centralizam a orquestracao e facilitam testes.

- **Repository**
  - `ISaleRepository` e `ISaleReadRepository` definem contratos.
  - Implementacoes concretas em Infra (Postgres e Mongo).

- **CQRS**
  - Escrita com modelo relacional (PostgreSQL + EF Core).
  - Leitura com modelo denormalizado (MongoDB).

- **Value Object**
  - `Money`, `Quantity`, `Discount`, `ExternalIdentity`.
  - Encapsulam validacoes e regras invariantes.

- **Domain Model**
  - Entidades ricas (`Sale`, `SaleItem`) com comportamento.

- **Adapter (publicacao de eventos)**
  - `LogEventPublisher` abstrai publicacao sem broker.

## Programacao Sincrona e Assincrona

- **Sincrona**
  - Dominio: regras e calculos executam em memoria.

- **Assincrona**
  - Application, Infra e API usam `async/await`.
  - Operacoes de banco e publicacao de eventos.

## Persistencia e Dados

- **PostgreSQL**
  - Modelo relacional para escrita.
  - EF Core com mappings explicitos.

- **MongoDB**
  - Read model denormalizado (External Identities).
  - Consultas otimizadas para leitura.

## Paginacao, Ordenacao e Filtros

Padrao de API definido em `.doc/general-api.md`:

- `_page` (default 1)
- `_size` (default 10)
- `_order` (ex: `saleDate desc, totalAmount asc`)

Filtros:
- `campo=valor` (match exato)
- `campo=*valor` ou `campo=valor*` para match parcial
- `_minCampo` / `_maxCampo` para numerico/data

## Tratamento de Erros

Padrao de resposta de erro:
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

## Estrutura de Pastas

```
root
├── src/
├── tests/
└── README.md
```

