FROM registry.access.redhat.com/dotnet/dotnet-22-runtime-rhel7
WORKDIR /app
COPY publish ./

#ENTRYPOINT ["dotnet", "./SwishClientTester.dll"]