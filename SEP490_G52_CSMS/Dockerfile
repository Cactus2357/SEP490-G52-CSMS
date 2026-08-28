# ---- Build stage ----
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj first for layer caching
COPY ["SEP490_G52_CSMS.csproj", "./"]
RUN dotnet restore "SEP490_G52_CSMS.csproj"

# Copy remaining source code and publish
COPY . .
RUN dotnet publish "SEP490_G52_CSMS.csproj" -c Release -o /app/publish /p:UseAppHost=false

# ---- Runtime stage ----
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
EXPOSE 9000
EXPOSE 8080

COPY --from=build /app/publish .

ENV TZ=Asia/Ho_Chi_Minh
ENV ASPNETCORE_URLS=https://+:9000;http://+:8080
ENV ASPNETCORE_HTTPS_PORTS=9000
ENV ASPNETCORE_ENVIRONMENT=Docker

ENTRYPOINT ["dotnet", "SEP490_G52_CSMS.dll"]
