const newgame = document.getElementById("new-game");
const status = document.getElementById("status");
const winner = document.getElementById("winner");
const cells = document.querySelectorAll(".cell");
let gameComplete = false;
let id = null;
newgame.addEventListener("click", async () => {
    const response = await fetch("http://localhost:5077/game/new", { method: "POST" });
    const data = await response.json();
    id = data.id;
    gameComplete = false;
    updateBoard(data.state);
});
cells.forEach(cell => {
    cell.addEventListener("click", async () => {
        if (id === null)
            return;
        if (gameComplete === true)
            return;
        const index = cell.getAttribute("data-index");
        if (cell.textContent === "") {
            const response = await fetch("http://localhost:5077/game/move", { method: "POST", headers: { "Content-Type": "application/json" }, body: JSON.stringify({ gameId: id, playerChoice: Number(index) }) });
            const data = await response.json();
            updateBoard(data);
        }
    });
});
function updateBoard(gamestate) {
    if (gamestate.isOver === false) {
        status.textContent = "Player turn: " + gamestate.currentPlayer;
    }
    let counter = 0;
    cells.forEach(cell => {
        if (gamestate.board[counter] === "none") {
            cell.textContent = "";
        }
        else if (gamestate.board[counter] === "X") {
            cell.textContent = "X";
        }
        else if (gamestate.board[counter] === "O") {
            cell.textContent = "O";
        }
        counter++;
    });
    if (gamestate.winner !== null) {
        winner.textContent = "Winner is " + gamestate.winner;
        status.textContent = "Game Over";
        gameComplete = true;
    }
    else if (gamestate.winner === null && gamestate.isOver) {
        winner.textContent = "It is a draw";
        status.textContent = "Game Over";
        gameComplete = true;
    }
    else
        winner.textContent = null;
}
export {};
//# sourceMappingURL=game.js.map