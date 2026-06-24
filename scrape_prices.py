#!/usr/bin/env python3
"""
Busca preços atuais na Farfetch, adiciona itens novos e atualiza preços existentes.
Uso: python3 scrape_prices.py
"""

import re
import json
import sys
import subprocess
import urllib.request
import urllib.error

API_BASE = "http://localhost:5000"

CATEGORY_URLS = [
    ("Camisetas", "https://www.farfetch.com/br/shopping/men/t-shirts-2/items.aspx"),
    ("Jaquetas",  "https://www.farfetch.com/br/shopping/men/jackets-2/items.aspx"),
    ("Calcados",  "https://www.farfetch.com/br/shopping/men/shoes-2/items.aspx"),
    ("Calcas",    "https://www.farfetch.com/br/shopping/men/trousers-2/items.aspx"),
]


def fetch_with_curl(url: str) -> str:
    result = subprocess.run([
        "curl", "-s", "-L", "--max-time", "20", "--compressed",
        "-H", "User-Agent: Mozilla/5.0 (X11; Linux x86_64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/124.0.0.0 Safari/537.36",
        "-H", "Accept: text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,*/*;q=0.8",
        "-H", "Accept-Language: pt-BR,pt;q=0.9",
        "-H", "Upgrade-Insecure-Requests: 1",
        "-H", "Sec-Fetch-Dest: document",
        "-H", "Sec-Fetch-Mode: navigate",
        "-H", "Sec-Fetch-Site: none",
        "-H", "Sec-Fetch-User: ?1",
        url
    ], capture_output=True, text=True)
    return result.stdout


def scrape_farfetch() -> list[dict]:
    """Retorna lista de itens com name, brand, category, imageUrl, productUrl, price."""
    all_items = []
    seen_urls = set()

    for category, url in CATEGORY_URLS:
        print(f"  Buscando {category}...", end=" ", flush=True, file=sys.stderr)
        html = fetch_with_curl(url)

        match = re.search(
            r'application/ld\+json">(\{.*?"ItemList".*?\})</script>',
            html, re.DOTALL
        )
        if not match:
            print("sem dados (bloqueado?)", file=sys.stderr)
            continue

        data = json.loads(match.group(1))
        items = data.get("itemListElement", [])
        count = 0
        for item in items:
            offer = item.get("offers", {})
            relative_url = offer.get("url", "")
            price = offer.get("price")
            if not relative_url or price is None:
                continue

            full_url = "https://www.farfetch.com" + relative_url
            if full_url in seen_urls:
                continue
            seen_urls.add(full_url)

            images = item.get("image", [])
            all_items.append({
                "name": item.get("name", ""),
                "brand": item.get("brand", {}).get("name", ""),
                "category": category,
                "imageUrl": images[0] if images else None,
                "productUrl": full_url,
                "price": float(price),
                "currency": offer.get("priceCurrency", "BRL"),
            })
            count += 1

        print(f"{count} itens", file=sys.stderr)

    return all_items


def api_request(path: str, method: str = "GET", body: dict = None, cookie: str = None):
    url = API_BASE + path
    data = json.dumps(body).encode() if body else None
    req = urllib.request.Request(url, data=data, method=method)
    req.add_header("Content-Type", "application/json")
    if cookie:
        req.add_header("Cookie", cookie)
    with urllib.request.urlopen(req) as r:
        content = r.read()
        set_cookie = r.headers.get("Set-Cookie")
        return json.loads(content) if content else None, set_cookie


def login(email: str, password: str):
    try:
        _, set_cookie = api_request("/api/auth/login", "POST", {"email": email, "password": password})
        if set_cookie:
            cookie = set_cookie.split(";")[0]
            print(f"  Logado como {email}")
            return cookie
    except Exception as e:
        print(f"  Falha no login: {e}")
    return None


def get_catalog(cookie: str) -> list[dict]:
    items = []
    page = 1
    while True:
        data, _ = api_request(f"/api/items?page={page}&pageSize=100", cookie=cookie)
        items.extend(data["items"])
        if page >= data["totalPages"]:
            break
        page += 1
    return items


def add_item(item: dict, cookie: str) -> bool:
    try:
        api_request("/api/items", "POST", {
            "name": item["name"],
            "brand": item["brand"],
            "category": item["category"],
            "imageUrl": item["imageUrl"],
            "productUrl": item["productUrl"],
            "currentPrice": item["price"],
            "currency": item["currency"],
        }, cookie=cookie)
        return True
    except urllib.error.HTTPError as e:
        print(f"    Erro HTTP {e.code}: {e.read().decode()}")
        return False


def update_price(item_id: str, new_price: float, cookie: str) -> bool:
    try:
        api_request(
            f"/api/items/{item_id}/price-history", "POST",
            {"price": new_price, "currency": "BRL", "source": "farfetch_scraper"},
            cookie=cookie
        )
        return True
    except Exception:
        return False


def main():
    print("=== Archivé — Atualização de Preços ===\n")

    print("1. Raspando Farfetch...")
    scraped = scrape_farfetch()
    print(f"   Total raspado: {len(scraped)} itens\n")

    if not scraped:
        print("Nenhum dado obtido. Encerrando.")
        sys.exit(1)

    print("2. Autenticando na API...")
    email = input("   Email: ")
    password = input("   Senha: ")
    cookie = login(email, password)
    if not cookie:
        sys.exit(1)

    print("\n3. Verificando catálogo...")
    catalog = get_catalog(cookie)
    catalog_by_url = {item["productUrl"]: item for item in catalog if item.get("productUrl")}
    print(f"   {len(catalog)} itens no catálogo\n")

    added = 0
    updated = 0
    unchanged = 0

    for item in scraped:
        url = item["productUrl"]

        if url not in catalog_by_url:
            # Item novo — adiciona
            ok = add_item(item, cookie)
            if ok:
                print(f"  + NOVO: {item['brand']} — {item['name'][:40]} (R$ {item['price']:,.2f})")
                added += 1
        else:
            # Item existente — verifica preço
            existing = catalog_by_url[url]
            old_price = existing["currentPrice"]
            new_price = item["price"]

            if new_price == old_price:
                unchanged += 1
                continue

            ok = update_price(existing["id"], new_price, cookie)
            if ok:
                print(f"  ~ {item['brand']} — {item['name'][:40]}")
                print(f"    R$ {old_price:,.2f} -> R$ {new_price:,.2f}")
                updated += 1

    print(f"\n=== Concluido ===")
    print(f"  Adicionados    : {added}")
    print(f"  Atualizados    : {updated}")
    print(f"  Sem alteracao  : {unchanged}")

    if cookie:
        print("\n4. Processando análise sazonal...")
        try:
            api_request("/api/seasonal-analysis/process", "POST", {}, cookie)
            print("  Análise sazonal processada com sucesso.")
        except Exception as e:
            print(f"  Falha ao processar análise sazonal: {e}")


def scrape_only():
    """Modo de encapsulamento: apenas raspa e emite os itens como JSON no stdout.

    Usado pelo framework (Archive.API -> FarfetchPriceScraper) para consumir o
    scraper como uma implementação de IPriceScraper, sem o fluxo interativo de
    login/sincronização. O progresso vai para stderr para manter o stdout limpo.
    """
    items = scrape_farfetch()
    json.dump(items, sys.stdout, ensure_ascii=False)


if __name__ == "__main__":
    if "--scrape-only" in sys.argv:
        scrape_only()
    else:
        main()
