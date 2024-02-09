from langchain_openai import OpenAI
from langchain_community.utilities import SQLDatabase
from langchain_experimental.sql import SQLDatabaseChain

# Setup DB connection, LLM provider and chain
db = SQLDatabase.from_uri(
    "postgresql+psycopg2://chinook_reader:<Strong!Passw0rd>@localhost:5432/chinook"
)

llm = OpenAI(temperature=0, verbose=True)

db_chain = SQLDatabaseChain.from_llm(llm=llm, db=db, verbose=True)

# Prepare prompt and set up user interaction

system_prompt = """
You're a helpful assistant that helps people query their music shop database.
Given an input question, first create a syntactically correct POSTGRESQL query to run, then look at the results of the query and return an answer to the user.

Use the following format, NEVER use JSON for the final answer:

Question: Question here
SQLQuery: SQL Query to run
SQLResult: Result of the SQL Query
Answer: Final answer here

Question: {question}
"""

def get_prompt():
    print("Type 'exit' to quit")

    while True:
        prompt = input("Enter a prompt: ")

        if prompt.lower() == 'exit':
            print('Exiting...')
            break
        else:
            try:
                question = system_prompt.format(question=prompt)
                print(db_chain.invoke(question))
            except Exception as e:
                print(e)

get_prompt()
