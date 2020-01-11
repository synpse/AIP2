using System.Collections.Generic;
using System.Threading;

/// <summary>
/// Class ShooterGameAIThinker that contains the AI, inherts from IThinker.
/// </summary>
public class ShootergameAIThinker : IThinker
{
    private const int win = 100000;

    private const int block = 500;

    private const int oneGood = 5;

    private const int blockedCorridor = 10;

    private List<Pos> positions = new List<Pos>();

    private List<Pos> enemyPiece = new List<Pos>();

    private List<Pos> myPiece = new List<Pos>();

    private List<Pos> allPiece = new List<Pos>();

    private const int maxDepth = 4;

    private int cDepth = 0;

    private System.Random random;

    PColor color;

    PShape shape;

    private List<Play> myWinCorridors = new List<Play>();

    struct Play
    {
        public int? posCol { get; set; }
        public int? posRow { get; set; }
        public int score { get; set; }
        public Play(int? posCol, int? posRow, int score)
        {
            this.posCol = posCol;
            this.posRow = posRow;
            this.score = score;
        }
    }

    public FutureMove Think(Board board, CancellationToken ct)
    {
        FutureMove? test = null;
        Play play;
        random = new System.Random();

        color = board.Turn;

        if (color == PColor.White)
        {
            shape = PShape.Round;
        }
        else
        {
            shape = PShape.Square;
        }

        if (shape == PShape.Round && board.PieceCount(color, PShape.Round) == 0)
        {
            shape = PShape.Square;
        }
        else if (shape == PShape.Square && board.PieceCount(color, PShape.Square) == 0)
        {
            shape = PShape.Round;
        }

        Check(board);

        test = PlayPiece(board);
        if (test != null)
        {
            return (FutureMove)test;
        }

        test = CheckEnemy(board);
        if (test != null)
        {
            return (FutureMove)test;
        }

        CheckWinCorridors(board);
        CheckEnemyWinCorridors(board);
        play = Negamax(board, board.Turn, maxDepth, ct);

        if (test != null)
        {
            if (play.posCol == null)
            {
                return FutureMove.NoMove;
            }
            else
            {
                return new FutureMove((int)play.posCol, PShape.Round);
            }
        }

        return new FutureMove(random.Next(0, board.cols), shape);
    }

    private void CheckWinCorridors(Board board)
    {
        Piece piece;
        foreach (IEnumerable<Pos> enumerable in board.winCorridors)
        {
            foreach (Pos pos in enumerable)
            {
                positions.Add(pos);
            }
        }

        foreach (Pos pos in positions)
        {
            if (board[pos.row, pos.col] == null)
            {
                if (pos.col == 0)
                {
                    if (board[pos.row, pos.col + 1] != null)
                    {
                        piece = (Piece)board[pos.row, pos.col + 1];

                        if (piece.color == board.Turn || piece.shape == shape)
                        {
                            myWinCorridors.Add(new Play(pos.col, pos.row, oneGood));
                        }
                    }

                }
                else if (pos.col == board.cols - 1)
                {
                    if (board[pos.row, pos.col - 1] != null)
                    {
                        piece = (Piece)board[pos.row, pos.col - 1];

                        if (piece.color == board.Turn || piece.shape == shape)
                        {
                            myWinCorridors.Add(new Play(pos.col, pos.row, oneGood));
                        }
                    }
                }
                else
                {
                    if (board[pos.row, pos.col + 1] != null)
                    {
                        piece = (Piece)board[pos.row, pos.col + 1];

                        if (piece.color == board.Turn || piece.shape == shape)
                        {
                            myWinCorridors.Add(new Play(pos.col, pos.row, oneGood));
                        }
                    }

                    if (board[pos.row, pos.col - 1] != null)
                    {
                        piece = (Piece)board[pos.row, pos.col - 1];
                        if (piece.color == board.Turn || piece.shape == shape)
                        {
                            myWinCorridors.Add(new Play(pos.col, pos.row, oneGood));
                        }
                    }
                }

            }
        }

    }

