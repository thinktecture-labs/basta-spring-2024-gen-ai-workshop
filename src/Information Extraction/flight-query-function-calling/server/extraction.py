#! /usr/bin/env python
from typing import List
from langchain.chains import create_extraction_chain_pydantic
from langchain_core.pydantic_v1 import BaseModel, Field
from langchain_openai import ChatOpenAI
from datetime import datetime

llm = ChatOpenAI(
    #base_url="http://localhost:8083/v1",
    model_name="gpt-4-turbo-preview",
    temperature=0,
    max_tokens=2000
)

class FlightSearch(BaseModel):
    departure_airport: str
    destination_airport: str
    departure_date: str
    return_date: str
    airlines: List[str]
    booking_class: str = Field(default='economy')
    number_of_persons: int

chain = create_extraction_chain_pydantic(
    pydantic_schema=FlightSearch,
    llm=llm)

def parse(query: str):
    input = f"""
    Current Date: {datetime.today()}
    Query: {query}
    """
    return chain.run(input)[0]