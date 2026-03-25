public class Square
{
    public Position Position{get;}
    public int Reference{get;}
    public BoxVal Value{get;set;}
    public Square(Position position, int reference, BoxVal val)
    {
        Position = position;
        Reference = reference;
        Value = val;
    }
}
public record Position(int row, int col);