    private void CheckEnemyWinCorridors(Board board)
    {
        Piece piece;
        PColor enemyColor =
            color == PColor.White ? PColor.Red : PColor.White;
        PShape enemyShape =
            shape == PShape.Round ? PShape.Square : PShape.Round;

        foreach (IEnumerable<Pos> enumerable in board.winCorridors)
        {
            foreach (Pos pos in enumerable)
            {
                positions.Add(pos);
            }
        }

        foreach (Pos pos in positions)
        {
            if (board[pos.row, pos.col] == null)
            {
                if (pos.col == 0)
                {
                    if (board[pos.row, pos.col + 1] != null)
                    {
                        piece = (Piece)board[pos.row, pos.col + 1];

                        if (piece.color == enemyColor || piece.shape == enemyShape)
                        {
                            myWinCorridors.Add(new Play(pos.col, pos.row, blockedCorridor));
                        }
                    }

                }
                else if (pos.col == board.cols - 1)
                {
                    if (board[pos.row, pos.col - 1] != null)
                    {
                        piece = (Piece)board[pos.row, pos.col - 1];

                        if (piece.color == enemyColor || piece.shape == enemyShape)
                        {
                            myWinCorridors.Add(new Play(pos.col, pos.row, blockedCorridor));
                        }
                    }
                }
                else
                {
                    if (board[pos.row, pos.col + 1] != null)
                    {
                        piece = (Piece)board[pos.row, pos.col + 1];

                        if (piece.color == enemyColor || piece.shape == enemyShape)
                        {
                            myWinCorridors.Add(new Play(pos.col, pos.row, blockedCorridor));
                        }
                    }

                    if (board[pos.row, pos.col - 1] != null)
                    {
                        piece = (Piece)board[pos.row, pos.col - 1];
                        if (piece.color == enemyColor || piece.shape == enemyShape)
                        {
                            myWinCorridors.Add(new Play(pos.col, pos.row, blockedCorridor));
                        }
                    }
                }

            }
        }
    }

    private Play Negamax(Board board, PColor turn, int maxDepth, CancellationToken ct)
    {
        Play bestMove = new Play(null, null, int.MinValue);

        PColor proxTurn =
            turn == PColor.Red ? PColor.White : PColor.Red;

        bool stup = false;

        if (ct.IsCancellationRequested)
        {
            return new Play(null, null, 0);
        }
        else
        {
            if (cDepth == maxDepth)
            {
                return bestMove;
            }

            cDepth++;

            foreach (Play play in myWinCorridors)
            {
                for (int j = 0; j < board.cols; j++)
                {
                    int pos = j;

                    if (board[(int)play.posCol, (int)play.posRow] == null)
                    {
                        int roundPieces = board.PieceCount(board.Turn, PShape.Round);
                        int squarePieces = board.PieceCount(board.Turn, PShape.Square);
                        if (shape == PShape.Round)
                        {
                            if (roundPieces == 0)
                            {
                                shape = PShape.Square;
                            }
                        }
                        else
                        {
                            if (squarePieces == 0)
                            {
                                shape = PShape.Round;
                            }
                        }

                        Play move = default;

                        board.DoMove(shape, j);

                        if (board.CheckWinner() != Winner.None)
                        {
                            stup = true;
                        }

                        if (!stup)
                        {
                            move = Negamax(board, proxTurn, maxDepth, ct);
                        }

                        board.UndoMove();

                        move.score = -move.score;

                        if (move.score > bestMove.score)
                        {
                            bestMove.score = move.score;
                            bestMove.posCol = pos;
                        }
                    }
                }
            }

            return bestMove;
        }
    }

    private void Check(Board board)
    {
        Piece piece;
        Pos pos;
        bool hasPiece;

        for (int i = 0; i < board.rows; i++)
        {
            for (int z = 0; z < board.cols; z++)
            {
                if (board[i, z] != null)
                {
                    hasPiece = false;
                    piece = (Piece)board[i, z];
                    pos = new Pos(i, z);
                    if (piece.color == board.Turn)
                    {
                        foreach (Pos position in myPiece)
                        {
                            if (position.col == pos.col && position.row == pos.row)
                            {
                                hasPiece = true;
                            }
                        }

                        if (!hasPiece)
                        {
                            myPiece.Add(pos);
                        }

                    }
                    else if (piece.color != board.Turn)
                    {
                        foreach (Pos position in enemyPiece)
                        {
                            if (position.col == pos.col && position.row == pos.row)
                            {
                                hasPiece = true;
                            }
                        }

                        if (!hasPiece)
                        {
                            enemyPiece.Add(pos);
                        }
                    }
                    foreach (Pos position in allPiece)
                    {
                        if (position.col == pos.col && position.row == pos.row)
                        {
                            hasPiece = true;
                        }
                    }

                    if (!hasPiece)
                    {
                        allPiece.Add(pos);
                    }
                }
            }
        }
    }

