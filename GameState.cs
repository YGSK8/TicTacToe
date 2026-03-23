public class GameState
{
    public string[] Board{get;}
    public string CurrentPlayer{get;}
    public bool IsOver{get;}
    public string? Winner{get;}

    public GameState(string [] board, string currentPlayer,bool isOver,string winner)
    {
        Board = board;
        CurrentPlayer = currentPlayer;
        IsOver = isOver;
        Winner = winner;
    }

}