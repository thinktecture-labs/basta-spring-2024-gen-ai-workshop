# Compare embeddings

This notebook let's you run the same query against different embeddings to compare their outputs.

## Install requirements

To run the required vector database locally, use this command:
`docker run --name 04-compare-embeddings-demo-vectordb -p 6333:6333 -p 6334:6334 -d qdrant/qdrant`

Prepare the environment:
```
conda create --name basta2024-04-compare-embeddings python=3.9.5 -y
conda install --name basta2024-04-compare-embeddings pip==23.3 -y --force

conda env config vars set --name basta2024-04-compare-embeddings OPENAI_API_KEY={YOUR OPENAI API KEY}
conda env config vars set --name basta2024-04-compare-embeddings HUGGINGFACEHUB_API_TOKEN={YOUR HUGGINGFACE API TOKEN}

conda activate basta2024-04-compare-embeddings
pip install -r requirements.txt
```

## Usage

Run the jupyter notebook.
