# Archivé API — Sprint 1

Back-end em **C# ASP.NET 8** com autenticação via **Cookie de Sessão** e persistência em arquivos JSON.

No modo local, a API roda sem Docker e sem banco relacional:
- usuários em `DataStore/users.json`
- catálogo/histórico em `DataStore/catalog.json`
- wishlist/tracking por usuário em `DataStore/users.json`

---

## Pré-requisitos

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)

---

## Como rodar

### Rodar a API

```bash
cd Archivé.API
dotnet run
```

A API sobe em `https://localhost:7000` e `http://localhost:5000`.  
O Swagger estará disponível em: `https://localhost:7000/swagger`

> Todos os dados da API persistem em arquivos JSON dentro da pasta `DataStore/`.

---

## Endpoints — Sprint 1

### UC01 · Cadastro & Login

| Método | Rota | Auth | Descrição |
|--------|------|------|-----------|
| `POST` | `/api/auth/register` | ❌ | Cadastra novo usuário e inicia sessão |
| `POST` | `/api/auth/login` | ❌ | Autentica e inicia sessão (cookie) |
| `POST` | `/api/auth/logout` | ✅ | Encerra a sessão |
| `GET`  | `/api/auth/me` | ✅ | Retorna dados do usuário autenticado |

**Exemplo — Register:**
```json
POST /api/auth/register
{
  "name": "João Silva",
  "email": "joao@email.com",
  "password": "senha123"
}
```

---

### UC02 · Busca de Item

| Método | Rota | Auth | Descrição |
|--------|------|------|-----------|
| `GET`  | `/api/items` | ❌ | Busca itens com filtros (paginado) |
| `GET`  | `/api/items/{id}` | ❌ | Retorna item por ID |
| `POST` | `/api/items` | ✅ | Cadastra novo item de moda |

**Query params para busca:**
| Param | Tipo | Descrição |
|-------|------|-----------|
| `query` | string | Busca por nome ou marca |
| `brand` | string | Filtra por marca exata |
| `category` | string | Filtra por categoria |
| `minPrice` | decimal | Preço mínimo |
| `maxPrice` | decimal | Preço máximo |
| `page` | int | Página (default: 1) |
| `pageSize` | int | Itens por página (default: 20, max: 100) |

---

### UC03 · Histórico de Preços

| Método | Rota | Auth | Descrição |
|--------|------|------|-----------|
| `GET`  | `/api/items/{id}/price-history` | ❌ | Histórico de preços do item |
| `POST` | `/api/items/{id}/price-history` | ✅ | Adiciona registro de preço |

**Query params para histórico:**
| Param | Tipo | Descrição |
|-------|------|-----------|
| `from` | DateTime | Data inicial |
| `to`   | DateTime | Data final |

---

### UC04 · Wishlist & Tracking

**Wishlist** (itens salvos para referência):

| Método | Rota | Auth | Descrição |
|--------|------|------|-----------|
| `GET`    | `/api/wishlist` | ✅ | Lista wishlist do usuário |
| `POST`   | `/api/wishlist` | ✅ | Adiciona item à wishlist |
| `DELETE` | `/api/wishlist/{entryId}` | ✅ | Remove item da wishlist |

**Tracking** (itens monitorados com preço-alvo):

| Método | Rota | Auth | Descrição |
|--------|------|------|-----------|
| `GET`    | `/api/tracking` | ✅ | Lista itens em tracking |
| `POST`   | `/api/tracking` | ✅ | Adiciona item ao tracking |
| `PATCH`  | `/api/tracking/{trackedId}/target-price` | ✅ | Atualiza preço-alvo |
| `DELETE` | `/api/tracking/{trackedId}` | ✅ | Remove do tracking |

---

## Estrutura do projeto

```
Archivé.API/
├── Controllers/
│   ├── AuthController.cs
│   ├── ItemsController.cs
│   └── WishlistAndTrackingController.cs
├── DataStore/
│   ├── users.json
│   └── catalog.json
├── DTOs/
│   └── DTOs.cs
├── Models/
│   ├── FashionItem.cs
│   ├── PriceHistory.cs
│   ├── TrackedItem.cs
│   ├── User.cs
│   └── WishlistEntry.cs
├── appsettings.json
├── appsettings.Development.json
└── Program.cs
```

---

## Stack

| Camada | Tecnologia |
|--------|-----------|
| Framework | ASP.NET 8 |
| Persistência | Arquivos JSON (DataStore) |
| Auth | Cookie de Sessão |
| Hash senha | BCrypt.Net |
| Docs | Swagger / OpenAPI |
| Cache (Sprint 2+) | Redis |
