1. Run the migrations to generate a `upload.db` file at the root of the project:

```
dotnet ef database update
```
or
```
Update-Database
```

in the nugget console of Visual Studio.

2. Run the API server and a swagger is accessible.

NOTES:
- I've worked on this for about 2 hours
- There are a lot of improvements that I want to do but I lack time:
  - Create services for CSV and XML respectively and inject them in the Dependency Injection
  - Add logging for error and what went wrong (it's in the requirements but I'm short on time)
  - I initially wanted to have a NextJS app in the same repo (usually I split them) for the upload and show page, but ran out of time
  - Better Response format, I usually use `{ code, data, message }` for responses as a general format so all API endpoint's results follow the same convention
