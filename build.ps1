Write-Host "Building backend..."
cd backend/VaultAuth.Api
dotnet restore
dotnet build -c Release

Write-Host "Running unit/integration tests..."
dotnet test

Write-Host "Applying EF Core migrations..."
dotnet ef database update

Write-Host "Building frontend..."
cd ../../frontend
npm install
npm run build

Write-Host "Starting Docker containers..."
cd ..
docker-compose up --build -d

Write-Host "Build, migrations, and startup complete!"
