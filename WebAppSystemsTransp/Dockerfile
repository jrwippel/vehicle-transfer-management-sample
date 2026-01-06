# ... outras instruções ...

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

# Adicione a instalação de fontes aqui
RUN apt-get update && apt-get install -y fontconfig fonts-dejavu && rm -rf /var/lib/apt/lists/*


FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ./WebAppSystemsTransp.csproj ./
RUN dotnet restore "WebAppSystemsTransp.csproj"
COPY . .
WORKDIR "/src/WebAppSystemsTransp"
RUN dotnet build "../WebAppSystemsTransp.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "../WebAppSystemsTransp.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "WebAppSystemsTransp.dll"]

LABEL maintainer="Jackson Ricardo Wippel <jrwippel@hotmail.com>"
LABEL version="1.0"