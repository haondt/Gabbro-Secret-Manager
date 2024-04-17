# Gabbro Secret Manager

A super simple secret manager for my home server deployment automation.

# Deployment

The following items must be configured:
- `JweSettings.EncryptionKey`: 32-byte encryption key for encrypting api tokens
- `JweSettings.SigningKey`: (optional, default can be used as well) 32-byte signing key for signing api tokens
- `MongoDbSettings.ConnectionString`: connection string for mongo db
- `MongoDbSettings.DatabaseName`: mongodb database to use

See `docker-compose.yml` for an example setup.

# Features

## User accounts

![image](https://github.com/haondt/Gabbro-Secret-Manager/assets/19233365/5d3c8eab-109e-4c48-ab10-5e6eeff1ae0c)


## View, search and filter existing secrets

![image](https://github.com/haondt/Gabbro-Secret-Manager/assets/19233365/0072fcf9-6cc9-4ba8-9b40-3cb1c7a909cf)

## Edit / create new secrets

![image](https://github.com/haondt/Gabbro-Secret-Manager/assets/19233365/79f4761b-2c71-422f-849a-c581432a0688)

## Create or delete api keys

![image](https://github.com/haondt/Gabbro-Secret-Manager/assets/19233365/a32fc038-c2e7-4b66-90f7-452ceac4052d)

## Retrieve secrets via api

![image](https://github.com/haondt/Gabbro-Secret-Manager/assets/19233365/28444cdc-9d57-4c05-be95-8b362110fd2a)


Details:
- Api must be called with an api key as a bearer token

Endpoints:
- `GET` - `{server-url}/api/secrets` - returns all secrets
  - also supports query items
  - `name` - filter the name of the secret
  - `tags` - filter for secrets that contain all given tags
- `GET` - `{server-url}/api/secret/{SECRET_ID}` - returns a particular secret
- `GET` - `export-data` - gets all user data

## Export user data

![image](https://github.com/haondt/Gabbro-Secret-Manager/assets/19233365/ae7b7f23-81ed-404e-898f-dd1d56dea781)

