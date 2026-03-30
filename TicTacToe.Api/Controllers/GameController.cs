using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("game")]
public class GameController:ControllerBase{

    private readonly Dictionary<Guid,Game> _games;
    private readonly GameDbContext _context;
    public GameController(Dictionary<Guid,Game> games,GameDbContext context)
    {
        _games = games;
        _context = context;
    }
    [HttpPost("new")]
    public IActionResult NewGame(){
        Guid id = Guid.NewGuid();
        Board board = new Board();
        Player player1 = new Player("player1",BoxVal.X);
        Player player2 = new Player("player2",BoxVal.O);
        Game game = new Game(player1,player2,board);
        GameRecord record= new GameRecord(id,player1.Name,player1.assignedBoxval.ToString(),player2.Name,player2.assignedBoxval.ToString(),game.Draw(),board.BoardState(),game.CurrentPlayer().assignedBoxval,game.GameOver(),game.Winner());
        _games.Add(id,game);
        _context.GameRecords.Add(record);
        _context.SaveChanges();
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
        if(game.GameOver()||!game.AvailableSquares().Contains(request.PlayerChoice))return BadRequest();
        game.MakeMove(request.PlayerChoice);
        GameRecord record = _context.GameRecords.SingleOrDefault(x => x.GameRecordId == request.GameId);
        record.Draw = game.Draw();
        record.Winner = game.Winner()?.assignedBoxval.ToString();
        record.CurrentPlayer = game.CurrentPlayer().assignedBoxval.ToString();
        record.IsOver = game.GameOver();
        record.BoardState = string.Join(",",game.BoardState());
        _context.GameRecords.Update(record);
        _context.SaveChanges();
        return Ok(game.GetGameState());
    } 
}


