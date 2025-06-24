# ChatAI Assistant 🤖

A real-time chat application with AI assistant support. Users can chat with multiple AI providers (OpenAI, Azure OpenAI, Claude) through a modern web interface.

## What is this project?

This is a **Proof of Concept (POC)** chat application that allows users to:
- Start chatting by simply entering a username
- Choose their preferred AI provider (OpenAI, Azure OpenAI, Claude, or Mock Bot)
- Have real-time conversations with AI assistants
- View conversation history
- Switch between different AI providers during chat

## Architecture

```
Frontend (Blazor WebAssembly + Radzen UI)
              ↕ HTTPS + SignalR
Backend (ASP.NET Core + SignalR Hub)
              ↕
Database (SQL Server LocalDB)
              ↕
AI Services (OpenAI/Azure/Claude APIs)
```

## Technologies Used

- **Frontend**: Blazor WebAssembly, Radzen Components
- **Backend**: ASP.NET Core 8.0, SignalR, Entity Framework
- **Database**: SQL Server LocalDB
- **AI Providers**: OpenAI GPT, Azure OpenAI, Anthropic Claude
- **Real-time**: SignalR WebSockets

## Quick Start

### Prerequisites
- .NET 8.0 SDK
- Visual Studio 2022 or VS Code
- SQL Server LocalDB (included with Visual Studio)

### 1. Clone & Setup
```bash
git clone <repository-url>
cd ChatAI_Assistant
```

### 2. Configure AI Keys (Optional)
Edit `src/ChatAI_Assistant.Server/appsettings.Development.json`:
```json
{
  "AI": {
    "Providers": {
      "OpenAI": {
        "ApiKey": "sk-your-openai-key-here",
        "Enabled": true
      },
      "AzureOpenAI": {
        "ApiKey": "your-azure-key",
        "Endpoint": "https://your-resource.openai.azure.com/",
        "Enabled": false
      }
    }
  }
}
```
> **Note**: You can skip this step and use the Mock Bot for testing

### 3. Run the Application

**Terminal 1 - Start Backend:**
```bash
cd src/ChatAI_Assistant.Server
dotnet run
```
Backend will run on `https://localhost:7000`

**Terminal 2 - Start Frontend:**
```bash
cd src/ChatAI_Assistant.Client
dotnet run
```
Frontend will run on `https://localhost:7001`

### 4. Use the App
1. Open `https://localhost:7001` in your browser
2. Enter any username (will be created automatically)
3. Choose your AI provider from the dropdown
4. Start chatting!

## Project Structure

```
ChatAI_Assistant/
├── src/
│   ├── ChatAI_Assistant.Server/     # Backend API + SignalR
│   │   ├── Controllers/             # REST API endpoints
│   │   ├── Services/                # Business logic
│   │   ├── Hubs/                   # SignalR real-time hub
│   │   └── Data/                   # Database entities
│   │
│   ├── ChatAI_Assistant.Client/    # Frontend Blazor app
│   │   ├── Components/             # UI components
│   │   ├── Services/               # API communication
│   │   └── Pages/                  # App pages
│   │
│   └── ChatAI_Assistant.Shared/    # Common DTOs and models
│
└── README.md
```

## Key Features

### Simple User Flow
1. **Login**: Just enter a username, no password needed
2. **Chat**: Automatic session creation and AI provider selection
3. **Real-time**: Instant messages via SignalR
4. **History**: Previous conversations saved automatically

### AI Provider Support
- **OpenAI**: GPT-3.5/GPT-4 models
- **Azure OpenAI**: Enterprise OpenAI service
- **Claude**: Anthropic's Claude AI
- **Mock Bot**: Simple demo bot (no API key needed)

### Technical Features
- **Clean Architecture** with Repository and Service patterns
- **Entity Framework** with SQL Server LocalDB
- **SignalR** for real-time communication
- **Radzen Components** for modern UI
- **Multi-provider AI** with factory pattern

## API Endpoints

### Quick Chat Flow
```http
POST /api/chat/quick-start
{
  "username": "john",
  "initialMessage": "Hello!",
  "preferredAIProvider": "OpenAI"
}
```

### Continue Chat
```http
POST /api/chat/continue  
{
  "sessionId": "session-guid",
  "userId": "user-guid", 
  "message": "How are you?",
  "preferredAIProvider": "OpenAI"
}
```

### Get Messages
```http
GET /api/chat/sessions/{sessionId}/messages?limit=50
```

## Configuration

### Database
The app uses SQL Server LocalDB by default. The database is created automatically on first run.

### AI Providers
- **OpenAI**: Get API key from https://platform.openai.com/
- **Azure OpenAI**: Set up Azure OpenAI service
- **Claude**: Get API key from https://console.anthropic.com/
- **Mock**: No configuration needed

### Environment Variables (Optional)
```bash
OPENAI_API_KEY=sk-your-key
AZURE_OPENAI_KEY=your-azure-key
AZURE_OPENAI_ENDPOINT=https://your-resource.openai.azure.com/
```

## Development Notes

### Adding New AI Provider
1. Create service implementing `IAIService`
2. Add to `AIServiceFactory`
3. Update configuration and UI selector

### Frontend Components
- Uses **Radzen Blazor** components for UI
- **SignalR client** for real-time updates
- **State management** for user session

### Backend Services
- **Repository pattern** for data access
- **Service layer** for business logic
- **SignalR hub** for real-time communication
- **Factory pattern** for AI providers

## Testing

### With Mock Bot (No API Keys)
```bash
# Backend
cd src/ChatAI_Assistant.Server
dotnet run

# Frontend  
cd src/ChatAI_Assistant.Client
dotnet run

# Open browser: https://localhost:7001
# Username: test
# AI Provider: Mock Bot
# Message: "Hello!"
```

### With Real AI
1. Configure API keys in `appsettings.Development.json`
2. Select OpenAI/Azure/Claude provider
3. Chat with real AI models

## Troubleshooting

### Common Issues
- **Database**: Run `dotnet ef database update` in Server project
- **CORS**: Check frontend URL matches backend CORS settings
- **SignalR**: Verify WebSocket connection in browser dev tools
- **AI API**: Check API keys and quotas

### Logs
Check console output for detailed error messages and API call logs.

## License

MIT License - Feel free to use and modify.

---

**Quick Demo**: Enter username → Choose AI → Start chatting! 🚀