public class Board
{
    private Square[,] _board = new Square[3,3];
    public Square[,] GetBoard()=>_board;
    private List<Position>[] _diagonals = [new List<Position>{},new List<Position>{}];
    private List<Position>[] _rows = new List<Position>[3];
    private List<Position>[] _cols = new List<Position>[3];
    public List<Position>[] GetDiagonals()=> _diagonals;
    public List<Position>[] GetRows()=> _rows;
    public List<Position>[] GetCols()=> _cols;
    public event Action BoardUpdated;
    public Board()
    {
        int reference = 0;
        for(int row = 0; row < _board.GetLength(0); row++)
        {
            _rows[row]= new List<Position>{};
            _cols[row]= new List<Position>{};
            for(int col = 0; col < _board.GetLength(1); col++)
            {
                _board[row,col]=new Square(new Position(row,col),reference,BoxVal.none);
                _rows[row].Add(new Position(row,col));
                _cols[row].Add(new Position(col,row));
                reference ++;
            }
        }
        int y = _board.GetLength(1)-1;
        for(int x = 0; x < _board.GetLength(0); x++)
        {
            _diagonals[0].Add(new Position(x,x));
            _diagonals[1].Add(new Position(y,x));
            y--;
        }
    }
    public void UpdateBoard(Position position, BoxVal val)
    {
        _board[position.row,position.col].Value = val;
        BoardUpdated.Invoke();   
    }
    public List<List<Position>> MatchingThree()
    {
        List<List<Position>> matches = new List<List<Position>>{};
        foreach(List<Position> list in _rows)
        {
            if(list.All(pos=>_board[pos.row,pos.col].Value==_board[list[0].row,list[0].col].Value) && _board[list[0].row,list[0].col].Value !=BoxVal.none)matches.Add(list);
        }
        foreach(List<Position> list in _cols)
        {
            if(list.All(pos=>_board[pos.row,pos.col].Value==_board[list[0].row,list[0].col].Value) && _board[list[0].row,list[0].col].Value !=BoxVal.none)matches.Add(list);
        }
        foreach(List<Position> list in _diagonals)
        {
            if(list.All(pos=>_board[pos.row,pos.col].Value==_board[list[0].row,list[0].col].Value) && _board[list[0].row,list[0].col].Value !=BoxVal.none)matches.Add(list);
        }
        return matches;
    }
    public void DisplayGrid()
    {
        Console.WriteLine(" ---+---+---");
        foreach(List<Position> row in _rows)
        {
            // foreach(Position pos in row)
            // {
            //     Console.Write("| "+(_board[pos.row,pos.col].Value != BoxVal.none?_board[pos.row,pos.col].Value:_board[pos.row,pos.col].Reference)+" ");
            // }
            foreach(Position pos in row)
            {
                if(_board[pos.row,pos.col].Value==BoxVal.none) Console.Write("| "+_board[pos.row,pos.col].Reference+" ");
                else if(_board[pos.row,pos.col].Value==BoxVal.O){
                    Console.Write("| ");
                    Console.ForegroundColor=ConsoleColor.DarkRed;
                    Console.Write(_board[pos.row,pos.col].Value+" ");
                    Console.ResetColor();
                    }
                else if(_board[pos.row,pos.col].Value==BoxVal.X){
                    Console.Write("| ");
                    Console.ForegroundColor=ConsoleColor.Yellow;
                    Console.Write(_board[pos.row,pos.col].Value+" ");
                    Console.ResetColor();
                    }
            }
            Console.Write("|\n");
            // Console.Write("--");
        Console.WriteLine(" ---+---+---");
        }
    }
    public List<int> AvailableSquares()
    {
        List<int> references=new List<int>{};
        foreach(Square square in _board)
        {
            if(square.Value==BoxVal.none)references.Add(square.Reference);
        }
        return references;
    }

}

public enum BoxVal{none,O,X}