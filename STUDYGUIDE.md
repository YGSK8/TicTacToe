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

### `[HttpGet]` / `[HttpPost]`

Attributes applied to individual methods. They do two things:
1. Specify which HTTP method (GET, POST, etc.) triggers this endpoint.
2. Append a sub-path to the controller's base route.

```csharp
[HttpGet("{id}")]
public IActionResult GetGame(Guid id) { ... }
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