# FrontendAtlas

Frontend for BackendAtlas project, built with Astro, Tailwind CSS, React, and Nano Store.

## Getting Started

1. Install dependencies:
   ```bash
   npm install
   ```

2. Start the development server:
   ```bash
   npm run dev
   ```

3. Open [http://localhost:4321](http://localhost:4321) in your browser.

## Features

- Astro for static site generation
- Tailwind CSS for styling
- React for interactive components
- Nano Store for state management

## Login Credentials

To access the admin dashboard, use the following credentials for the SuperAdmin role:

- **Email**: admin@sistema.com
- **Password**: admin123

These credentials are seeded in the backend database for testing purposes.

## Important: Starting the Backend

Before using the frontend, make sure the backend is running:

```bash
cd ../BackendAtlas
dotnet run
```

The backend will run on `https://localhost:7029` by default. Make sure this URL is configured in your `.env` file:

```dotenv
PUBLIC_API_URL=https://localhost:7029
```

If you get SSL certificate errors, you may need to trust the self-signed certificate or adjust your `.env` to use `http://localhost:5000` if you've configured a different port.