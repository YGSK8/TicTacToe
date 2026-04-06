# ASP.NET Core — Study Guide: Building Your First Web API

Read this before you start writing code. Come back to any section when something isn't clicking.

---

## 1. What Is a Web API?

A Web API is a program that listens for HTTP requests and responds with data — usually JSON. It has no screen, no buttons, no interface. It just receives a request, does something, and sends back a response.

When a client (a browser, a mobile app, another server) calls your API, it sends an HTTP request with a method and a path:

```
POST /game/new
POST /game/move
GET  /game/{id}
```

Your API reads the request, runs some logic, and returns a JSON response. That is the entire job.

The key difference from a console app: your code does not run top-to-bottom once and exit. It starts up, then **waits**. Every incoming request triggers the relevant piece of your code, which runs and returns a response. The app keeps waiting for the next request until you shut it down.

---

## 2. How ASP.NET Core Starts Up

Every ASP.NET Core app follows the same three-phase startup in `Program.cs`:

**Phase 1 — Configure (builder)**

You create a builder and register everything the app will need: services, middleware, settings. Nothing is running yet. You are just giving instructions.

```csharp
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
```

**Phase 2 — Build (app)**

You hand the builder all its instructions and it produces a running app object.

```csharp
var app = builder.Build();
```

**Phase 3 — Pipeline and Run**

You tell the app how to handle requests (route them to controllers), then start the server.

```csharp
app.MapControllers();
app.Run();
```

> Think of it like building a car before driving it. The builder phase assembles all the parts. `Build()` locks it in. `Run()` turns the key.

---

## 3. Services and Dependency Injection

This is the concept that confuses most beginners. Take your time with it.

**The problem it solves:**

Your `GameController` needs access to a game store — a `Dictionary<Guid, Game>` that holds active games. You could create it inside the controller, but then every request would create a new, empty dictionary and lose all previous games. You need one dictionary shared across all requests.

**The solution — dependency injection (DI):**

You register objects with the framework in Phase 1, and the framework hands them to whoever needs them automatically.

**Step 1 — Register the service in `Program.cs`:**

```csharp
builder.Services.AddSingleton<Dictionary<Guid, Game>>();
```

This tells the framework: create one `Dictionary<Guid, Game>` and keep it alive for the entire lifetime of the app. Anyone who asks for it gets the same instance.

**Step 2 — Declare the dependency in your controller:**

```csharp
public GameController(Dictionary<Guid, Game> games)
{
    _games = games;
}
```

When ASP.NET Core creates your controller to handle a request, it sees the constructor parameter, looks it up in the registered services, and passes it in automatically. You never call `new GameController(...)` yourself.

**The three lifetimes:**

| Lifetime | Behaviour | Use case |
|---|---|---|
| `Singleton` | One instance for the entire app | Shared state (your game store) |
| `Scoped` | One instance per HTTP request | Database connections |
| `Transient` | New instance every time | Lightweight, stateless services |

For your game store, **Singleton** is correct. One dictionary, shared across all requests, alive as long as the server is running.

**Seeing the difference in practice — the IdGenerator test:**

Create a service that generates a short random ID on construction:

```csharp
public class IdGenerator
{
    public string Id { get; } = Guid.NewGuid().ToString()[..4];
}
```

Inject it twice into the same controller:

```csharp
public TestController(IdGenerator first, IdGenerator second)
{
    _first = first;
    _second = second;
}
```

Return both IDs from an endpoint and observe:

| Registration | FirstId | SecondId | After refresh |
|---|---|---|---|
| `AddTransient` | `A1B2` | `C3D4` | Different again |
| `AddScoped` | `E5F6` | `E5F6` | New matching pair |
| `AddSingleton` | `Z9X0` | `Z9X0` | Same forever |

- **Transient** — new instance every time the constructor asks, even within the same request. Two different IDs.
- **Scoped** — one instance for the entire HTTP request. Same ID for both. New pair on next request.
- **Singleton** — one instance for the entire app lifetime. Same ID no matter how many requests.

---

## 4. Controllers and Routing

A controller is a class that handles HTTP requests. Each public method in a controller is an endpoint — a specific URL the API responds to.

