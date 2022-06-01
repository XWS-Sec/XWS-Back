# XWS-Back
*University project*
## 1. Repository for backand logic of Dislinkt

The backand is consisted of :
* BaseApi (Users microservice)
* Users.Graph microservice
* Post microservice
* Chat microservice
* JobOffers microservice

### - User microservice
Entry point to the system. Acts as integration part for external systems. Api knows all users and is required for authentication and authorization. All other requests are forwarded to corresponding endpoints based on the task at hand.

|Num| Environment variable | Description |
|--| -------------------- | ----------- |
|1 | XWS_PKI_ROOT_CERT_FOLDER | the folder where the certificate is stored |
|2 | XWS_PKI_ADMINPASS | the admin password for the certificate, also a base password for admins|
|3 | BaseApiMongoDb | mongo connection string |
|4 | XWS_FRONT_PATH_LOGGED | the base url to redirect after passwordless logins |
|5 | USER_PIC_DIR | base directory for storing user images |
|6 | XWS_FRONT_PATH | the base url of frontend |
|7 | POST_PIC_DIR | base directory for storing post images |
|8 | XWS_SENDGIRD | sendgrid api key for email service |
|9 | XWS_FACE_APP_ID | facebook app id for passwordless |
|10| XWS_FACE_APP_SECRET | facebook app secret for passwordless |
|11| DATABUS_PATH | base folder for databus used for sending large messages |
|12| USE_RMQ | if not empty the system will use rabbit mq as transport |
|13| RMQ_HOST | connection string for rabbitmq| 

### - Users.Graph microservice
The endpoint that is used for handling the connections between users and their interests and skills. It uses graph database.

|Num| Environment variable | Description |
|--| -------------------- | ----------- |
|1| NEO4J_URI | the uri of the graph database |
|2| NEO4J_USER | the username for the connection |
|3| NEO4J_PASS | the password for the connection |



### - Post microservice
The endpoint that is used for handling posts and comments. When it receives a request for retrieveing the posts it send the content of the response via NServiceBus.

### - Chat microservice
The endpoint that is used for handling messages and chats. It is possible to extend it for creating groups. When it receives a new message from the user microservice it stores it in the database and notifies all parties about the new message.

### - JobOffers microservice
The endpoint that is used for handling company accounts and job offers. It will be possible to authenticate a user through the user api, or with api token. It is used for third party applications to publish the job offers via the Dislinkt api.

## 2. SignalR
The library used for implementing talking via gRPC. When a user is logged in it should connect to user hub, chat hub and post hub which will be used for sending notifications and data from backend.

## 3. NServiceBus
The library used for implementing asynchronous messaging and sagas for dislinkt ecosystem.

## TODO [Docker image explanation...]
