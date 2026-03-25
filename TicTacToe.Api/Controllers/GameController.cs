using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("game")]
public class GameController:ControllerBase{

    private readonly Dictionary<Guid,Game> _games;
    public GameController(Dictionary<Guid,Game> games)
    {
        _games = games;
    }
    [HttpPost("new")]
    public IActionResult NewGame(){
        Guid id = Guid.NewGuid();
        Board board = new Board();
        Player player1 = new Player("player1",BoxVal.X);
        Player player2 = new Player("player2",BoxVal.O);
        Game game = new Game(player1,player2,board);
        _games.Add(id,game);
        return Ok(new{Id = id, State = game.GetGameState()});
    }
    [HttpGet("{id}")]
    public IActionResult GetGame(Guid id)
    {
        if(!_games.ContainsKey(id))return NotFound();
        Game game = _games[id];
        return Ok(game.GetGameState());
    }
    [HttpPost("move")]
    public IActionResult Move(MoveRequest request)
    {
        if(!_games.ContainsKey(request.GameId))return NotFound();
        Game game = _games[request.GameId];
        game.MakeMove(request.PlayerChoice);
        return Ok(game.GetGameState());
    } 
}


