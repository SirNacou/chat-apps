import { defineConfig } from '@hey-api/openapi-ts'

export default defineConfig({
  input: 'http://localhost:8080/openapi/v1.json',
  output: 'src/client',
  plugins: [
    '@hey-api/typescript',
    { name: '@hey-api/sdk', validator: true },
    { name: '@hey-api/client-ky', runtimeConfigPath: "./src/hey-api" },
    'zod',
    '@tanstack/react-query',
  ],
  logs: {
    path: './logs'
  }
})
