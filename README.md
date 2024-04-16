# Gabbro Secret Manager

A super simple secret manager for my home server deployment automation.

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

![image](https://github.com/haondt/Gabbro-Secret-Manager/assets/19233365/7c483bd0-2c0c-4034-b386-48c7f9279618)

Details:
- Api must be called with an api key as a bearer token

Endpoints:
- `GET` - `{server-url}/api/secrets` - returns all secrets
- `GET` - `{server-url}/api/secrets/{SECRET_NAME}` - returns a particular secret

## Export user data

![image](https://github.com/haondt/Gabbro-Secret-Manager/assets/19233365/ae7b7f23-81ed-404e-898f-dd1d56dea781)

