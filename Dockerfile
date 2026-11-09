FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY wt-lab7-7postgres-efcore-denisenko.csproj ./
RUN dotnet restore ./wt-lab7-7postgres-efcore-denisenko.csproj
COPY . .
RUN dotnet publish ./wt-lab7-7postgres-efcore-denisenko.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_ENVIRONMENT=Production
EXPOSE 10000
ENTRYPOINT ["dotnet", "wt-lab7-7postgres-efcore-denisenko.dll"]
