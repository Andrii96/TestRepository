FROM microsoft/dotnet:2.1-sdk AS build-env
WORKDIR /app

# Copy csproj and restore as distinct layers
COPY . ./
RUN dotnet restore

# Copy everything else and build

RUN dotnet publish -c Release -o out

Run ls

# Build runtime image
WORKDIR /app
Run ls
COPY ./Devabit.Telelingua.ReportingServices/appsettings.json .
# COPY --from=build-env ./Devabit.Telelingua.ReportingServices/out/ .
ENTRYPOINT ["dotnet", "./Devabit.Telelingua.ReportingServices/out/Devabit.Telelingua.ReportingServices.Web.dll"]