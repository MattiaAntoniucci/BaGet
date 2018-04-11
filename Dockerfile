FROM microsoft/aspnetcore-build as build-env
WORKDIR /build/baget_

COPY . ./

RUN dotnet restore BaGet.sln

RUN dotnet publish -c Release -o out BaGet.sln

FROM microsoft/aspnetcore
WORKDIR /app
COPY --from=build-env /build/baget_/src/BaGet/out/ ./
EXPOSE 3894
ENTRYPOINT ["dotnet", "BaGet.dll"]