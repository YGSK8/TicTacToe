public class Utils
{
    public static Position IntToPosition(int number)
    {
        return number switch
        {
            0=>new Position(0,0),
            1=>new Position(0,1),
            2=>new Position(0,2), 
            3=>new Position(1,0), 
            4=>new Position(1,1), 
            5=>new Position(1,2), 
            6=>new Position(2,0), 
            7=>new Position(2,1), 
            8=>new Position(2,2), 
        };
    }
}