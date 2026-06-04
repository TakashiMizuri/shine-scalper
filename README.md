# Shine Scalper

Бот для автоматизированной торговли на **Bybit** (USDT-M futures): пробои уровней, paper-режим, веб-дашборд.

**База данных:** SQLite (файл `shine-scalper.db` создаётся при первом запуске backend).

## Документация

- [Спецификация v0.1](docs/SPEC-v0.1.md)
- [Changelog v0.1](changelog/v0.1.md)

## Требования

- [.NET 8 SDK](https://dotnet.microsoft.com/download)
- [Node.js 20+](https://nodejs.org/) (для дашборда)

## Запуск

```bash
# Backend
cd src/ShineScalper.Host
dotnet run

# Dashboard (в другом терминале)
cd web/dashboard
npm install
npm run dev
```

- API: http://localhost:5280  
- SignalR: http://localhost:5280/hubs/trading  
- Dashboard: http://localhost:5173 (Vite проксирует `/api` и `/hubs` на backend)

При первом запуске backend создаёт SQLite-базу с paper equity **10 000 USDT**.

## Тесты

```bash
dotnet test ShineScalper.sln
```

## Структура solution

```
src/
  ShineScalper.Core/          # домен, интерфейсы
  ShineScalper.Infrastructure/ # SQLite, EF Core
  ShineScalper.Bybit/          # REST + WebSocket market data
  ShineScalper.Engine/         # уровни, сигналы, paper trading
  ShineScalper.Api/            # REST + SignalR
  ShineScalper.Host/           # точка входа
web/dashboard/                 # React + lightweight-charts
changelog/                     # история версий
tests/ShineScalper.Engine.Tests/
```

## Режимы

| Режим | v0.1 |
|-------|------|
| Paper | Полностью работает |
| Live | Заглушка (`NotSupportedException`) |

## Конфигурация

Основные параметры: `src/ShineScalper.Host/appsettings.json`  

Строка подключения SQLite (по умолчанию):

```json
"ConnectionStrings": {
  "TradingDb": "Data Source=shine-scalper.db"
}
```

Секреты Bybit (для будущего live): User Secrets / переменные `Bybit__ApiKey`, `Bybit__ApiSecret`.
