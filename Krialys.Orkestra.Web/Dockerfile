#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

#Depending on the operating system of the host machines(s) that will build or run the containers, the image specified in the FROM statement may need to be changed.
#For more information, please see https://aka.ms/containercompat

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["Krialys.Orkestra.WebApi/Krialys.Orkestra.WebApi.csproj", "Krialys.Orkestra.WebApi/"]
RUN dotnet restore "Krialys.Orkestra.WebApi/Krialys.Orkestra.WebApi.csproj"
COPY . .
WORKDIR "/src/Krialys.Orkestra.WebApi"
RUN dotnet build "Krialys.Orkestra.WebApi.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Krialys.Orkestra.WebApi.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Krialys.Orkestra.WebApi.dll"]



#FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
#WORKDIR /app
#EXPOSE 80
#EXPOSE 443
#
#FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
#WORKDIR /src
#
#COPY ["src/Host/Host.csproj", "src/Host/"]
#COPY ["src/Shared/Shared.csproj", "src/Shared/"]
#COPY ["src/Client/Client.csproj", "src/Client/"]
#COPY ["src/Client.Infrastructure/Client.Infrastructure.csproj", "src/Client.Infrastructure/"]
#
#RUN dotnet restore "src/Host/Host.csproj"
#
#COPY . .
#WORKDIR "/src/src/Host"
#
#RUN dotnet publish "Host.csproj" -c Release --no-restore -o /app/publish
#
#FROM base AS final
#WORKDIR /app
#
#COPY --from=build /app/publish .
#
#ENTRYPOINT ["dotnet", "FSH.BlazorWebAssembly.Host.dll"]