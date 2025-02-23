# Message Service API

## Этот проект представляет собой сервис для обмена сообщениями через REST API и WebSocket.

## 1. Запуск проекта

### Требования:

Docker
Docker Compose
.NET SDK 9
PostgreSQL

### Клонирование репозитория:
```
git clone https://github.com/your-repo/message-service.git
cd message-service
```

### Запуск с Docker Compose:
```
docker-compose up --build -d
```

API будет доступно на http://localhost:8080
Swagger: http://localhost:8080/swagger
WebSocket: ws://localhost:8080/ws

### Остановка сервиса:
```
docker-compose down
```

## 2. API Endpoints

### Получить все сообщения:
```
GET /api/v1/messages/all
```

### Получить сообщение по ID:
```
GET /api/v1/messages/{id}
```

### Отправить сообщение:
```
POST /api/v1/messages
Content-Type: application/json
{
  "text": "Привет, WebSocket!",
  "messageId": 12312
}
```

### Сообщения за временной период
```
GET /api/v1/messages/history?from=yyyy-mm-ddT00:00:00.000Z&to=yyyy-mm-ddT00:00:00.000Z
```

## 3. WebSocket API

### Подключение: ws://localhost:8080/ws

### Пример кода WebSocket-клиента:
```
const ws = new WebSocket("ws://localhost:8080/ws");
ws.onmessage = (event) => console.log("📩", event.data);
```

## 4. Дополнительно

### Swagger-документация: http://localhost:8080/swagger

### WebSocket сервер: ws://localhost:8080/ws

### Теперь сервис готов к работе! 🚀
