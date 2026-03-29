public class GameTests
{
    [Fact]
    public void SquareCount_NewBoard_9()
    {
        Board board = new Board();
        int count = board.AvailableSquares().Count;
        Assert.Equal(9,count);
    }
    [Fact]
    public void AvailableSquares_AfterMove_SquareRemoved()
    {
        Board board = new Board();
        Player player1 = new Player("player1",BoxVal.X);
        Player player2 = new Player("player2",BoxVal.O);
        Game game = new Game(player1,player2,board);
        int playerChoice = 1;
        game.MakeMove(playerChoice);
        Assert.DoesNotContain(playerChoice,game.AvailableSquares());
    }
    [Fact]
    public void Win_ThreeInARow_PlayerAndGameOver()
    {
        Board board = new Board();
        Player player1 = new Player("player1",BoxVal.X);
        Player player2 = new Player("player2",BoxVal.O);
        Game game = new Game(player1,player2,board);
        List<int> moves = [0,3,1,4,2];
        foreach(int move in moves){
            game.MakeMove(move);
        }
        Assert.Equal(player1,game.Winner());
        Assert.True(game.GameOver());
    }
    [Fact]
    public void GameOver_Draw_GameOverAndDrawTrue()
    {
        Board board = new Board();
        Player player1 = new Player("player1",BoxVal.X);
        Player player2 = new Player("player2",BoxVal.O);
        Game game = new Game(player1,player2,board);
        List<int> moves = [0,1,2,4,3,5,7,6,8];
        foreach(int move in moves){
            game.MakeMove(move);
        }
        Assert.True(game.GameOver());
        Assert.True(game.Draw());
    }
    [Fact]
    public void Win_LastMove_DrawFalseAndPlayerWin()
    {
        Board board = new Board();
        Player player1 = new Player("player1",BoxVal.X);
        Player player2 = new Player("player2",BoxVal.O);
        Game game = new Game(player1,player2,board);
        List<int> moves = [0,4,8,6,3,7,1,5,2];
        foreach(int move in moves){
            game.MakeMove(move);
        }
        Assert.True(game.GameOver());
        Assert.False(game.Draw());
        Assert.Equal(player1,game.Winner());
    }
    [Fact]
    public void MakeMove_OccupiedSpace_ArgumentOutOfRangeException()
    {
        Board board = new Board();
        Player player1 = new Player("player1",BoxVal.X);
        Player player2 = new Player("player2",BoxVal.O);
        Game game = new Game(player1,player2,board);
        game.MakeMove(0);
        Assert.Throws<ArgumentOutOfRangeException>(()=>game.MakeMove(0));
    }
}
