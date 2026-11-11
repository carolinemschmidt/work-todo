# Work Todo

A todo app that works out what to do next, so you don't have to remember which tasks are blocked by other tasks or by something you're waiting for.

Live: https://carolinemschmidt.github.io/work-todo/ (a demo that runs in your browser with sample data; to keep your own data, run the app locally as described below)

## What it does

Lets you create todos with a title, description, due date and priority, tag them, and attach links to tickets. A todo can depend on other todos, or on anything you describe in free text, such as "waiting for approval", which you tick off once it is resolved. The **Next up** page lists the open todos that aren't waiting on anything, most urgent first.

## Getting started

### Requirements

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- SQL Server Express (free from Microsoft) with a default `SQLEXPRESS` instance, using Windows authentication

No other external services are needed. The app has been tested on Windows only.

### Installation

```bash
git clone https://github.com/carolinemschmidt/work-todo.git
cd work-todo
dotnet restore WorkTodo.slnx
```

### Configuration

With a default SQL Express instance there is nothing to configure: the Development settings in `src/WorkTodo.Web/appsettings.Development.json` already point at `.\SQLEXPRESS` and create a database called `WorkTodo` on first run.

To change anything, set an environment variable before starting the app. The settings are ordinary .NET configuration, so there is no `.env` file and no `.env.example`; this table lists everything that can be set.

| Environment variable | Default | What it does |
| --- | --- | --- |
| `ConnectionStrings__WorkTodo` | `Server=.\SQLEXPRESS;Database=WorkTodo;Trusted_Connection=True;TrustServerCertificate=True` (Development) | Which SQL Server and database to use. Required when running a published build. |
| `Database__ApplyMigrationsOnStartup` | `true` in Development, otherwise `false` | Creates the database and applies migrations when the app starts. |
| `ASPNETCORE_URLS` | `http://localhost:5157` when started with `dotnet run` | The address the app listens on. |

For example, in PowerShell:

```powershell
$env:ConnectionStrings__WorkTodo = "Server=.\MYINSTANCE;Database=MyTodos;Trusted_Connection=True;TrustServerCertificate=True"
```

### Run the project

```bash
dotnet run --project src/WorkTodo.Web
```

The app is served at http://localhost:5157. On first run it creates the `WorkTodo` database and applies the migrations. If SQL Server can't be reached, the app prints why and exits instead of starting; check that SQL Server Express is running and that the connection string is right.

### More ways to run it

**A published build, without the source or an IDE.** A published build runs in the Production environment, so it doesn't read `appsettings.Development.json`. Give it the connection string, tell it to create the database, and pick an address:

```powershell
dotnet publish src/WorkTodo.Web --configuration Release --output publish

$env:ConnectionStrings__WorkTodo = "Server=.\SQLEXPRESS;Database=WorkTodo;Trusted_Connection=True;TrustServerCertificate=True"
$env:Database__ApplyMigrationsOnStartup = "true"
$env:ASPNETCORE_URLS = "http://localhost:5157"
dotnet publish\WorkTodo.Web.dll
```

**Create or update the database by hand.** If you would rather not create the database on startup, run the migrations yourself. `dotnet ef` runs in the Development environment, so it uses the connection string above or your `ConnectionStrings__WorkTodo` override.

```bash
dotnet tool restore
dotnet ef database update --project src/WorkTodo.Data --startup-project src/WorkTodo.Web
```

**The browser demo, locally.**

```bash
dotnet run --project src/WorkTodo.Demo
```

It is served at http://localhost:5158.

**The tests.**

```bash
dotnet test WorkTodo.slnx
```

## Architecture

The solution is split into layers that only depend on the ones below them. `Core` holds the entities, validation and dependency rules. `Data` holds the database context, migrations and the repositories. `Services` holds the application logic, such as what counts as "next up". `UI` holds the pages and components, with no business logic of its own. The same UI library runs in two hosts: a Blazor Server app that uses SQL Server, and a Blazor WebAssembly demo that swaps in in-memory repositories. Contract tests run against both kinds of repository, so they behave the same.

```
work-todo/
├── src/
│   ├── WorkTodo.Core/            # Entities, validation, dependency rules
│   ├── WorkTodo.Data/            # EF Core context, migrations, repository interfaces and SQL Server repositories
│   ├── WorkTodo.Data.InMemory/   # In-memory repositories, used by the demo
│   ├── WorkTodo.Services/        # Application logic: todos, dependencies, tags, next up
│   ├── WorkTodo.UI/              # Pages and components shared by both hosts
│   ├── WorkTodo.Web/             # Blazor Server host (SQL Server)
│   └── WorkTodo.Demo/            # Blazor WebAssembly host (in-memory, seeded with sample data)
├── tests/                        # Tests for Core, Data (including in-memory), Services, UI and Demo
├── .github/                      # CI/CD workflow and the script that prepares the demo for GitHub Pages
├── CLAUDE.md                     # Project rules for Claude Code
├── implementation.md             # Notes on how the agent handled the implementation
├── dotnet-tools.json             # Local tools (dotnet-ef)
└── WorkTodo.slnx                 # The solution
```

## Tech stack

| Component | Choice | Reason |
| --- | --- | --- |
| Language and runtime | C# on .NET 10 | The stack the project was specified with. The same domain, service and UI code runs in both the server app and the browser demo. |
| UI | Blazor (Server and WebAssembly) | One component library with two hosts: Server for the real app, WebAssembly so the demo can run on static hosting. |
| Database | SQL Server Express | Free, runs on your own machine, and is the database the project was specified with. |
| Data access | Entity Framework Core | Migrations create and update the schema. Repositories hide it from the rest of the code. |
| Test databases | SQLite in memory, plus hand-written in-memory repositories | Fast tests with no server. How they differ from SQL Server is listed under Limitations. |
| Component tests | xUnit and bUnit | Tests Blazor components without a browser. |
| CI/CD | GitHub Actions and GitHub Pages | Free for a public repository. Every push is built and tested, and a push to `main` publishes the demo. |

## Limitations

- **One user, no login.** There are no accounts or permissions.
- **Tested on Windows with SQL Server Express only.** The default connection string uses Windows authentication. Other platforms and SQL Server editions haven't been tried.
- **Some things can't be edited.** A ticket link can't be changed (remove it and add it again), and a tag can't be renamed (delete it and create it again).
- **Filtering and sorting happen in memory.** That is fine for a personal list, not for thousands of todos.
- **No conflict handling.** If the same todo is edited in two browser tabs, the last save wins.
- **The tests don't run against SQL Server.** They use SQLite and the in-memory repositories, which behave differently, for example in how tag names are compared and how ids are numbered. The migrations and real writes were checked by hand against SQL Server Express. CI has no SQL Server job.
- **The demo doesn't keep your data.** Changes are lost when the page is reloaded. The first visit downloads about 6.5 MB (compressed), partly because the demo carries SQL Server code it never uses.
- **Only the demo is deployed.** The Blazor Server app with a real database isn't hosted anywhere; you run it yourself.

## Developed with

Claude Code (agent-driven development) as part of the course Next-Generation Software Development with AI.
