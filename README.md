# Mercya ASP.NET Core sample

A minimal ASP.NET Core app that meets Mercya's ASP.NET Core buildpack contract:

1. **One `.csproj` at the project root** (or at the project's configured Root Directory).
2. **References `Amazon.Lambda.AspNetCoreServer.Hosting`.**
3. **Calls `AddAWSLambdaHosting(LambdaEventSource.HttpApi)`** in `Program.cs`.

The build fails fast with a clear message if 1 or 2 are missing, rather than
deploying a Lambda with no working handler.

## Routes

| Route | Purpose |
| --- | --- |
| `/` | HTML page, so the deployment screenshot has something to capture |
| `/health` | Echoes `MERCYA_DOMAIN`, proving the platform's env wiring reached the app |
| `/weatherforecast` | The stock template endpoint |

## Running locally

```bash
dotnet run
```

`AddAWSLambdaHosting` no-ops when `AWS_LAMBDA_FUNCTION_NAME` isn't set, so this
still runs on Kestrel locally and on Lambda when deployed - no branching.
