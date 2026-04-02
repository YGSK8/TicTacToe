using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[ApiController]
[Route("game")]
public class GameController:ControllerBase{
    private readonly GameDbContext _context;
    public GameController(GameDbContext context)
    {
        _context = context;
    }
    [HttpPost("new")]
    public async Task<IActionResult> NewGame(){
        Guid id = Guid.NewGuid();
        Board board = new Board();
        Player player1 = new Player("player1",BoxVal.X);
        Player player2 = new Player("player2",BoxVal.O);
        Game game = new Game(player1,player2,board);
        GameRecord record= new GameRecord(id,player1.Name,player1.assignedBoxval.ToString(),player2.Name,player2.assignedBoxval.ToString(),game.Draw(),board.BoardState(),game.CurrentPlayer().assignedBoxval,game.GameOver(),game.Winner());
        _context.GameRecords.Add(record);
        await _context.SaveChangesAsync();
        return Ok(new{Id = id, State = game.GetGameState()});
    }
    [HttpGet("{id}")]
    public async Task<IActionResult> GetGame(Guid id)
    {
        GameRecord? record = await _context.GameRecords.FindAsync(id);
        if(record == null)return NotFound();
        GameState gameState = new GameState(record.BoardState.Split(','),record.CurrentPlayer,record.IsOver,record.Winner);
        return Ok(gameState);
    }

    [HttpPost("move")]
    public async Task<IActionResult> Move(MoveRequest request)
    {
        GameRecord? record = await _context.GameRecords.FindAsync(request.GameId);
        if(record == null)return NotFound();
        Board board = new Board(record.BoardState.Split(','));
        Player player1 = new Player(record.Player1Name,Utils.StringToBoxVal(record.Player1BoxVal));
        Player player2 = new Player(record.Player2Name,Utils.StringToBoxVal(record.Player2BoxVal));
        Player currentplayer = Utils.StringToBoxVal(record.CurrentPlayer)==player1.assignedBoxval?player1:player2;
        bool draw = record.Draw;
        bool gameOver = record.IsOver;
        Player? winner;
        if(record.Winner == null)winner = null; else winner = Utils.StringToBoxVal(record.Winner)==player1.assignedBoxval?player1:player2;
        Game game = new Game(player1,player2,board,currentplayer,draw,gameOver,winner);
        if(game.GameOver()||!game.AvailableSquares().Contains(request.PlayerChoice))return BadRequest();
        game.MakeMove(request.PlayerChoice);
        record.Draw = game.Draw();
        record.Winner = game.Winner()?.assignedBoxval.ToString();
        record.CurrentPlayer = game.CurrentPlayer().assignedBoxval.ToString();
        record.IsOver = game.GameOver();
        record.BoardState = string.Join(",",game.BoardState());
        _context.GameRecords.Update(record);
        await _context.SaveChangesAsync();
        return Ok(game.GetGameState());
    }
}


