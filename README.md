# VaultAuth

VaultAuth is a secure identity management system designed for financial applications. It provides user registration, login, password reset, and account lockout features with strong validation rules and hashed password storage. Built with React, C#, PostgreSQL, and Docker, VaultAuth ensures safe authentication flows.



---

\## Tech Stack

\- \*\*Frontend:\*\* React

\- \*\*Backend:\*\* C# (.NET Web API)

\- \*\*Database:\*\* PostgreSQL

\- \*\*Containerization:\*\* Docker \& Docker Compose

\- \*\*Testing:\*\*  Unit tests, integration tests with TestServer

\- \*\*Authentication:\*\* JWT tokens



---



\## Prerequisites

\- \[Node.js](https://nodejs.org/) (for frontend)

\- \[.NET SDK](https://dotnet.microsoft.com/download) (for backend)

\- \[Docker Desktop](https://docs.docker.com/get-docker/) (for containerization)



---



\## Setup Instructions



\### 1. Clone the repository

```bash

git clone https://github.com/Jeliane101/VaultAuth.git

cd vaultauth



2\. Environment variables

Create a .env file in the backend project with:

JWT\_SECRET=your-secret-key-here

DB\_HOST=postgres

DB\_PORT=5432

DB\_NAME=vaultauth

DB\_USER=your-db-username

DB\_PASSWORD=your-db-password



3\. Run with Docker

docker-compose up --build



4\. Run locally (without Docker)

Backend:

cd backend

dotnet run



Frontend:

cd frontend

npm install

npm start



API Endpoints

POST /api/auth/register → Register a new user

POST /api/auth/login → Login and receive JWT

POST /api/auth/update-password→ Update Password(“Reset”)



PUT /api/auth/update → Update user profile (requires JWT)



GET /api/auth/profile → Get user details (requires JWT)



Testing

Unit Tests

Run backend unit tests:

cd backend

dotnet test



Integration Tests

Integration tests simulate API calls against an in‑memory server:

cd backend

dotnet test 



Notes

User emails must be unique (enforced at both backend and DB level).



Passwords are securely hashed before storage.



Profile images are stored in wwwroot/images.



Bonus Features

Dockerized multi‑container setup (API + DB).



JWT authentication with protected routes.



Integration tests for API endpoints.



Author

Developed by Jeliane for the Wonga Intermediate Developer Assessment.

&nbsp;





