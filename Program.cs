Board board = new Board();
Player player1 = new Player("Yan",BoxVal.X);
Player player2 = new Player("Mari",BoxVal.O);
Player []playerlist = [player1,player2];
Game game = new Game(player1,player2,board);

board.DisplayGrid();
while (!game.GameOver())
{
    foreach(Player player in playerlist)
    {
        Console.WriteLine(Utils.TextGen(player.Name));
        int selection = Utils.Validator(board.AvailableSquares());
        game.PlayerMove(player,selection);
        if (game.GameOver())
        {
            if(game.Draw())Console.WriteLine("It is a draw");
            else Console.WriteLine(game.Winner().Name + " won in "+player.MovesCount()+" moves");
            break;
        }
    }
}
