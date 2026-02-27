# -------------------------
# STAGE 1: BUILD
# -------------------------
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy csproj first for better Docker layer caching
COPY ["IvaFlashSale/IvaFlashSale.csproj", "IvaFlashSale/"]
RUN dotnet restore "IvaFlashSale/IvaFlashSale.csproj"

# Copy everything else
COPY . .
WORKDIR "/src/IvaFlashSale"

# Build the application
RUN dotnet publish "IvaFlashSale.csproj" -c Release -o /app/publish /p:UseAppHost=false


# -------------------------
# STAGE 2: RUNTIME
# -------------------------
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

# Copy published files
COPY --from=build /app/publish .

# Required for Render dynamic port binding
ENV ASPNETCORE_URLS=http://+:8080

# Expose port (Render will override internally)
EXPOSE 8080

# Start the application
ENTRYPOINT ["dotnet", "IvaFlashSaleEngine.dll"]