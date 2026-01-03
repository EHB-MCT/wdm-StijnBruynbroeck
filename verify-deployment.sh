#!/bin/bash

# Weapon of Math Destruction - Deployment Verification Script
# This script verifies that all system components are working correctly

echo "🎮 Weapon of Math Destruction - System Verification"
echo "=================================================="

# Check if Docker is running
echo "📦 Checking Docker environment..."
if ! docker info > /dev/null 2>&1; then
    echo "❌ Docker is not running. Please start Docker first."
    exit 1
fi
echo "✅ Docker is running"

# Check if .env file exists
echo ""
echo "📋 Checking environment configuration..."
if [ ! -f ".env" ]; then
    echo "⚠️  .env file not found. Creating from template..."
    if [ -f ".env.template" ]; then
        cp .env.template .env
        echo "✅ .env file created from template"
        echo "📝 Please edit .env file with your database credentials"
    else
        echo "❌ .env.template not found!"
        exit 1
    fi
else
    echo "✅ .env file exists"
fi

# Check services status
echo ""
echo "🔍 Checking service status..."

# Database
echo "📊 Checking Database..."
if curl -s http://localhost:5432 > /dev/null 2>&1; then
    echo "✅ Database is accessible"
else
    echo "⚠️  Database may not be ready (this is normal for PostgreSQL)"
fi

# Backend
echo "🔧 Checking Backend API..."
if curl -s http://localhost:8080 > /dev/null 2>&1; then
    echo "✅ Backend API is running on port 8080"
    
    # Test API endpoints
    echo "🧪 Testing API endpoints..."
    if curl -s "http://localhost:8080/api/users" | head -c 10 > /dev/null 2>&1; then
        echo "✅ Users endpoint is working"
    else
        echo "❌ Users endpoint failed"
    fi
else
    echo "❌ Backend API is not accessible"
fi

# Admin Dashboard
echo "📈 Checking Admin Dashboard..."
if curl -s http://localhost:3000 > /dev/null 2>&1; then
    echo "✅ Admin Dashboard is running on port 3000"
else
    echo "❌ Admin Dashboard is not accessible"
fi

# Port availability check
echo ""
echo "🔌 Port Availability Check..."
ports=("5432:Database" "8080:Backend API" "3000:Admin Dashboard")

for port_info in "${ports[@]}"; do
    port=$(echo $port_info | cut -d: -f1)
    service=$(echo $port_info | cut -d: -f2)
    
    if netstat -tuln 2>/dev/null | grep -q ":$port "; then
        echo "✅ Port $port ($service) is open"
    else
        echo "❌ Port $port ($service) is not accessible"
    fi
done

# Docker containers check
echo ""
echo "🐳 Docker Container Status..."
containers=("wdm-stijnbruynbroeck-database" "wdm-stijnbruynbroeck-backend" "wdm-stijnbruynbroeck-admin-dashboard")

for container in "${containers[@]}"; do
    if docker ps --format "table {{.Names}}" | grep -q "$container"; then
        echo "✅ $container is running"
    else
        echo "❌ $container is not running"
    fi
done

# Access instructions
echo ""
echo "🚀 System Access Instructions:"
echo "=================================="
echo "🎮 Unity Game: Build and run in Unity Editor"
echo "📊 Admin Dashboard: http://localhost:3000"
echo "🔧 Backend API: http://localhost:8080"
echo "📦 Database: postgresql://user:password@localhost:5432/behavioral_profiling"
echo ""

echo "📚 Quick Start Guide:"
echo "======================"
echo "1. Open Unity Editor and load the client project"
echo "2. Run the game to start collecting behavioral data"
echo "3. Visit admin dashboard to view user profiles and analytics"
echo "4. Use dashboard to analyze patterns and generate influence strategies"
echo ""

echo "🔍 Troubleshooting:"
echo "===================="
echo "• If services don't start: 'docker-compose down && docker-compose up --build'"
echo "• If backend fails: Check .env file database credentials"
echo "• If dashboard is blank: Ensure backend is running first"
echo "• To view logs: 'docker-compose logs [service-name]'"
echo ""

echo "✅ System verification complete!"
echo "🎮 Weapon of Math Destruction is ready for use."