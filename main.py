import torch, io, cv2
import numpy as np
from fastapi import FastAPI, UploadFile, File
from fastapi.middleware.cors import CORSMiddleware
from sentence_transformers import SentenceTransformer, util
from ultralytics.models.sam import SAM3SemanticPredictor
from PIL import Image
from fastapi import Response

app = FastAPI()

app.add_middleware(
    CORSMiddleware,
    allow_origins=["http://localhost:5173", "http://127.0.0.1:5173"],
    allow_methods=["*"],
    allow_headers=["*"],
)

clip_model = SentenceTransformer("clip-ViT-B-32")
catalog = torch.load("catalog_embeddings.pt", weights_only=False)

sam_overrides = dict(
    conf=0.25,
    task="segment",
    mode="predict",
    model="sam3.pt",
    half=True,
)
sam = SAM3SemanticPredictor(overrides=sam_overrides)

CLOTHING_PROMPTS = [
    "t-shirt", "pants",
    "shoes", "bag", "hoodie", "shorts"
]

MIN_AREA_RATIO = 0.02  # peça precisa ocupar pelo menos 2% da imagem

print(f"✓ Catálogo carregado: {len(catalog['items'])} itens")


def extract_masked_crops(pil_image, img_np, results):
    """Extrai crops mascarados (fundo branco) das máscaras retornadas pelo SAM."""
    H, W = img_np.shape[:2]
    img_area = H * W
    crops = []

    for result in results:
        if result.masks is None:
            continue
        masks = result.masks.data.cpu().numpy()  # (N, H, W)
        for mask in masks:
            mask_uint = (mask > 0).astype(np.uint8)

            # filtro p ruído
            if mask_uint.sum() / img_area < MIN_AREA_RATIO:
                continue

            x, y, w, h = cv2.boundingRect(mask_uint)
            if w < 50 or h < 50:
                continue

            # mask pixels da peça, resto vira branco
            masked = img_np.copy()
            masked[mask_uint == 0] = 255

            # crop pro bbox pra remover espaço vazio em volta
            crop_array = masked[y:y + h, x:x + w]
            crops.append(Image.fromarray(crop_array))

    return crops


@app.post("/search")
async def search_by_image(image: UploadFile = File(...)):
    img_bytes = await image.read()
    pil_image = Image.open(io.BytesIO(img_bytes)).convert("RGB")
    img_np = np.array(pil_image)

    sam.set_image(img_np)
    results = sam(text=CLOTHING_PROMPTS)

    crops = extract_masked_crops(pil_image, img_np, results)

    if not crops:
        crops = [pil_image]

    seen_ids = set()
    all_matches = []

    for crop in crops:
        query_emb = clip_model.encode(crop, convert_to_tensor=True)
        scores = util.cos_sim(query_emb, catalog["embeddings"])[0]
        top_k = torch.topk(scores, k=3)

        for i, score in zip(top_k.indices, top_k.values):
            score_val = float(score)
            if score_val < 0.3:
                continue
            item = catalog["items"][i]
            item_id = item["Id"]
            if item_id in seen_ids:
                continue
            seen_ids.add(item_id)
            all_matches.append({**item, "score": round(score_val, 4)})

    all_matches.sort(key=lambda x: x["score"], reverse=True)
    return all_matches[:12]


@app.get("/health")
def health():
    return {"status": "ok", "items": len(catalog["items"])}


@app.post("/search/debug")
async def search_debug(image: UploadFile = File(...)):
    img_bytes = await image.read()
    pil_image = Image.open(io.BytesIO(img_bytes)).convert("RGB")
    img_np = np.array(pil_image)
    H, W = img_np.shape[:2]
    img_area = H * W

    sam.set_image(img_np)
    results = sam(text=CLOTHING_PROMPTS)

    output = img_np.copy()
    overlay = output.copy()
    colors = [(255, 0, 0), (0, 255, 0), (0, 0, 255), (255, 255, 0),
              (255, 0, 255), (0, 255, 255), (255, 128, 0), (128, 0, 255)]

    idx = 0
    for result in results:
        if result.masks is None:
            continue
        masks = result.masks.data.cpu().numpy()
        for mask in masks:
            mask_uint = (mask > 0).astype(np.uint8)
            if mask_uint.sum() / img_area < MIN_AREA_RATIO:
                continue
            color = colors[idx % len(colors)]
            overlay[mask_uint == 1] = color
            x, y, w, h = cv2.boundingRect(mask_uint)
            cv2.rectangle(output, (x, y), (x + w, y + h), color, 3)
            cv2.putText(output, f"#{idx}", (x, y - 10),
                        cv2.FONT_HERSHEY_SIMPLEX, 1, color, 2)
            idx += 1

    output = cv2.addWeighted(output, 0.6, overlay, 0.4, 0)
    output_bgr = cv2.cvtColor(output, cv2.COLOR_RGB2BGR)
    _, encoded = cv2.imencode(".jpg", output_bgr)
    return Response(content=encoded.tobytes(), media_type="image/jpeg")


@app.post("/search/crops")
async def search_crops(image: UploadFile = File(...)):
    """Retorna os crops mascarados lado a lado, pra inspeção visual."""
    img_bytes = await image.read()
    pil_image = Image.open(io.BytesIO(img_bytes)).convert("RGB")
    img_np = np.array(pil_image)

    sam.set_image(img_np)
    results = sam(text=CLOTHING_PROMPTS)

    crops = extract_masked_crops(pil_image, img_np, results)

    if not crops:
        return Response(content=b"", media_type="image/jpeg")

    target_h = 400
    resized = []
    for crop in crops:
        ratio = target_h / crop.height
        new_w = int(crop.width * ratio)
        resized.append(np.array(crop.resize((new_w, target_h))))

    concat = np.concatenate(resized, axis=1)
    concat_bgr = cv2.cvtColor(concat, cv2.COLOR_RGB2BGR)
    _, encoded = cv2.imencode(".jpg", concat_bgr)
    return Response(content=encoded.tobytes(), media_type="image/jpeg")