# Store and Retrieval

This notebook shows storing and retrieval.

repare the environment:
```
conda create --name 03-store-retrieval python=3.9.5 -y
conda install -n 03-store-retrieval pip -y --force

conda env config vars set --name 03-store-retrieval HUGGINGFACEHUB_API_TOKEN={YOUR HUGGINGFACE API TOKEN}
conda env config vars set --name 03-store-retrieval OPENAI_API_KEY={YOUR OPEN AI API KEY}

conda activate 03-store-retrieval
pip install -r requirements.txt
```
