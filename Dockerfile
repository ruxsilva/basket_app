FROM mcr.microsoft.com/dotnet/sdk:9.0 AS restore
WORKDIR /src

# Copy only project files first for better layer caching
COPY ["BasketProject/BasketProject.csproj", "BasketProject/"]
RUN dotnet restore "BasketProject/BasketProject.csproj"

# Build Node.js app in parallel to .NET build
FROM node:20-alpine AS angular-build
WORKDIR /src
COPY ./BasketProject/Web/ClientApp/package*.json ./
RUN npm ci --quiet
COPY ./BasketProject/Web/ClientApp .
# Disable Google Fonts inlining during build
RUN sed -i 's/inlineFonts: true/inlineFonts: false/g' angular.json || echo "No fonts configuration found"
# Use production build without --prod flag (deprecated)
RUN npm run build

# Build .NET app
FROM restore AS publish
COPY . .
WORKDIR "/src/BasketProject"
RUN dotnet restore "BasketProject.csproj" && dotnet publish "BasketProject.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Final image
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app

# Set environment variables
ENV ASPNETCORE_URLS=http://+:8081
ENV DOTNET_EnableDiagnostics=0

# Copy only what's needed
COPY --from=publish /app/publish .
COPY --from=angular-build /src/dist/client-app/browser /app/Web/ClientApp/dist/client-app/browser

EXPOSE 8081
ENTRYPOINT ["dotnet", "BasketProject.dll"]