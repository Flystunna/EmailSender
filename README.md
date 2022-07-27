# EmailSender

Requirements:
Provisioned mssql database to log email trials
smtp credentials 

All these need to be filled in the appsettings.json file

Application auto creates and auto migrates every migration history
APIs are protected by header X-ApiKey.

This is needed when making requests to the API.
Value of X-ApiKey is stored in appsettings.json.

Documentation URL is at {baseurl}/swagger

API has 3 endpoints

onboarduser
resend
getalllogs
