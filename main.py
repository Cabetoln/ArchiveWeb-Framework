# main.py
import torch, io
from fastapi import FastAPI, UploadFile, File
from fastapi.middleware.cors import CORSMiddleware
from sentence_transformers import SentenceTransformer, util
from PIL import Image

app = FastAPI()

app.add_middleware(
    CORSMiddleware,
    allow_origins=["http://localhost:5173", "http://127.0.0.1:5173"],
    allow_methods=["*"],
    allow_headers=["*"],
)

model = SentenceTransformer("clip-ViT-B-32")
catalog = torch.load("catalog_embeddings.pt", weights_only=False)

print(f"✓ Catálogo carregado: {len(catalog['items'])} itens")


@app.post("/search")
async def search_by_image(image: UploadFile = File(...)):
    img_bytes = await image.read()
    pil_image = Image.open(io.BytesIO(img_bytes)).convert("RGB")

    query_emb = model.encode(pil_image, convert_to_tensor=True)
    scores = util.cos_sim(query_emb, catalog["embeddings"])[0]

    top_k = torch.topk(scores, k=12)

    return [
        {**catalog["items"][i], "score": round(float(scores[i]), 4)}
        for i in top_k.indices
        if float(scores[i]) > 0.20
    ]


@app.get("/health")
def health():
    return {"status": "ok", "items": len(catalog["items"])}