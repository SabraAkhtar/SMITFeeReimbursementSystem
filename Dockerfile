FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["SMITFeeReimbursementSystem/SMITFeeReimbursementSystem.csproj", "SMITFeeReimbursementSystem/"]
RUN dotnet restore "SMITFeeReimbursementSystem/SMITFeeReimbursementSystem.csproj"

COPY . .
WORKDIR "/src/SMITFeeReimbursementSystem"
RUN dotnet publish "SMITFeeReimbursementSystem.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

RUN mkdir -p /app/data
RUN mkdir -p /app/wwwroot/uploads/payments
RUN mkdir -p /app/wwwroot/uploads/receipts

COPY --from=build /app/publish .

ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:$PORT

EXPOSE 8080

ENTRYPOINT ["dotnet", "SMITFeeReimbursementSystem.dll"]
