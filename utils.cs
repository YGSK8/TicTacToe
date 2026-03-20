using System.Diagnostics;

public class Utils
{
    public static int Validator(List<int> list)
    {
        foreach(int number in list)
        {
            Console.Write(number + " ");
        }
        Console.WriteLine(" ");
        int selection = -1;
        while (!list.Contains(selection))
        {
            selection = Convert.ToInt32(Console.ReadLine());
            if(!list.Contains(selection))Console.WriteLine("Invalid choice");
        }
        return selection;
    }

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
    public static string TextGen(string name)
    {
        return name+"'s turn, your options are: ";
    }
    
    public static void OnGameEnded()
    {
        Environment.Exit(0);
    }
}