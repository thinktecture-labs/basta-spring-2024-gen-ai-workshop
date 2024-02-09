# Demo: Talk to my data - SQL

You will need an OpenAI api key for this demo to run.

## Preparation

First, let's setup the database:
```pwsh
docker run -d --name talk-to-my-data-pgsql-db -e "POSTGRES_PASSWORD=<Strong!Passw0rd>" -p 5432:5432 -v ${PWD}/db-init:/mnt/db postgres:16.1

docker container exec -it talk-to-my-data-pgsql-db bash
chmod +x /mnt/db/setup.sh
/mnt/db/setup.sh
exit
```

After that initial setup, you can use `docker start --name talk-to-my-data-pgsql-db` to restart the container when it was stopped.

Then, create the python environment:
```pwsh
conda create --name talk-to-my-data-sql python=3.12 -y
conda env config vars set --name talk-to-my-data-sql OPENAI_API_KEY={YOUR OPENAI API KEY}

conda activate talk-to-my-data-sql
pip install -r requirements.txt
```

After that initial setup, you can simply use `conda activate talk-to-my-data-sql` to use this environment.

## Run the demo

```pwsh
python ./db.py
```
