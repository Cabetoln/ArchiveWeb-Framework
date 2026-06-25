# Arquitetura do Framework — Archivé

Diagrama de classes (UML) do `Archive.API`, evidenciando a separação entre o
**núcleo congelado do framework** (estável, agnóstico de domínio) e o
**plugin de domínio** (`Fashion`), que implementa os pontos flexíveis.

## Como o framework funciona

- `Core/Contracts` define os **pontos flexíveis** (hot spots) como interfaces.
- `Fashion/` é um **plugin de domínio** que realiza essas interfaces. Trocá-lo por
  `Books`, `Electronics`, etc. produz outra aplicação **sem alterar o núcleo**.
- As camadas estáveis (`Controllers → Services → Repositories`) dependem **apenas
  das abstrações** (Inversão de Dependência).
- `Program.cs` é a **raiz de composição**: injeta as implementações `Fashion` via DI.
- O plugin `Fashion` apenas **encapsula** os serviços Python externos (CLIP+SAM e o scraper).

## Diagrama de classes

```mermaid
classDiagram
    direction LR

    %% ───────────────────────── Núcleo do Framework (pontos flexíveis) ─────────────────────────
    namespace CoreContracts {
        class IProductSchema {
            <<interface>>
            +DomainName : string
            +Attributes : IReadOnlyList~AttributeDefinition~
            +FavoritableGrouping : IFavoritableGrouping
            +Build(CreateProductRequest) Product
            +ApplyFilters(IQueryable~Product~, SearchProductsRequest) IQueryable~Product~
        }
        class IImageSearchProvider {
            <<interface>>
            +IsAvailable : bool
            +SearchAsync(Stream, int, CancellationToken) Task
        }
        class IPriceScraper {
            <<interface>>
            +Name : string
            +ScrapeAsync(CancellationToken) Task
        }
        class IFavoritableGrouping {
            <<interface>>
            +Key : string
            +DisplayName : string
            +ExtractValue(Product) string
        }
    }

    %% ───────────────────────── Plugin de domínio: Fashion ─────────────────────────
    namespace Fashion {
        class FashionProductSchema {
            +DomainName : string
            +Attributes : IReadOnlyList~AttributeDefinition~
            +FavoritableGrouping : IFavoritableGrouping
            +Build(CreateProductRequest) Product
            +ApplyFilters(...) IQueryable~Product~
        }
        class ClipImageSearchProvider {
            -_baseUrl : string
            +IsAvailable : bool
            +SearchAsync(...) Task
        }
        class FarfetchPriceScraper {
            +Name : string
            +ScrapeAsync(...) Task
        }
        class FashionBrandGrouping {
            +Key : string
            +DisplayName : string
        }
    }

    %% ───────────────────────── Núcleo congelado (orquestração) ─────────────────────────
    namespace Aplicacao {
        class ItemsController
        class FavoriteGroupsController
        class ICatalogService {
            <<interface>>
        }
        class CatalogService
        class IFavoriteGroupsService {
            <<interface>>
        }
        class FavoriteGroupsService
        class IItemRepository {
            <<interface>>
        }
        class Program {
            <<raiz de composicao>>
        }
    }

    %% ───────────────────────── Serviços externos (Python) ─────────────────────────
    namespace ServicosExternos {
        class PythonImageService {
            <<externo: main.py>>
            CLIP e SAM3
        }
        class PythonScraper {
            <<externo: scrape_prices.py>>
            Farfetch
        }
    }

    %% ── Realização dos pontos flexíveis (plugin implementa as interfaces) ──
    IProductSchema <|.. FashionProductSchema
    IImageSearchProvider <|.. ClipImageSearchProvider
    IPriceScraper <|.. FarfetchPriceScraper
    IFavoritableGrouping <|.. FashionBrandGrouping

    %% ── Relações internas do schema ──
    IProductSchema ..> IFavoritableGrouping : usa
    FashionProductSchema --> FashionBrandGrouping : compõe

    %% ── Núcleo congelado depende apenas das abstrações (DIP) ──
    ItemsController ..> ICatalogService
    ItemsController ..> IImageSearchProvider
    FavoriteGroupsController ..> IFavoriteGroupsService
    CatalogService ..|> ICatalogService
    CatalogService ..> IProductSchema
    CatalogService ..> IPriceScraper
    CatalogService ..> IItemRepository
    FavoriteGroupsService ..|> IFavoriteGroupsService
    FavoriteGroupsService ..> IProductSchema

    %% ── Raiz de composição liga o plugin ao núcleo (DI) ──
    Program ..> FashionProductSchema : registra
    Program ..> ClipImageSearchProvider : registra
    Program ..> FarfetchPriceScraper : registra

    %% ── Plugin encapsula os serviços Python ──
    ClipImageSearchProvider ..> PythonImageService : HTTP POST /search
    FarfetchPriceScraper ..> PythonScraper : Process --scrape-only
```

## Legenda da notação

| Notação | Significado |
|---|---|
| `<|..` (tracejada, triângulo vazado) | **Realização** — classe `Fashion` implementa a interface do `Core` |
| `..>` (tracejada, seta) | **Dependência** — "usa" / depende da abstração |
| `-->` (cheia, seta) | **Associação/composição** |
| `<<interface>>` | Estereótipo de interface (ponto flexível) |

> Substituir o pacote **Fashion** por outro domínio (ex.: `Books` com
> `BookProductSchema`, `AuthorGrouping`, etc.) gera uma nova aplicação reaproveitando
> todo o núcleo — é isso que caracteriza o **framework**.
