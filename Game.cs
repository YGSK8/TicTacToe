public class Game
{
    public Player Player1{get;}
    public Player Player2{get;}
    private int _moves;
    private bool _draw;
    private bool _gameover;
    private Player _winner;
    public bool Draw()=>_draw;
    public Player Winner()=>_winner;
    public bool GameOver()=>_gameover;
    public Board Board{get;}
    public Game(Player player1, Player player2,Board board)
    {
        Player1 = player1;
        Player2 = player2;
        Board = board;
        board.BoardUpdated+=board.DisplayGrid;
        board.BoardUpdated+=CheckStatus;
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
        if(_moves==9){_draw=true;_gameover=true;}
    }

    public void PlayerMove(Player player,int playerchoice)
    {
        _moves++;
        Board.UpdateBoard(player.PlayerMove(playerchoice,Utils.IntToPosition),player.assignedBoxval);
    }

}