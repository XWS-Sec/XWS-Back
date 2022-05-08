# XWS-Back
*University project*
## 1. Repository for backand logic of Dislinkt

The backand is consisted of :
* User microservice
* Post microservice
* Chat microservice
* Agent microservice

### - User microservice
Entry point to the system. Acts as integration part for external systems. Api knows all users and is required for authentication and authorization. All other requests are forwarded to corresponding endpoints based on the task at hand.

### - Post microservice
The endpoint that is used for handling posts and comments. When it receives a request for retrieveing the posts it send the content of the response via hub (SignalR)

### - Chat microservice
The endpoint that is used for handling messages and chats. It is possible to extend it for creating groups. When it receives a new message from the user microservice it stores it in the database and notifies all parties about the new message.

### - Agent microservice
The endpoint that is used for handling company accounts and job offers. It will be possible to authenticate a user through the user api, or with api token.

## 2. SignalR
The library used for implementing talking via gRPC. When a user is logged in it should connect to user hub, chat hub and post hub which will be used for sending notifications and data from backend.

## 3. NServiceBus
The library used for implementing asynchronous messaging and sagas for dislinkt ecosystem.

## TODO [Docker image explanation...]
