FROM microsoft/dotnet:2.1-aspnetcore-runtime AS base
WORKDIR /app
EXPOSE 53777
EXPOSE 44318

FROM microsoft/dotnet:2.1-sdk AS build
WORKDIR /src
COPY ["HarbourhopPaymentSystem/HarbourhopPaymentSystem.csproj", "HarbourhopPaymentSystem/"]
RUN dotnet restore "HarbourhopPaymentSystem/HarbourhopPaymentSystem.csproj"
COPY . .
WORKDIR "/src/HarbourhopPaymentSystem"
RUN dotnet build "HarbourhopPaymentSystem.csproj" -c Release -o /app

FROM build AS publish
RUN dotnet publish "HarbourhopPaymentSystem.csproj" -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "HarbourhopPaymentSystem.dll"]