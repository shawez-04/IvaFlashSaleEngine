# -------------------------
# STAGE 1: BUILD
# -------------------------
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# FIX: Match your specific folder and file name
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
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

# Copy published files
COPY --from=build /app/publish .

# Required for Render dynamic port binding
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

# FIX: Ensure the DLL name matches your project name
ENTRYPOINT ["dotnet", "IvaFlashSaleEngine.dll"]
