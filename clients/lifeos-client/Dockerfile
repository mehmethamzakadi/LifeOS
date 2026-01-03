# ============================================
# LifeOS Client - Multi-Stage Dockerfile
# ============================================
# Stage 1: Build React application
# ============================================
FROM node:20-alpine AS build

WORKDIR /app

# Copy package files
COPY package.json package-lock.json* ./

# Update npm to latest version to avoid version warnings
RUN npm install -g npm@latest

# Install dependencies (including devDependencies for build)
RUN npm ci || npm install

# Copy source code
COPY . .

# Build argument for API URL (build-time environment variable)
ARG VITE_API_URL=http://localhost:6060
ENV VITE_API_URL=$VITE_API_URL

# Build the application
RUN npm run build:prod

# ============================================
# Stage 2: Production with Nginx
# ============================================
FROM nginx:alpine

# Install curl for healthcheck
RUN apk add --no-cache curl

# Copy custom nginx configuration
COPY nginx.conf /etc/nginx/conf.d/default.conf

# Copy built application from build stage
COPY --from=build /app/dist /usr/share/nginx/html

# Add healthcheck
HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
  CMD curl -f http://localhost/ || exit 1

# Expose port 80
EXPOSE 80

# Start nginx
CMD ["nginx", "-g", "daemon off;"]

