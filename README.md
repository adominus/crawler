# Description 
A simple web crawler accepting a URL as a parameter and outputting a simple sitemap. 

# Running 
Make sure you have [dotnet core 3.1 SDK](https://dotnet.microsoft.com/download/dotnet-core/3.1) installed. 

Then you can run the following command from the root of the project:

`dotnet run --project Monzo.Crawler/Monzo.Crawler.csproj --website "http://monzo.com"`

## Tests 
From the root of the project you can run the following: 

`dotnet test Monzo.Crawler.sln -v n`

