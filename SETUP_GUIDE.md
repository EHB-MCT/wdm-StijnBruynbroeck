# Weapon of Math Destruction - Setup Guide

## 🎯 Project Overview
This is a behavioral profiling and influence system built with Unity, Node.js/Express, PostgreSQL, and HTML/JavaScript. The system tracks player behavior, analyzes patterns, and applies psychological influence mechanisms to study decision-making processes.

## 📋 System Requirements

### Unity Requirements
- **Unity Version**: 2021.3.x LTS or later
- **Target Platform**: Windows, macOS, Linux (Standalone)
- **Required Unity Modules**:
  - Universal Render Pipeline (URP) - Pre-installed in project
  - Input System Package - Already configured
  - TextMeshPro - Already included

### Development Environment
- **Node.js**: Version 18.x or later
- **Docker**: Version 20.x or later with Docker Compose
- **Git**: For version control
- **Code Editor**: Visual Studio Code recommended (with Unity extension)

## 🚀 Quick Start Guide

### 1. Environment Setup
1. **Clone the repository**:
   ```bash
   git clone <repository-url>
   cd wdm-StijnBruynbroeck
   ```

2. **Copy environment configuration**:
   ```bash
   cp .env.template .env
   ```

3. **Edit .env file** with your preferred settings:
   ```env
   DB_USER=postgres
   DB_PASSWORD=your_secure_password
   DB_NAME=mathdestruction
   ```

### 2. Docker Deployment (Recommended)
1. **Start all services**:
   ```bash
   docker compose up --build
   ```

2. **Access services**:
   - **Backend API**: http://localhost:8080
   - **Admin Dashboard**: http://localhost:3001
   - **Database**: localhost:5432 (PostgreSQL)

3. **Start Unity and open the project**:
   - Open Unity Hub
   - Click "Open" and navigate to the `client` folder
   - Unity will detect the project and load it

### 3. Manual Development Setup

#### Backend Setup
```bash
cd backend
npm install
npm start
```

#### Database Setup
```bash
# Docker approach (recommended)
docker run -d --name postgres-dev \
  -e POSTGRES_USER=postgres \
  -e POSTGRES_PASSWORD=your_password \
  -e POSTGRES_DB=mathdestruction \
  -p 5432:5432 postgres:15-alpine
```

#### Admin Dashboard
```bash
cd admin-dashboard
python -m http.server 3000  # Or use any static file server
```

#### Unity Game
1. Open Unity 2021.3.x LTS
2. Open project from `client/` folder
3. Unity will automatically configure dependencies
4. Press Play to start the game

## 🎮 Game Controls & Features

### Basic Controls
- **Left Click**: Move to hex tile
- **Shift + Click**: Build village on adjacent tile
- **B Key**: Toggle build mode
- **ESC**: Cancel build mode

### Game Systems
1. **Resource Management**: Gold, Wood, Food, Stone, Population
2. **Building System**: Construct villages to generate passive income
3. **Threat System**: Respond to enemy threats (pay off or fight)
4. **Quest System**: Complete objectives for rewards and experience
5. **Progression System**: Level up to unlock benefits
6. **Behavioral Tracking**: All actions are logged for analysis

### Win Conditions
- **Build 5 villages** OR **Accumulate 100 gold**
- **Lose condition**: Run out of both gold and wood

## 🔧 Technical Architecture

### Data Flow
```
Unity Game (C#) → Backend API (Node.js) → PostgreSQL Database
         ↓                           ↓
    Admin Dashboard ← Analytics Service ← User Profiles
```

### Key Components
- **Unity Client**: Behavioral data collection with influence mechanisms
- **Express Backend**: RESTful API with real-time profiling
- **PostgreSQL**: User data, interactions, and analytics storage
- **Admin Dashboard**: Real-time behavioral analysis and visualization

## 🧠 Behavioral Profiling Features

### Tracked Metrics
- **Decision Timing**: Time taken for each action
- **Risk Tolerance**: Response patterns to threats
- **Resource Management**: Spending and saving patterns
- **Movement Analysis**: Spatial behavior and exploration
- **Emotional Response**: Reactions to wins/losses
- **Quest Decisions**: Strategic vs reactive choices

