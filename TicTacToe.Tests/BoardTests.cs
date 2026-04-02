using Xunit.Abstractions;

public class BoardTests
{
    private readonly ITestOutputHelper _output;
    public BoardTests(ITestOutputHelper output)
    {
        _output = output;
    }
    [Fact]
    public void NewBoardDatabaseRecordCorrectState()
    {
        string dbBoardRecord = "X,none,none,none,none,none,none,none,none";
        string[] record = dbBoardRecord.Split(',');
        Board board = new Board(record);
        Assert.Equal(board.BoardState(),record);
    }
}