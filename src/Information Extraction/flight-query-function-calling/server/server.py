#! /usr/bin/env python

import extraction

from fastapi import FastAPI
from fastapi.encoders import jsonable_encoder
from fastapi.middleware.cors import CORSMiddleware
from pydantic import BaseModel

c = None

app = FastAPI()
app.add_middleware(
    CORSMiddleware,
    allow_origins=["*"],
    allow_methods=["POST"]
)

class Query(BaseModel):
    query: str

@app.post("/extract")
async def parse(query: Query):
    result = extraction.parse(query.query)
    return jsonable_encoder(result)

if __name__ == "__main__":
    import uvicorn
    uvicorn.run(app, host="0.0.0.0", port=8000)