### Influence Mechanisms
- **Framing Effects**: Gain/loss framing based on player profile
- **Social Proof**: Display what other players choose
- **Scarcity Indicators**: Limited-time offers and urgency
- **Anchoring Bias**: Default options and suggestions
- **Dynamic Difficulty**: Adjusts based on player skill

## 📊 Admin Dashboard Features

### Real-time Analytics
- **User Selection**: Filter by specific player or view all
- **Behavioral Charts**: Decision patterns over time
- **Heat Maps**: Movement and action visualization
- **Profile Comparison**: Side-by-side user analysis
- **Influence Effectiveness**: Track which mechanisms work best

### Data Export
- **CSV Export**: Raw data for external analysis
- **JSON Export**: Complete user profiles
- **Session Reports**: Detailed timeline analysis

## 🔍 Troubleshooting

### Common Issues

#### Unity Issues
- **"All compiler errors have to be fixed"**: Check that all C# files compile
- **Missing references**: Ensure all GameObject references are set in Inspector
- **Input not working**: Check that Input System package is installed

#### Backend Issues
- **"Connection refused"**: Ensure Docker containers are running
- **Database errors**: Verify PostgreSQL is accessible and credentials are correct
- **Port conflicts**: Check if ports 5432, 8080, 3000 are available

#### Data Issues
- **No data in dashboard**: Verify Unity game is sending events to backend
- **Empty user profiles**: Check GameLogger.cs is properly initialized
- **Missing influence effects**: Ensure InfluenceManager is enabled

### Debug Commands
```bash
# Check Docker status
docker compose ps

# View backend logs
docker compose logs backend

# Reset database
docker compose down -v
docker compose up --build

# Test API directly
curl http://localhost:8080/api/health
```

## 📚 Code Structure & Conventions

### Microsoft C# Coding Standards
- **PascalCase** for public members and methods
- **camelCase** for private members
- **XML Documentation** for all public APIs
- **Consistent spacing** and access modifiers
- **Source attribution** for borrowed implementations

### Project Structure
```
wdm-StijnBruynbroeck/
├── client/                 # Unity game project
│   └── Assets/Scripts/     # C# game logic
├── backend/                # Node.js API server
│   ├── src/               # Source code
│   └── Dockerfile          # Container configuration
├── admin-dashboard/         # Web-based admin interface
├── docker-compose.yml       # Multi-service orchestration
├── .env.template          # Environment variables template
└── README.md              # This documentation
```

## 🔒 Security & Privacy

### Data Protection
- **No external API keys** required
- **Local development only** - no cloud services
- **Anonymized data** - uses GUIDs, not personal info
- **GDPR considerations** - clear data retention policies

### Ethical Implementation
- **Transparency**: All influence mechanisms are documented
- **User consent**: Game clearly states data collection
- **Data minimization**: Only necessary behavioral metrics stored
- **Research purposes**: System designed for academic study

## 🚀 Deployment Options

### Development (Default)
- Uses Docker Compose with local volumes
- Hot-reload enabled for rapid development
- Debug logging enabled throughout

### Production Considerations
- Use environment-specific configurations
- Implement proper authentication
- Set up reverse proxy (nginx)
- Configure SSL certificates
- Monitor system performance

## 📞 Support & Resources

### Documentation Files
- **SYSTEM_DOCUMENTATION.md**: Detailed technical architecture
- **ETHICAL_ANALYSIS.md**: Comprehensive ethical considerations
- **SOURCES.md**: All reference materials and attributions
- **TODO.md**: Development roadmap and status

### External Resources
- **Microsoft C# Coding Conventions**: [Link](https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions)
- **Unity Documentation**: [Link](https://docs.unity3d.com/)
- **Docker Documentation**: [Link](https://docs.docker.com/)
- **Node.js Best Practices**: [Link](https://nodejs.org/en/docs/guides/)

---

## 🎯 Success Indicators

Your system is working correctly when:
1. ✅ **Unity game starts** without compiler errors
2. ✅ **Docker containers** run without errors
3. ✅ **Backend API** responds on localhost:8080
4. ✅ **Admin dashboard** loads at localhost:3001
5. ✅ **Game actions** appear in dashboard
6. ✅ **Behavioral profiles** update in real-time
7. ✅ **Influence mechanisms** affect game text and options

---

**Last Updated**: January 3, 2026
**Version**: 1.0
**License**: MIT License - See LICENSE file for details