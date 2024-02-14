# Flight Query - Information Extraction

Eine Demo, die veranschaulicht wie man mittels [Kor](https://eyurtsev.github.io/kor/index.html) aus einer Anfrage eines Benutzer Formularfelder befuellen kann. Das kann nuetzlich sein, um z.B. Filtereinstellungen mittels natuerlicher Sprache zu definieren.

## Schema

Das JSON-Schema für Kor ist in der Datei [schema.json](./server/schema.json) definiert. Siehe dazu auch [Schema from JSON](https://eyurtsev.github.io/kor/schema_from_json.html)

## Client

Der Client ist eine sehr simple SPA mit VanillaJS gebaut und kann gestartet werden, in dem die Datei [index.html](./client/workspace/index.html) im Browser geoeffnet wird.

## Server

Der Server ist eine FastAPI, die per HTTP POST auf `/query` eine Query entgegennimmt. Das Result des Aufrufs ist die geparste Anfrage.

Der Server kann wie folgt gestartet werden.

```bash
python ./server/server.py
```
