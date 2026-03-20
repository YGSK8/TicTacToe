public class Player
{
    public string name{get;set;}
    public readonly BoxVal assignedBoxval;
    private int _moves =0;
    public int MovesCount()=>_moves;
    public Player(string playername,BoxVal boxval)
    {
        name = playername;
        assignedBoxval = boxval;
    }
    public Position PlayerMove(int number,Func<int,Position>converter)
    {
        Position selection = converter(number);
        _moves++;
        return selection;
    }
}