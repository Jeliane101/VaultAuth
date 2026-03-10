#!/bin/bash
set -e

echo "Building backend..."
cd backend/VaultAuth.Api
dotnet restore
dotnet build -c Release

echo "Running unit/integration tests..."
dotnet test

echo "Applying EF Core migrations..."
dotnet ef database update

echo "Building frontend..."
cd ../../frontend
npm install
npm run build

echo "Starting Docker containers..."
cd ..
docker-compose up --build -d

echo "Build, migrations, and startup complete!"
