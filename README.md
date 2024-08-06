# RealTime-Display-LED
Real-time Time Display LED System at Bus Stations

## Install project

To install project, we need follow this steps:  
1. Open a terminal
2. Clone project in your directories as you want with command
```shell
git clone
```
3. Launch the project with command :
```shell
docker compose up --build -d
```
4. Config Node-red with documentation [node-red](./doc/node-red.md) in directory `doc`

## Test project

To test project we need follow this steps:
1. Do request GET in this adress `http://localhost:8000/api/Positionning/0`
2. You should have response `5 mn`


## Config

For launch this project, you need :
- docker with minimal version `19.03`
- docker-compose with minimal version `1.29.2`  

To develop this project, you need:
- .Net Core runtime version `3.1`
- Development tools .Net Framework version `4.7.2`