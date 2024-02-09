#! /usr/bin/env python

import filter

from fastapi import FastAPI
from fastapi.responses import Response, JSONResponse
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

@app.post("/filter")
async def parse(query: Query):
    result = filter.parse(query.query)
    return JSONResponse(result["flight_search"])

if __name__ == "__main__":
    import uvicorn
    uvicorn.run(app, host="0.0.0.0", port=8000)