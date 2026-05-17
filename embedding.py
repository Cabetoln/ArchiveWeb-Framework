import json, requests, torch, io
from PIL import Image
from sentence_transformers import SentenceTransformer

model = SentenceTransformer("clip-ViT-B-32")

# Carrega seus itens (pode vir do banco ou de um JSON)
with open("Archive.API/DataStore/catalog.json") as f:
    items = json.load(f)["FashionItems"]

embeddings = []
valid_items = []

for item in items:
    try:
        resp = requests.get(item["ImageUrl"], timeout=10)
        img = Image.open(io.BytesIO(resp.content)).convert("RGB")
        emb = model.encode(img, convert_to_tensor=True)
        embeddings.append(emb)
        valid_items.append(item)
        print(f"✓ {item['Name']}")
    except Exception as e:
        print(f"✗ {item['Name']}: {e}")

torch.save({
    "embeddings": torch.stack(embeddings),  # tensor de shape [300, 512]
    "items": valid_items
}, "catalog_embeddings.pt")