    private FutureMove? PlayPiece(Board board)
    {
        FutureMove? move = null;

        foreach (Pos pos in allPiece)
        {
            if (move == null)
            {
                move = CheckColsShape(board, pos.col);
            }
        }

        if (move == null)
        {
            foreach (Pos pos in allPiece)
            {
                if (move == null)
                {
                    move = CheckCols(board, pos.col);
                }
            }
        }

        if (move == null)
        {
            foreach (Pos pos in allPiece)
            {
                if (move == null)
                {
                    move = CheckRowsColorShape(board, pos.row);
                }
            }
        }

        return move;

    }

    private FutureMove? CheckRowsColorShape(Board board, int row)
    {
        string[] check = new string[board.cols];
        string result = "";
        const string pos1 = "_OOO";
        const string pos2 = "O_OO";
        const string pos3 = "OO_O";
        const string pos4 = "OOO_";

        for (int col = 0; col < board.cols; col++)
        {
            if (board[row, col] != null)
            {
                if (board[row, col].Value.color == color || board[row, col].Value.shape == shape)
                {
                    check[col] = "O";
                }
                else if (board[row, col].Value.color != color || board[row, col].Value.shape != shape)
                {
                    check[col] = "X";
                }
            }
            else
            {
                check[col] = "_";
            }
        }

        for (int i = 0; i < check.Length; i++)
        {
            result += check[i];
        }

        if (result.Contains(pos1))
        {
            return PlayWinBlockRow(board, pos1, check);
        }
        else if (result.Contains(pos2))
        {
            return PlayWinBlockRow(board, pos2, check);
        }
        else if (result.Contains(pos3))
        {
            return PlayWinBlockRow(board, pos3, check);
        }
        else if (result.Contains(pos4))
        {
            return PlayWinBlockRow(board, pos4, check);
        }
        return null;
    }

    private FutureMove? CheckEnemyRowsColorShape(Board board, int row)
    {
        string[] check = new string[board.cols];
        string result = "";
        const string pos1 = "_OOO";
        const string pos2 = "O_OO";
        const string pos3 = "OO_O";
        const string pos4 = "OOO_";

        for (int col = 0; col < board.cols; col++)
        {
            if (board[row, col] != null)
            {
                if (board[row, col].Value.color != color || board[row, col].Value.shape != shape)
                {
                    check[col] = "O";
                }
                else if (board[row, col].Value.color == color || board[row, col].Value.shape == shape)
                {
                    check[col] = "X";
                }
            }
            else
            {
                check[col] = "_";
            }
        }

        for (int i = 0; i < check.Length; i++)
        {
            result += check[i];
        }

        if (result.Contains(pos1))
        {

            return PlayWinBlockRow(board, pos1, check);
        }
        else if (result.Contains(pos2))
        {

            return PlayWinBlockRow(board, pos2, check);
        }
        else if (result.Contains(pos3))
        {
            return PlayWinBlockRow(board, pos3, check);
        }
        else if (result.Contains(pos4))
        {
            return PlayWinBlockRow(board, pos4, check);
        }
        return null;
    }

    private FutureMove? PlayWinBlockRow(Board board, string pos, string[] results)
    {
        int wCol = 0;

        List<int> cols = new List<int>();
        int x = 0;

        for (int i = 0; i < results.Length; i++)
        {
            if (pos.Contains(results[i]))
            {
                cols.Add(i);
                if (results[i] == "_") wCol = i;
                if (cols.Count == 4)
                {
                    return new FutureMove(wCol, shape);
                }
                x++;
            }
            else
            {
                if (cols.Count < 4)
                {
                    cols.RemoveRange(0, cols.Count);
                }
            }
        }

        return null;
    }


    private FutureMove? CheckCols(Board board, int col)
    {
        List<bool> threeInLine = new List<bool>(3);
        Piece piece;
        PColor enemyColor = color == PColor.White ? PColor.Red : PColor.White;

        for (int i = 0; i < board.rows; i++)
        {
            if (board[i, col] == null)
            {
                return null;
            }
            piece = (Piece)board[i, col];
            if (piece.color == board.Turn)
            {
                threeInLine.Add(true);
            }
            else
            {
                threeInLine.RemoveRange(0, threeInLine.Count);
            }
            if (threeInLine.Count == 3)
            {
                if (!board.IsColumnFull(col))
                {
                    if (board[i + 1, col].HasValue || i == board.rows)
                    {
                        piece = (Piece)board[i + 1, col];
                        if (piece.color == enemyColor)
                        {
                            threeInLine.RemoveRange(0, threeInLine.Count);
                        }
                    }
                    else
                    {
                        myWinCorridors.Add(new Play(col, i, win));
                        return new FutureMove(col, shape);
                    }
                }
                
            }
        }

        return null;
    }

