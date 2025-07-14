# SurfSessions-API

API web ASP.NET Core (.NET 9) avec EF core et une BDD MySQL, back-end de l'application **Sessions**.  
→ Angular front-end de l'application **Sessions** : [SurfSessions-Web](https://github.com/gdelaunay/SurfSessions-Web)  
<br>
Elle permet aux utilisateurs de créer des spots avec une position géographique, et d'obtenir des prévisions de 
météo marines détaillées (houle, vent...) pour ceux-ci via l'API [Open-Meteo](https://open-meteo.com/).  
Les utilisateurs peuvent paramétrer un spot en indiquant leurs conditions idéales, et peuvent activer les notifications 
par mail pour être alertés lorsque les prévisions météo correspondent à ces conditions idéales.  
L'application permet aussi d'enregistrer ses sessions, rattachées à un spot et aux prévisions météo du jour, ainsi qu'à 
une note sur 5, des photos et/ou un commentaire, fournissant un journal complet des sessions passées.


## Sommaire

- [Prérequis](#prérequis)
- [Installation](#installation)
- [Développement](#développement)
- [Développement du front-end](#développement-du-front-end)
- [Déploiement](#déploiement)
- [Déploiement HTTPS](#déploiement-https)
- [Arrêt et nettoyage](#arrêt-et-nettoyage)
- [Ressources additionnelles](#ressources-additionnelles)
- [License](#license)


## Prérequis

S'assurer d'avoir le SDK **.NET 9**, **Docker** et **Docker compose** d'installés.

1. Vérifier que le SDK **.NET 9** est installé :
```bash
dotnet --version
```
Si la commande n'est pas trouvée ou que la version n'est pas ``9.x.x``, installer [.NET 9](https://dotnet.microsoft.com/download/dotnet/9.0).

2. Vérifier que **Docker** et **Docker compose** sont installés :
```bash
docker --version
docker compose version
```
Si les commandes ne sont pas trouvées, installer [Docker](https://docs.docker.com/get-docker/).


## Installation

1. Cloner le dépôt :
```bash
git clone https://github.com/gdelaunay/SurfSessions-API.git
cd SurfSessions-API
```

2. Copier le fichier ``.env`` d'exemple :
```bash
cp .env.example .env
```
Et modifier les valeurs ``user``, ``password`` et ``root_password``.


## Développement

Pour lancer l'API en mode développement :

1. Décommenter la ligne ``Développement`` dans la section ``mysql`` du fichier ``compose.yaml``, pour ouvrir le port du container de la BDD :
```yaml
     ports:
       - "3306:3306"  # Développement
```

2. Lancer le container de la BDD MySQL avec Docker compose :
```bash
docker compose up mysql
```
3. Remplacer dans le fichier ``Program.cs - ligne:29`` la variable ``MYSQL_CONNECTION_STRING`` par ``MYSQL_CONNECTION_STRING_DEV`` :
```csharp
builder.Services.AddDbContext<AppDbContext>(options => options.UseMySQL(Environment.GetEnvironmentVariable("MYSQL_CONNECTION_STRING_DEV")!));
```
4. Lancer l'API .NET via l'IDE ou via cette commande à la racine du projet :
```bash
dotnet run
```
Une fois que l'API est lancée, elle est disponible à l'adresse [`http://localhost:5050/`](http://localhost:5050/).  
Sa documentation OpenAPI est consultable à l'adresse [`http://localhost:5050/openapi/v1.json`](http://localhost:5050/openapi/v1.json)
et téléchargeable directement à celle-ci [`http://localhost:5050/openapi/download`](http://localhost:5050/openapi/download).


## Développement du front-end

>**Nécessite d'effectuer les étapes 1 et 3 de la section [Développement](#développement).**

Pour lancer l'API en mode développement du front-end Angular [SurfSessions-Web](https://github.com/gdelaunay/SurfSessions-Web) :
1. Décommenter la ligne ``Développement`` dans la section ``api`` du fichier ``compose.yaml``, pour ouvrir le port du container de l'API :
```yaml
    ports:
      - "5050:5050"   # Développement
```
2. Lancer le container de la BDD MySQL et de l'API .NET avec Docker compose :
```bash
docker compose up mysql api
```


## Déploiement

>**Si les modifications de la section [Développement](#développement) ont été appliquées, rétablir l’état initial.**  

Le déploiement se fait à l'aide du fichier Docker ``compose.yaml`` qui embarque l'API .NET, la BDD MySQL, le front-end Angular, et un reverse-proxy NGINX :

1. Si ce n'est pas déjà fait, installer le projet [SurfSessions-Web](https://github.com/gdelaunay/SurfSessions-Web) dans
un nouveau dossier et construire son image Docker (requise par le  ``compose.yaml``) :
```bash
git clone https://github.com/gdelaunay/SurfSessions-Web.git
cd SurfSessions-Web
docker compose build
```
2. Lancer l'application complète, depuis le dossier du projet SurfSessions-API :
```bash
docker compose up --build
```
Une fois tous les containers lancés, l'application est disponible à l'adresse [`http://localhost:80/`](http://localhost:80/).


## Déploiement HTTPS

>**Nécessite d'effectuer l'étape 1 de la section [Déploiement](#déploiement).**

Un support du protocole HTTPS est préconfiguré, nécessitant de disposer d'un nom de domaine. Pour déployer avec HTTPS :

1. Décommenter les lignes ``HTTPS`` dans la section ``rproxy`` du fichier ``compose.yaml``, pour ouvrir le port HTTPS et monter le volume des certificats :
```yaml
    ports:
      - "80:80"
      - "443:443"   # HTTPS

    volumes:
      - ./nginx/conf.d:/etc/nginx/conf.d:ro
      - ./nginx/certs:/etc/nginx/certs:ro  # HTTPS
```
2. Décommenter la section server ``HTTPS`` dans le fichier ``nginx/conf.d/default.conf`` et remplacer la valeur de ``server_name``
par notre nom de domaine, ou sous-domaine:
```nginx configuration
# HTTPS 
server {
    listen 443 ssl;
    server_name mydomain.com;
    ssl_certificate /etc/nginx/certs/live/mydomain.com/fullchain.pem;
    ssl_certificate_key /etc/nginx/certs/live/mydomain.com/privkey.pem;

    location /api/ {
        proxy_pass http://sessions-api:5050;
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
    }

    location / {
        proxy_pass http://sessions-web:80;
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
    }
}
```
3. Pour générer un certificat avec **Let's Encrypt** dans un container Docker **Certbot**, exécuter cette commande 
depuis le dossier du projet, en remplaçant les paramètres par notre nom de domaine / sous-domaine, et adresse email :

>**Il faut s'assurer que notre nom de domaine pointe bien vers l'adresse ip publique de notre machine.**

>**S'assurer aussi que le port 80 de celle-ci soit disponible et accessible publiquement (qu'aucun pare-feu ne bloque les requêtes entrantes).**
```bash
docker run --rm -p 80:80 -v "${PWD}/nginx/certs:/etc/letsencrypt" certbot/certbot certonly --standalone -d mydomain.com --non-interactive --agree-tos -m admin@mydomain.com
```
Le certificat est généré dans le dossier ``nginx/certs/live/mydomain.com``, permettant de gérer d'autres domaines,
et monté dans un volume Docker pour le reverse-proxy.

4. Lancer l'application complète avec HTTPS :
```bash
docker compose up --build
```
Une fois tous les containers lancés, l'application est disponible à l'adresse [`https://mydomain.com/`](https://mydomain.com/).


## Arrêt et nettoyage

- Pour arrêter l'application et supprimer les conteneurs associés :
```bash
docker compose down
```
- Pour supprimer également les volumes :
>**⚠ Supprime toutes les données persistées (BDD, certificats).**
```bash
docker compose down -v
```
- Pour effectuer un nettoyage complet :
>**⚠ Supprime toutes les ressources liées au projet (containers, volumes, images, réseaux).**
```bash
docker compose down --rmi all -v
```


## Ressources additionnelles

- [Documentation ASP.NET Core](https://learn.microsoft.com/aspnet/core/?view=aspnetcore-9.0)
- [Documentation EF Core](https://learn.microsoft.com/ef/core/)
- [Documentation Docker compose](https://docs.docker.com/compose/)
- [Documentation reverse proxy NGINX](https://docs.nginx.com/nginx/admin-guide/web-server/reverse-proxy/)
- [Intégration d'OpenApi dans Postman](https://learning.postman.com/docs/integrations/available-integrations/working-with-openAPI/)


## License

[MIT](https://choosealicense.com/licenses/mit/)
