FROM mcr.microsoft.com/dotnet/runtime:6.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["AirdropAlgorandASA/AirdropAlgorandASA.csproj", "AirdropAlgorandASA/"]
RUN dotnet restore "AirdropAlgorandASA/AirdropAlgorandASA.csproj"
COPY . .
WORKDIR "/src/AirdropAlgorandASA"
RUN dotnet build "AirdropAlgorandASA.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "AirdropAlgorandASA.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "AirdropAlgorandASA.dll"]