```csharp
[ApiController]
[Route("game")]
public class GameController : ControllerBase
{
    [HttpPost("new")]
    public IActionResult NewGame() { ... }

    [HttpPost("move")]
    public IActionResult MakeMove() { ... }

    [HttpGet("{id}")]
    public IActionResult GetGame(Guid id) { ... }
}
```

**Breaking down the attributes:**

- `[ApiController]` — marks this class as an API controller. Enables automatic model validation and JSON binding.
- `[Route("game")]` — sets the base path for all endpoints in this controller. Every method's route is relative to this.
- `[HttpPost("new")]` — this method handles `POST` requests to `/game/new`.
- `[HttpGet("{id}")]` — handles `GET` requests to `/game/{id}`. The `{id}` is a route parameter — ASP.NET Core reads it from the URL and passes it to your method automatically.

**`ControllerBase`** — the base class your controller inherits from. It provides helper methods you use to build responses:

```csharp
return Ok(gameState);         // 200 — success, with JSON body
return NotFound();            // 404 — resource not found
return BadRequest("message"); // 400 — client sent invalid input
```

**`IActionResult`** — the return type of every endpoint. It represents any HTTP response. You never return raw data directly — you always wrap it in one of these helpers.

---

## 5. What Happens When a Request Arrives

Trace the full lifecycle of a `POST /game/new` request:

1. The client sends: `POST /game/new` with a JSON body.
2. ASP.NET Core receives it and matches the path and method to `GameController.NewGame()`.
3. The framework creates a `GameController` instance, injecting the `Dictionary<Guid, Game>` from the service container into the constructor.
4. It deserializes the JSON body into your request object automatically (`[ApiController]` handles this).
5. `NewGame()` runs: creates a `Game`, stores it in the dictionary, calls `GetGameState()`, returns `Ok(gameState)`.
6. ASP.NET Core serializes `gameState` to JSON and sends it back as the HTTP response.
7. The controller instance is discarded. The dictionary (singleton) stays alive.

**The client receives:**

```json
{
  "board": ["none","none","none","none","none","none","none","none","none"],
  "currentPlayer": "Yan",
  "isOver": false,
  "winner": null
}
```

---

## 6. The Mental Model

```
Program.cs        — runs once at startup. Configures everything.
Service container — a bag of shared objects. Register things in; the framework pulls them out.
Controller        — receives a request, calls game logic, returns a response. No logic of its own.
Game logic        — unchanged from your console app. Knows nothing about HTTP.
```

The framework does the plumbing between all of these. Your job is to configure it correctly in `Program.cs` and write clean controller methods that delegate to your game logic.

---

## 7. Key Classes and Attributes Reference

### `ControllerBase`

The base class your controller inherits from. It gives you all the helper methods you need to return HTTP responses. You never instantiate it — you inherit from it.

