public class Game
{
    public Player Player1{get;}
    public Player Player2{get;}
    private Player _currentPlayer;
    private int _moves;
    private bool _draw;
    private bool _gameover;
    private Player? _winner;
    public Player CurrentPlayer()=>_currentPlayer;
    public bool Draw()=>_draw;
    public Player? Winner()=>_winner;
    public bool GameOver()=>_gameover;
    public Board Board{get;}
    public Game(Player player1, Player player2,Board board)
    {
        Player1 = player1;
        Player2 = player2;
        Board = board;
        _currentPlayer = player1;
        board.BoardUpdated+=CheckStatus;
    }
    public Game(Player player1, Player player2, Board board, Player currentplayer, bool draw, bool gameOver, Player? winner) : this(player1, player2, board)
    {
        _currentPlayer = currentplayer;
        _draw = draw;
        _gameover = gameOver;
        _winner = winner;
        _moves = 9-AvailableSquares().Count();
    }
    public void CheckStatus()
    {
        List<List<Position>> list = Board.MatchingThree();
        if(list.Count != 0)
        {
            if(Player1.assignedBoxval == Board.GetBoard()[list[0][0].row, list[0][0].col].Value)_winner = Player1;
            else _winner = Player2;
            _gameover=true;
        }
        if(_moves==9 && Winner()==null){_draw=true;_gameover=true;}
    }

    public void PlayerMove(Player player,int playerchoice)
    {
        _moves++;
        Board.UpdateBoard(player.PlayerMove(playerchoice,Utils.IntToPosition),player.assignedBoxval);
        _currentPlayer = _currentPlayer==Player1?Player2:Player1;
    }
    public List<int> AvailableSquares()
    {
        return Board.AvailableSquares();
    } 
    public GameState GetGameState()
    {
        return new GameState(Board.BoardState(),_currentPlayer.assignedBoxval.ToString(),_gameover,_winner?.Name);
    }
    public string[] BoardState()
    {
        return Board.BoardState();
    }
    public void MakeMove(int playerchoice)
    {
        if (AvailableSquares().Contains(playerchoice))
        {
            PlayerMove(_currentPlayer,playerchoice);
        }
        else throw new ArgumentOutOfRangeException();
    }
}
