FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build

WORKDIR /src

COPY ["Api_Layer/Resturant_Ordering_System.Api_Layer.csproj", "Api_Layer/"]
COPY ["Business_Layer/Resturant_Ordering_System.Application.csproj", "Business_Layer/"]
COPY ["Data_Access_Layer/Resturant_Ordering_System.Infrastructre.csproj", "Data_Access_Layer/"]
COPY ["Domain_Layer/Resturant_Ordering_System.Domain.csproj", "Domain_Layer/"]

RUN dotnet restore "Api_Layer/Resturant_Ordering_System.Api_Layer.csproj"

COPY . .

RUN dotnet publish "Api_Layer/Resturant_Ordering_System.Api_Layer.csproj" \
    -c Release \
    -o /app/publish \
    --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final

WORKDIR /app

EXPOSE 8080

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "Resturant_Ordering_System.Api_Layer.dll"]