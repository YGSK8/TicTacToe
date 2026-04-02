public class GameRecord
{
    public Guid GameRecordId{get;set;}
    public string Player1Name{get;set;}
    public string Player1BoxVal{get;set;}
    public string Player2Name{get;set;}
    public string Player2BoxVal{get;set;}
    public bool Draw{get;set;}
    public string BoardState{get;set;}
    public string CurrentPlayer{get;set;}
    public bool IsOver{get;set;}
    public string? Winner{get;set;}
    public GameRecord(Guid guid,string player1name,string player1boxval,string player2name,string player2boxval, bool draw,string [] board, BoxVal currentPlayer,bool isOver,Player winner)
    {
        GameRecordId = guid;
        Player1Name = player1name;
        Player1BoxVal = player1boxval;
        Player2Name = player2name;
        Player2BoxVal = player2boxval;
        Draw = draw;
        BoardState = string.Join(",",board);
        CurrentPlayer = currentPlayer.ToString();
        IsOver = isOver;
        Winner = winner?.assignedBoxval.ToString();
    }
    public GameRecord()
    {
        
    }
}