    private FutureMove? CheckColsShape(Board board, int col)
    {
        FutureMove? move;
        List<bool> threeInLine = new List<bool>();
        Piece piece;
        PShape enemyShape =
            color == PColor.White ? PShape.Square : PShape.Round;

        for (int i = 0; i < board.rows; i++)
        {
            if (board[i, col] == null)
            {
                return null;
            }
            piece = (Piece)board[i, col];
            if (piece.shape == shape)
            {
                threeInLine.Add(true);
            }
            else
            {
                threeInLine.RemoveRange(0, threeInLine.Count);
            }
            if (threeInLine.Count == 3)
            {
                if (!board.IsColumnFull(col))
                {
                    if (board[i + 1, col].HasValue || i == board.rows)
                    {
                        piece = (Piece)board[i + 1, col];
                        if (piece.shape == enemyShape)
                        {
                            threeInLine.RemoveRange(0, threeInLine.Count);
                        }
                    }
                    else
                    {
                        myWinCorridors.Add(new Play(col, i, win));
                        return new FutureMove(col, shape);

                    }
                }               

            }
        }

        return null;
    }

    private FutureMove? CheckEnemy(Board board)
    {
        FutureMove? move = null;

        foreach (Pos pos in allPiece)
        {
            if (move == null)
            {
                move = CheckEnemyColsShape(board, pos.col);

            }
        }

        if (move == null)
        {
            foreach (Pos pos in allPiece)
            {
                if (move == null)
                {
                    move = CheckEnemyCols(board, pos.col);

                }
            }
        }

        if (move == null)
        {
            foreach (Pos pos in allPiece)
            {
                if (move == null)
                {
                    move = CheckEnemyRowsColorShape(board, pos.row);
                }
            }
        }

        return move;
    }

    private FutureMove? CheckEnemyCols(Board board, int col)
    {
        FutureMove? move;
        List<bool> threeInLine = new List<bool>(3);
        Piece piece;
        PColor enemyColor = color == PColor.White ? PColor.Red : PColor.White;

        for (int i = 0; i < board.rows; i++)
        {
            if (board[i, col] == null)
            {
                return null;
            }

            piece = (Piece)board[i, col];
            if (piece.color == enemyColor)
            {
                threeInLine.Add(true);
            }
            else
            {
                threeInLine.RemoveRange(0, threeInLine.Count);
            }
            if (threeInLine.Count == 3)
            {
                if (!board.IsColumnFull(col))
                {
                    if (board[i + 1, col].HasValue || i == board.rows)
                    {
                        piece = (Piece)board[i + 1, col];
                        if (piece.color == color)
                        {
                            threeInLine.RemoveRange(0, threeInLine.Count);
                        }
                    }
                    else
                    {
                        myWinCorridors.Add(new Play(col, i, block));
                        return new FutureMove(col, shape);
                    }
                }             

            }
        }

        return null;
    }

    private FutureMove? CheckEnemyColsShape(Board board, int col)
    {
        FutureMove? move;
        List<bool> threeInLine = new List<bool>();
        Piece piece;
        PShape enemyShape =
            color == PColor.White ? PShape.Square : PShape.Round;

        for (int i = 0; i < board.rows; i++)
        {
            if (board[i, col] == null)
            {
                return null;
            }
            piece = (Piece)board[i, col];
            if (piece.shape == enemyShape)
            {
                threeInLine.Add(true);
            }
            else
            {
                threeInLine.RemoveRange(0, threeInLine.Count);
            }
            if (threeInLine.Count == 3)
            {
                if (!board.IsColumnFull(col))
                {
                    if (board[i + 1, col].HasValue || i == board.rows)
                    {
                        piece = (Piece)board[i + 1, col];
                        if (piece.shape == shape)
                        {
                            threeInLine.RemoveRange(0, threeInLine.Count);
                        }
                    }
                    else
                    {
                        myWinCorridors.Add(new Play(col, i, block));
                        return new FutureMove(col, shape);
                    }
                }
                
            }
        }

        return null;
    }
}
