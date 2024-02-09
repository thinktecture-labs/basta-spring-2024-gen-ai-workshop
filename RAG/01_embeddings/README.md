# Embeddings

This notebook let's you see the semantic difference between several sentences.

Prepare the environment:
```
conda create --name basta2024-01-embeddings python=3.9.5 -y
conda install -n basta2024-01-embeddings pip==23.3 -y --force

conda env config vars set --name basta2024-01-embeddings HUGGINGFACEHUB_API_TOKEN={YOUR HUGGINGFACE API TOKEN}

conda activate basta2024-01-embeddings

pip install -r requirements.txt
```
