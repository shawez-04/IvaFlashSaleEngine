# -------------------------
# STAGE 1: BUILD
# -------------------------
# Changed from 10.0 to 8.0 (LTS)
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# FIX: Copy the .csproj based on your actual folder structure
# If your .csproj is in the root, use: COPY ["IvaFlashSaleEngine.csproj", "./"]
COPY ["IvaFlashSale/IvaFlashSaleEngine.csproj", "IvaFlashSale/"]
RUN dotnet restore "IvaFlashSale/IvaFlashSaleEngine.csproj"

# Copy everything else
COPY . .
WORKDIR "/src/IvaFlashSale"

# Build the application
RUN dotnet publish "IvaFlashSaleEngine.csproj" -c Release -o /app/publish /p:UseAppHost=false


# -------------------------
# STAGE 2: RUNTIME
# -------------------------
# Changed from 10.0 to 8.0 (LTS)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Copy published files from the build stage
COPY --from=build /app/publish .

# Render uses the PORT environment variable. 
# This line tells .NET to listen on the port Render provides.
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

# Ensure the DLL name matches your project output
ENTRYPOINT ["dotnet", "IvaFlashSaleEngine.dll"]