It does **not** include view/HTML support (that's `Controller`, a subclass). For a pure JSON API, `ControllerBase` is always the right choice.

---

### `IActionResult`

The return type of every endpoint method. It represents an HTTP response — any response. You don't return raw data directly because the framework needs to control the status code, headers, and body together.

`ControllerBase` gives you helper methods that produce `IActionResult`:

| Method | HTTP Status | When to use |
|---|---|---|
| `Ok(data)` | 200 | Success — returns data as JSON |
| `NotFound()` | 404 | The requested resource doesn't exist |
| `BadRequest("msg")` | 400 | Client sent invalid input |
| `Created(...)` | 201 | A new resource was successfully created |
| `NoContent()` | 204 | Success — nothing to return |

---

### `[ApiController]`

An attribute applied to your controller class. It activates three behaviours automatically:

1. **Model binding** — reads JSON from the request body and maps it to your method parameters.
2. **Validation** — if a required field is missing, it returns a `400 Bad Request` automatically without you writing any validation logic.
3. **Route inference** — works with `[Route]` to build the full URL for each endpoint.

Without this attribute, you'd have to handle all three manually.

---

### `[Route("game")]`

Defines the base URL path for all endpoints in the controller. Every method's `[HttpGet]` or `[HttpPost]` path is appended to this.

```
[Route("game")]          → base path: /game
[HttpPost("new")]        → full path: POST /game/new
[HttpGet("{id}")]        → full path: GET  /game/{id}
```

---

### HTTP Methods — Full Reference

| Method | Attribute | Purpose | curl flag |
|---|---|---|---|
| GET | `[HttpGet]` | Read/retrieve a resource | *(default)* |
| POST | `[HttpPost]` | Create a new resource | `-X POST` |
| PUT | `[HttpPut]` | Replace an existing resource (full update) | `-X PUT` |
| PATCH | `[HttpPatch]` | Partially update a resource | `-X PATCH` |
| DELETE | `[HttpDelete]` | Delete a resource | `-X DELETE` |

**When to use each:**

```
GET    /games/{id}     → read a game
POST   /games          → create a new game
PUT    /games/{id}     → replace the entire game record
PATCH  /games/{id}     → update one or two fields (e.g. just the board state)
DELETE /games/{id}     → delete a game
```

**PUT vs PATCH:**
- `PUT` sends the full object — all fields replaced
- `PATCH` sends only the fields that changed — more efficient for partial updates
- For your TicTacToe `POST /game/move`, some would argue `PATCH /game/{id}` is more correct since you're partially updating game state. Both are valid in practice.

**In your controller:**

Attributes applied to individual methods. They do two things:
1. Specify which HTTP method triggers this endpoint.
2. Append a sub-path to the controller's base route.

```csharp
[HttpGet("{id}")]
public IActionResult GetGame(Guid id) { ... }

[HttpPut("{id}")]
public IActionResult ReplaceGame(Guid id, GameRequest request) { ... }

[HttpDelete("{id}")]
public IActionResult DeleteGame(Guid id) { ... }
```

The `{id}` in the route is a **route parameter** — ASP.NET Core reads it from the URL and passes it directly to your method parameter. If the URL is `/game/abc-123`, then `id` receives that value automatically.

---

### `Guid`

A globally unique identifier. A 128-bit value that looks like this: `3f2504e0-4f89-11d3-9a0c-0305e82c3301`.

`Guid.NewGuid()` generates a new random one. The chance of two being identical is astronomically small — which makes it safe to use as a unique key for games.

In your API: every new game gets a `Guid` as its ID. The client stores it and sends it with every subsequent request.

---

### Anonymous Objects — `new { }`

When you need to return multiple values from an endpoint without creating a dedicated class, you can use an anonymous object:

```csharp
return Ok(new { Id = id, State = game.GetGameState() });
```

ASP.NET Core serializes it to JSON automatically:

```json
{
  "id": "3f2504e0-4f89-11d3-9a0c-0305e82c3301",
  "state": {
    "board": ["none", "none", ...],
    "currentPlayer": "Player1",
    "isOver": false,
    "winner": null
  }
}
```

Use anonymous objects for one-off response shapes. If the same shape appears in multiple places, create a proper class instead.

---

## 8. Making HTTP Requests with curl

curl is the command-line tool for sending HTTP requests. Always run it in a **separate terminal** from `dotnet run`.

**GET request:**
```
curl http://localhost:5077/game/{id}
```

**POST request with no body:**
```
curl -X POST http://localhost:5077/game/new
```

**POST request with JSON body:**
```
curl -X POST http://localhost:5077/game/move -H "Content-Type: application/json" -d '{"gameId":"your-guid-here","playerChoice":0}'
```

**PUT request (full update):**
```
curl -X PUT http://localhost:5077/game/{id} -H "Content-Type: application/json" -d '{"field":"value"}'
```

**PATCH request (partial update):**
```
curl -X PATCH http://localhost:5077/game/{id} -H "Content-Type: application/json" -d '{"field":"value"}'
```

**DELETE request:**
```
curl -X DELETE http://localhost:5077/game/{id}
```

**Flags:**
| Flag | Meaning |
|---|---|
| `-X POST` | Set the HTTP method (POST, PUT, PATCH, DELETE) |
| `-H "Content-Type: application/json"` | Set a request header — required when sending a JSON body |
| `-d '...'` | Set the request body |
| `-v` | Verbose — shows status code, headers, and body |

**Rule:** Always put the entire curl command on one line. Backslash line-continuation (`\`) can cause issues in some terminals.

---

## 9. Entity Framework Core

### What It Is

EF Core is an ORM (Object-Relational Mapper). It translates C# objects into database rows and back. You write C# — it handles the SQL.

### Chain of Command

**Writing data (Add / Update / Remove):**
```
Controller
    ↓
DbContext.GameRecords.Add(record)    ← you stage the change
    ↓
Change Tracker                       ← EF Core watches the object for changes
    ↓
DbContext.SaveChanges()              ← you trigger the write
    ↓
SQL Generator                        ← EF Core builds INSERT / UPDATE / DELETE
    ↓
Database Provider (SQLite)           ← executes the SQL
    ↓
tictactoe.db                         ← row written to disk
```

**Reading data (Find / SingleOrDefault / Where):**
```
Controller
    ↓
DbContext.GameRecords.SingleOrDefault(x => x.Id == id)   ← you write LINQ
    ↓
SQL Generator                        ← EF Core translates LINQ to SELECT SQL
    ↓
Database Provider (SQLite)           ← executes the SQL
    ↓
tictactoe.db                         ← row read from disk
    ↓
Entity Materializer                  ← EF Core maps columns back to C# object
    ↓
Change Tracker                       ← EF Core starts watching the returned object
    ↓
Controller receives C# object
```

**Key rule:** You never write SQL. You write C# — EF Core handles the translation at every step.

```
Your Code (C# objects)
        ↓
   EF Core (DbContext)       ← the bridge
        ↓
   SQLite / PostgreSQL / SQL Server   ← swappable
```

### Architecture Diagram

```
TicTacToe.Api/
├── Program.cs              ← registers DbContext with DI
├── GameRecord.cs           ← Model: one class = one database table
├── GameDbContext.cs        ← DbContext: bridge between C# and database
├── Migrations/             ← auto-generated SQL schema files
│   ├── 20260330_InitialCreate.cs      ← Up() creates table, Down() drops it
│   ├── 20260330_InitialCreate.Designer.cs
│   └── GameDbContextModelSnapshot.cs ← current schema snapshot
└── tictactoe.db            ← the actual SQLite database file
```

### The Three EF Core Components

| Component | What it is | You write it? |
|---|---|---|
| `Model` | C# class that maps to a table | Yes |
| `DbContext` | Bridge between code and database | Yes |
| `Migration` | SQL to create/update the schema | EF Core generates it |

---

### Step-by-Step: Adding EF Core to a Project

**Step 1 — Install NuGet packages** (run from the API project folder):
```
dotnet add package Microsoft.EntityFrameworkCore.Sqlite --version 9.0.0
dotnet add package Microsoft.EntityFrameworkCore.Tools --version 9.0.0
```

**Step 2 — Install the global CLI tool** (once per machine):
```
dotnet tool install --global dotnet-ef --version 9.0.0
```
If you get a PATH warning, add the tools folder:
```
export PATH="$PATH:/Users/yanishgooradoo/.dotnet/tools"
cat << \EOF >> ~/.zprofile
export PATH="$PATH:/Users/yanishgooradoo/.dotnet/tools"
EOF
```

**Step 3 — Create the Model** (e.g. `GameRecord.cs`):
- Standalone class — do NOT inherit from DTOs or other classes
- Property named `Id` or `{ClassName}Id` → EF Core auto-detects as primary key
- All properties need `{get; set;}` — EF Core uses setters when reading from the database
- Add a **parameterless constructor** — EF Core uses it to create instances when reading rows
- Non-nullable reference types (`string`) will cause CS8618 warnings from the parameterless constructor — this is expected

**Step 4 — Create the DbContext** (e.g. `GameDbContext.cs`):
```csharp
using Microsoft.EntityFrameworkCore;

public class GameDbContext : DbContext
{
    public DbSet<GameRecord> GameRecords { get; set; }
    public GameDbContext(DbContextOptions<GameDbContext> options) : base(options) { }
}
```
- `DbSet<T>` → represents a table. Name it as the plural of your model (`GameRecords`).
- Constructor passes options to the base `DbContext` class.

**Step 5 — Register in `Program.cs`:**
```csharp
builder.Services.AddDbContext<GameDbContext>(options =>
    options.UseSqlite("Data Source=tictactoe.db"));
```
- Place this before `builder.Build()`
- `"Data Source=tictactoe.db"` is the connection string — file is created relative to where the app runs

**Step 6 — Create and apply the migration:**
```
dotnet ef migrations add InitialCreate
dotnet ef database update
```
- `migrations add` generates the migration files in `Migrations/`
- `database update` runs the SQL and creates the `.db` file

---

### Common DbContext Methods

| Method | What it does | When to use |
|---|---|---|
| `_context.GameRecords.Add(record)` | Stages a new record for INSERT | Creating a new entity |
| `_context.GameRecords.Update(record)` | Marks all properties as modified | Forcing an UPDATE |
| `_context.GameRecords.Remove(record)` | Stages a DELETE | Deleting an entity |
| `_context.GameRecords.Find(id)` | Fetches by primary key | Fast lookup by ID |
| `_context.GameRecords.SingleOrDefault(x => x.Id == id)` | LINQ query — returns one or null | Lookup with a condition |
| `_context.GameRecords.Where(x => x.IsOver == true).ToList()` | LINQ query — returns many | Filtering records |
| `_context.SaveChanges()` | Writes all staged changes to the database | Always call after Add/Update/Remove |

**Important:** `Add()`, `Update()`, and `Remove()` only stage changes in memory. Nothing hits the database until you call `SaveChanges()`.

**Change tracking:** When you fetch a record via `SingleOrDefault` or `Find`, EF Core tracks it. If you modify its properties and call `SaveChanges()`, EF Core detects the changes and generates an UPDATE automatically. If change tracking isn't working, call `_context.GameRecords.Update(record)` to force it.

---

### SQLite CLI Commands

```
sqlite3 tictactoe.db                          # open the database
sqlite3 tictactoe.db "SELECT * FROM Table;"   # run a one-off query
sqlite3 tictactoe.db "SELECT col1, col2 FROM Table WHERE Id = 'guid';"
.tables                                       # list all tables (inside sqlite3 shell)
.schema GameRecords                           # show table structure
.exit                                         # exit sqlite3 shell
```

**Note:** The `.db` file is created relative to where `dotnet run` is called. Inconsistent working directories = multiple `.db` files. Use `find . -name "*.db"` to locate them all.

---

## 10. Async / Await

### Threads vs CPU Cores

```
Cores    = cashiers (4 physical executors — fixed by hardware)
Threads  = checkout lanes (100s of concurrent work units — managed by OS)
Customers = the actual instructions waiting to be executed
```

The OS is the store manager — it decides which core serves which thread and for how long. Cores switch between threads extremely fast (context switching), giving the illusion that all threads run simultaneously.

**Threads do NOT belong to an application.** Each app has its own thread pool, but the OS scheduler shares CPU time across all apps running on the machine.

**Thread pool:** A fixed set of threads managed by .NET. Minimum = 1 per core. Maximum = hard cap (~32,767, never reached in practice). Adding threads is expensive — the pool reuses them instead of creating new ones.

### Why Async Matters

**Without async (synchronous):**
```
Request arrives → thread assigned → thread calls database → thread waits → thread idle
                                                              ↑
                                              doing nothing, blocking other requests
```

**With async:**
```
Request arrives → thread assigned → thread calls database → thread released to pool
                                                              ↑
                                              free to handle other requests
Database responds → thread from pool picks up → continues → returns response
```

The thread count stays the same. What changes is how much work each thread gets done between waits.

```
WITHOUT async: 100 threads = 100 max concurrent requests (each one blocked waiting)
WITH async:    100 threads = thousands of concurrent requests (threads free during I/O)
```

### When to Use Async

```
async = useful ONLY when waiting for something outside the CPU

In-memory operation  → nanoseconds  → thread never needs to be released → NO async
I/O operation        → milliseconds → thread should be released          → YES async
```

Examples in your controller:
```csharp
_games.Add(id, game)               // Dictionary write → RAM → NO async
_context.GameRecords.Add(record)   // EF Core staging → RAM only → NO async
await _context.SaveChangesAsync()  // SQL sent to SQLite on disk → YES async
await _context.FindAsync(id)       // SQL SELECT from database → YES async
```

`_context.GameRecords.Add()` is commonly misunderstood — it stages the record in EF Core's change tracker in memory only. No database contact until `SaveChanges`.

### Async Pattern in ASP.NET Core

Three things change every time:

```
SYNCHRONOUS                          ASYNC
────────────────────────────────────────────────────────
public IActionResult NewGame()       public async Task<IActionResult> NewGame()
{                                    {
    _context.SaveChanges();              await _context.SaveChangesAsync();
    return Ok(...);                      return Ok(...);
}                                    }
```

1. Add `async` to the method signature
2. Wrap the return type in `Task<>`
3. Every I/O call becomes `await someMethodAsync()`

The thread is released at the exact line where `await` appears. When the I/O completes, a thread from the pool picks up and continues from the next line.

### EF Core Async Methods

| Synchronous | Async |
|---|---|
| `SaveChanges()` | `SaveChangesAsync()` |
| `Find(id)` | `FindAsync(id)` |
| `SingleOrDefault(...)` | `SingleOrDefaultAsync(...)` |
| `ToList()` | `ToListAsync()` |

---

## 11. Frontend — HTML, TypeScript, and the Browser

### The Full Stack Picture

```
Browser
  ↓ opens
localhost:3000  (static file server — serves HTML, CSS, JS files as-is)
  ↓ loads
index.html → loads game.js (compiled from game.ts)
  ↓ user clicks button
game.js calls fetch("localhost:5077/game/new")
  ↓
localhost:5077  (backend server — Kestrel + ASP.NET Core)
  ↓
GameController.NewGame()
  ↓
SQLite database
  ↓
JSON response back to browser
  ↓
game.js updates the HTML
```

The browser coordinates between both servers. The two servers never talk to each other.

### Web Server vs Backend Server vs Static File Server

```
Static file server   → serves HTML/CSS/JS files with no logic (localhost:3000)
Web server           → anything that serves HTTP responses (both servers qualify)
Backend server       → web server + application logic + database (localhost:5077)
```

Every backend server uses a web server underneath. Not every web server is a backend server.

### Kestrel and ASP.NET Core — Abstraction Layers

"Sits on top" refers to abstraction level, not request order.

```
Request flow (who sees it first):     Abstraction hierarchy:

Network         ← first               Your Code (GameController)  ← highest abstraction
Kestrel         ← second              ASP.NET Core (routing, DI, JSON)
ASP.NET Core    ← third               Kestrel (HTTP, TCP, bytes)
Your Code       ← last                Network                      ← lowest level
```

- **Kestrel** — handles raw bytes, TCP connections, HTTP protocol. Converts network data into `HttpContext`. No concept of controllers or routing.
- **ASP.NET Core** — sits above Kestrel. Takes `HttpContext` and applies routing, middleware, dependency injection, JSON serialization.
- **Your controllers** — sit above ASP.NET Core. Only deal with game logic. No knowledge of HTTP.

### Two Terminals Required

```
Terminal 1: dotnet run          → API on localhost:5077
Terminal 2: npx http-server .   → Frontend on localhost:3000
```

The browser opens localhost:3000 and calls the API at localhost:5077.

### HTML Reference

```html
<!DOCTYPE html>
<html lang="en">
  <head>
    <meta charset="UTF-8">
    <title>Page Title</title>       ← shown on browser tab
    <style> ... </style>            ← CSS goes here
  </head>
  <body>
    <h1>Heading</h1>                ← largest heading
    <p id="status">Text</p>        ← paragraph (semantic text content)
    <button id="btn">Click</button> ← clickable button
    <div id="board">                ← generic container (no semantic meaning)
      <button class="cell" data-index="0"></button>
    </div>
    <script type="module" src="game.js"></script>  ← load JS last, inside body
  </body>
</html>
```

**`<p>` vs `<div>`:**
- `<p>` = paragraph — use for text content. Semantic meaning.
- `<div>` = generic box — use for grouping elements. No semantic meaning.

**Attributes:**
- `id` — unique identifier, used by TypeScript to find the element (`getElementById`)
- `class` — shared identifier, used to style or select multiple elements
- `data-index` — custom data attribute, readable by TypeScript via `getAttribute("data-index")`

### CSS Grid (3×3 Board)

```css
#board {
    display: grid;
    grid-template-columns: repeat(3, 60px);  /* 3 columns, each 60px wide */
}
.cell {
    width: 60px;
    height: 60px;
}
```

`display: grid` turns a container into a grid. `repeat(3, 60px)` creates 3 equal columns. Child elements flow into the grid automatically left-to-right, top-to-bottom.

### TypeScript Basics

**Variable declarations:**
```typescript
const x = value;          // cannot be reassigned (use for DOM elements, fixed values)
let x: string | null = null;  // can be reassigned (use for game state)
```

No type before the name like C#. Keyword (`const`/`let`) comes first. Type annotation uses `:` after the name.

**Type annotation syntax:**
```typescript
// C#:          string   name = "Yan";
// TypeScript:  const    name: string = "Yan";
```

**Arrow functions (lambdas):**
```typescript
() => { }                  // no parameters
(x) => { }                 // one parameter
async () => { }            // async arrow function
```

**Non-null assertion (`!`):**
```typescript
const el = document.getElementById("btn")!;
```
`getElementById` returns `HTMLElement | null`. The `!` tells TypeScript "trust me, this won't be null." Use when you know the element exists in your HTML.

### DOM Manipulation

```typescript
document.getElementById("id")!          // find element by id
document.querySelectorAll(".class")      // find all elements with class (never null)
element.textContent = "text"            // set visible text inside an element
element.getAttribute("data-index")      // read a data-* attribute (returns string | null)
element.addEventListener("click", fn)   // listen for a click event
```

### fetch API

Making HTTP requests from TypeScript — equivalent of calling an API with curl, but in code.

**GET request:**
```typescript
const response = await fetch("http://localhost:5077/game/" + id);
const data = await response.json();
```

**POST request with JSON body:**
```typescript
const response = await fetch("http://localhost:5077/game/move", {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify({ gameId: id, playerChoice: 3 })
});
const data = await response.json();
```

`fetch` returns a Promise (TypeScript's equivalent of `Task<T>`). Always `await` it.
`response.json()` parses the response body — also a Promise, also `await` it.
`JSON.stringify()` converts a JS object to a JSON string for the request body.

### CORS — Cross-Origin Resource Sharing

The browser blocks requests to a different origin by default. Your frontend on localhost:3000 calling your API on localhost:5077 is a cross-origin request.

Configure in `Program.cs` (Phase 1 + Phase 3):

```csharp
// Phase 1 — register the policy
builder.Services.AddCors(options =>
    options.AddPolicy("AllowFrontend", policy =>
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));

// Phase 3 — apply it (before MapControllers)
app.UseCors("AllowFrontend");
```

`AllowAnyOrigin` is fine for local development. In production, lock it to specific domains.

**Policy name must match exactly** — the string in `AddPolicy` and `UseCors` are case-sensitive.

### TypeScript Compilation

Browsers cannot run TypeScript. `tsc` compiles `.ts` → `.js`.

```
game.ts   →   tsc   →   game.js
                          ↑
                    browser runs this
```

**tsconfig.json key setting:**
```json
"module": "ES2020"
```
Set this to target browsers. Default (`commonjs`) generates Node.js-style code with `exports` that browsers cannot read.

**Script tag must use `type="module"`** when using ES2020:
```html
<script type="module" src="game.js"></script>
```

**Workflow:**
1. Edit `game.ts`
2. Run `tsc` to recompile
3. Refresh the browser

### Debugging Frontend — Browser DevTools

`F12` opens the browser developer tools. The **Console** tab shows errors and `console.log()` output — equivalent of your terminal for frontend code.

When the UI doesn't update, always check the Console first. Common errors:
- `exports is not defined` → wrong `module` setting in tsconfig (set to ES2020)
- `Failed to fetch` → API server not running, or CORS not configured
- `file://` security error → open via HTTP server, not directly from the filesystem