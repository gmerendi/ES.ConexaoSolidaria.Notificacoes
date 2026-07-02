# ES.ConexaoSolidaria.Notificacoes

Serviço **Worker de Notificações** da plataforma **Conexão Solidária**, desenvolvido para a ONG Esperança Solidária como parte do Hackathon POSTECH/FIAP.<br><br>
Esse serviço é utilizado apenas no deploy Local.  Quando realizado deploy no Cloud AWS, utilizamos os serviços serverless (SQS / Lambda / SNS).  Mais detalhes podem ser verificados no repositorio de infraestrutura — [https://github.com/gmerendi/ES.ConexaoSolidaria.Infra].

Responsável por:
- Consumir eventos de domínio publicados pelos demais microsserviços da plataforma;
- Enviar e-mails transacionais correspondentes a cada evento (via SMTP);
- Rodar como um **Worker Service** puro (sem API HTTP), monitorado via arquivo de heartbeat para o Kubernetes.

---

## Sumário
- [Arquitetura](#arquitetura)
- [Stack Tecnológica](#stack-tecnológica)
- [Eventos Consumidos](#eventos-consumidos)
- [Como Rodar Localmente](#como-rodar-localmente)
- [Variáveis de Ambiente](#variáveis-de-ambiente)
- [Observabilidade](#observabilidade)
- [Testes](#testes)
- [Estrutura do Projeto](#estrutura-do-projeto)
- [Github Actions](#github-actions)

---

## Arquitetura

Este serviço é um consumidor de eventos assíncrono: ele não expõe nenhuma API HTTP. Escuta filas do broker de mensageria e, para cada evento relevante, dispara um e-mail transacional para o usuário correspondente (via SMTP, usando um simulador **Mailpit** em ambiente local).

> O diagrama completo da arquitetura da plataforma está no repositório de infraestrutura — [https://github.com/gmerendi/ES.ConexaoSolidaria.Infra]

## Stack Tecnológica

- **.NET 8** (Worker Service / `BackgroundService`, sem Web Host)
- **MassTransit** + **RabbitMQ** para consumo de eventos
- **MailKit** / **MimeKit** para envio de e-mails via SMTP
- **Mailpit** como servidor SMTP de desenvolvimento (captura os e-mails localmente, sem enviar de verdade)
- **Docker** / **Docker Compose**

## Eventos Consumidos

| Evento | Consumer | Ação | Status |
|---|---|---|---|
| `UserCreatedEvent` | `UserCreatedEventConsumer` | Envia e-mail de boas-vindas ao novo usuário | ✅ Ativo |
| `DonationProcessedEvent` | `DonationProcessedEventConsumer` | Envia e-mail de confirmação de doação processada | ✅ Ativo |
| `CampaignCreatedEvent` | `CampaignCreatedEventConsumer` | Notificar usuários sobre nova campanha | ⚠️ Parcial — consumer existe, mas o envio de e-mail está comentado e ele **não está registrado** no `AddConsumer` do MassTransit; nenhum serviço publica esse evento hoje |

## Como Rodar Localmente

### Pré-requisitos
- [.NET 8 SDK](https://dotnet.microsoft.com/download)
- Docker Desktop 4.79.0
- Infraestrutura de mensageria já em execução (RabbitMQ do `ES.ConexaoSolidaria.Campanhas` ou `ES.ConexaoSolidaria.Usuarios`)

### Subindo o serviço

```bash
git clone https://github.com/gmerendi/ES.ConexaoSolidaria.Notificacoes.git
cd ES.ConexaoSolidaria.Notificacoes

docker compose up -d --build
```

O `docker-compose.yml` sobe o Worker de Notificações e o **Mailpit** (servidor SMTP de teste):

| Serviço | URL/Porta |
|---|---|
| Mailpit — SMTP | localhost:1025 |
| Mailpit — Interface Web (visualizar e-mails enviados) | http://localhost:8025 |

## Variáveis de Ambiente

| Variável | Descrição |
|---|---|
| `RabbitMq__Host` / `RabbitMq__Username` / `RabbitMq__Password` | Conexão com o RabbitMQ |
| `QUEUES__USER_CREATED_QUEUE` | Fila do evento `UserCreatedEvent` |
| `QUEUES__DONATION_PROCESSED_QUEUE` | Fila do evento `DonationProcessedEvent` |
| `QUEUES__CAMPAIGN_CREATED_QUEUE` | Fila do evento `CampaignCreatedEvent` (lida na configuração, mas ainda sem endpoint/consumer ativo) |
| `Application__Type` | `LOCAL` ou `AWS` — define se a conexão ao RabbitMQ usa hostname simples ou URI completa |
| `Email__SmtpHost` | Host do servidor SMTP (`mailpit` por padrão) |
| `Email__SmtpPort` | Porta do servidor SMTP (`1025` por padrão) |
| `Email__NomeRemetente` / `Email__EmailRemetente` | Nome e e-mail exibidos como remetente |

## Observabilidade

- Este serviço **não expõe métricas Prometheus nem endpoint HTTP de health check**.
- A saúde do processo é sinalizada por um heartbeat: a cada 10 segundos, o `Worker` escreve a data/hora atual no arquivo `/tmp/healthy`, pensado para ser lido por uma probe do Kubernetes baseada em `exec` (verificando a idade do arquivo).
- Logs estruturados são enviados via `IBaseLogger`, com auditoria opcional (DynamoDB).

## Testes

Este repositório não possui testes por servir apenas para teste de envio de e-mails localmente.  No AWS, será feito com SNS.

## Estrutura do Projeto

```
ES.ConexaoSolidaria.Notificacoes/
├── src/
│   └── Notificacoes/
│       ├── Consumers/           # UserCreatedEventConsumer, DonationProcessedEventConsumer
│       ├── Domain/               # Eventos de Domínio (contratos compartilhados) e Shared
│       ├── Infrastructure/
│       │   ├── Email/            # SmtpEmailService (MailKit/MimeKit)
│       │   ├── Extensions/       # Messaging, AuditLog, Logging, Email
│       │   └── Logging/
│       ├── Worker.cs              # BackgroundService (heartbeat de health check)
│       └── Program.cs             # Host de console (sem Web Host)
├── docker-compose.yml             # Worker + Mailpit
└── ES.ConexaoSolidaria.Notificacoes.slnx
```

Projeto desenvolvido para o Hackathon **POSTECH** — grupo 1.

## Github Actions

Esse repositorio não possui Github Action por ser utilizado somente em desenvolvimento local para simulação de envio de e-mails.





