using System;
using System.Collections.Generic;
using System.Threading;

/// <summary>
/// Class ShooterGameAIThinker that contains the AI, inherits from IThinker.
/// </summary>
public class G08ShooterGameExeThinker : IThinker
{
    private const int win = 100000;

    private const int block = 500;

    private const int oneGood = 5;

    private const int blockedCorridor = 10;

    private readonly List<Pos> positions = new List<Pos>();

    private readonly List<Pos> enemyPiece = new List<Pos>();

    private readonly List<Pos> myPiece = new List<Pos>();

    private readonly List<Pos> allPiece = new List<Pos>();

    private readonly List<Play> myWinCorridors = new List<Play>();

    private const int maxDepth = 4;

    private int cDepth = 0;

    private Random random;

    PColor color;

    PShape shape;

    enum Player
    {
        me,
        enemy
    }

    enum CheckType
    {
        color,
        shape
    }

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
        FutureMove? test;
        Play play;
        random = new Random();

        color = board.Turn;

        if (color == PColor.White)
            shape = PShape.Round;

        else
            shape = PShape.Square;

        if (shape == PShape.Round && 
            board.PieceCount(color, PShape.Round) == 0)
            shape = PShape.Square;

        else if (shape == PShape.Square && 
            board.PieceCount(color, PShape.Square) == 0)
            shape = PShape.Round;

        Check(board);

        test = CheckPlayer(board, Player.me);
        if (test != null)
            return (FutureMove)test;

        test = CheckPlayer(board, Player.enemy);
        if (test != null)
            return (FutureMove)test;

        PColor myColor = board.Turn;
        PShape myShape = shape;

        CheckWinCorridors(Player.me, myColor, myShape, board);

        PColor enemyColor =
            color == PColor.White ? PColor.Red : PColor.White;
        PShape enemyShape =
            shape == PShape.Round ? PShape.Square : PShape.Round;

        CheckWinCorridors(Player.enemy, enemyColor, enemyShape, board);

        play = Negamax(board, board.Turn, maxDepth, ct);

        if (test != null)
        {
            if (play.posCol == null)
                return FutureMove.NoMove;

            else
                return new FutureMove((int)play.posCol, PShape.Round);
        }

        return new FutureMove(random.Next(0, board.cols), shape);
    }

    private void CheckWinCorridors(
        Player player, 
        PColor color, 
        PShape shape, 
        Board board)
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

                        NewPlay(pos, piece, player, color, shape);
                    }

                }
                else if (pos.col == board.cols - 1)
                {
                    if (board[pos.row, pos.col - 1] != null)
                    {
                        piece = (Piece)board[pos.row, pos.col - 1];

                        NewPlay(pos, piece, player, color, shape);
                    }
                }
                else
                {
                    if (board[pos.row, pos.col + 1] != null)
                    {
                        piece = (Piece)board[pos.row, pos.col + 1];

                        NewPlay(pos, piece, player, color, shape);
                    }

                    if (board[pos.row, pos.col - 1] != null)
                    {
                        piece = (Piece)board[pos.row, pos.col - 1];

                        NewPlay(pos, piece, player, color, shape);
                    }
                }
            }
        }
    }

    private void NewPlay(
        Pos pos, 
        Piece piece,
        Player player,
        PColor color, 
        PShape shape)
    {
        if (piece.color == color ||
            piece.shape == shape)
        {
            if (player == Player.me)
                myWinCorridors.Add(
                    new Play(
                        pos.col,
                        pos.row,
                        oneGood));

            else
                myWinCorridors.Add(
                    new Play(
                        pos.col,
                        pos.row,
                        blockedCorridor));
        }
    }

    private Play Negamax(
        Board board, 
        PColor turn, 
        int maxDepth, 
        CancellationToken ct)
    {
        Play bestMove = new Play(null, null, int.MinValue);

        PColor proxTurn =
            turn == PColor.Red ? PColor.White : PColor.Red;

        if (ct.IsCancellationRequested)
            return new Play(null, null, 0);
        else
        {
            if (cDepth == maxDepth)
                return bestMove;

            cDepth++;

            foreach (Play play in myWinCorridors)
            {
                for (int j = 0; j < board.cols; j++)
                {
                    int pos = j;

                    if (board[(int)play.posCol, (int)play.posRow] == null)
                    {
                        int roundPieces = 
                            board.PieceCount(board.Turn, PShape.Round);

                        int squarePieces = 
                            board.PieceCount(board.Turn, PShape.Square);

                        if (shape == PShape.Round)
                            if (roundPieces == 0)
                                shape = PShape.Square;
                        else
                            if (squarePieces == 0)
                                shape = PShape.Round;

                        Play move = default;

                        board.DoMove(shape, j);

                        if (board.CheckWinner() == Winner.None)
                            move = Negamax(board, proxTurn, maxDepth, ct);

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
                        CompareColors(pos, hasPiece, myPiece);
                    }
                    else if (piece.color != board.Turn)
                    {
                        CompareColors(pos, hasPiece, enemyPiece);
                    }

                    foreach (Pos position in allPiece)
                        if (position.col == pos.col && 
                            position.row == pos.row)
                            hasPiece = true;

                    if (!hasPiece)
                        allPiece.Add(pos);
                }
            }
        }
    }

    private void CompareColors(
        Pos pos, 
        bool hasPiece,
        List<Pos> pieces)
    {
        foreach (Pos position in pieces)
            if (position.col == pos.col &&
                position.row == pos.row)
                hasPiece = true;

        if (!hasPiece)
            pieces.Add(pos);
    }

    private FutureMove? CheckPlayer(Board board, Player player)
    {
        FutureMove? move = null;

        foreach (Pos pos in allPiece)
            if (move == null)
                move = CheckCols(
                    board, 
                    pos.col, 
                    player, 
                    CheckType.shape);

        if (move == null)
            foreach (Pos pos in allPiece)
                if (move == null)
                    move = CheckCols(
                        board, 
                        pos.col, 
                        player, 
                        CheckType.color);

        if (move == null)
            foreach (Pos pos in allPiece)
                if (move == null)
                    move = CheckRows(
                        board, 
                        pos.row, 
                        player);

        return move;
    }

    private FutureMove? CheckRows(
        Board board, 
        int row, 
        Player player)
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
                if (player == Player.me)
                {
                    if (board[row, col].Value.color == color ||
                        board[row, col].Value.shape == shape)
                        check[col] = "O";

                    else if (board[row, col].Value.color != color ||
                        board[row, col].Value.shape != shape)
                        check[col] = "X";
                }
                else
                {
                    if (board[row, col].Value.color != color ||
                        board[row, col].Value.shape != shape)
                        check[col] = "O";

                    else if (board[row, col].Value.color == color ||
                        board[row, col].Value.shape == shape)
                        check[col] = "X";
                }
            }
            else
                check[col] = "_";
        }

        for (int i = 0; i < check.Length; i++)
            result += check[i];

        if (result.Contains(pos1))
            return PlayWinBlockRow(pos1, check);

        else if (result.Contains(pos2))
            return PlayWinBlockRow(pos2, check);

        else if (result.Contains(pos3))
            return PlayWinBlockRow(pos3, check);

        else if (result.Contains(pos4))
            return PlayWinBlockRow(pos4, check);

        return null;
    }

    private FutureMove? PlayWinBlockRow(string pos, string[] results)
    {
        int wCol = 0;
        int x = 0;

        List<int> cols = new List<int>();

        for (int i = 0; i < results.Length; i++)
        {
            if (pos.Contains(results[i]))
            {
                cols.Add(i);
                if (results[i] == "_") 
                    wCol = i;

                if (cols.Count == 4)
                    return new FutureMove(wCol, shape);

                x++;
            }
            else
                if (cols.Count < 4)
                    cols.RemoveRange(0, cols.Count);
        }

        return null;
    }


    private FutureMove? CheckCols(
        Board board, 
        int col, 
        Player player,
        CheckType checkType)
    {
        List<bool> threeInLine = new List<bool>(3);
        Piece piece;

        PColor enemyColor = 
            color == PColor.White ? PColor.Red : PColor.White;

        PShape enemyShape =
            color == PColor.White ? PShape.Square : PShape.Round;

        for (int i = 0; i < board.rows; i++)
        {
            if (board[i, col] == null)
                return null;

            piece = (Piece)board[i, col];

            if (checkType == CheckType.color)
            {
                if (piece.color == board.Turn)
                    threeInLine.Add(true);
                else
                    threeInLine.RemoveRange(0, threeInLine.Count);
            }
            else
            {
                if (piece.shape == shape)
                    threeInLine.Add(true);
                else
                    threeInLine.RemoveRange(0, threeInLine.Count);
            }

            if (threeInLine.Count == 3)
            {
                if (!board.IsColumnFull(col))
                {
                    if (board[i + 1, col].HasValue ||
                        i == board.rows)
                    {
                        piece = (Piece)board[i + 1, col];

                        if (checkType == CheckType.color)
                            if (piece.color == enemyColor)
                                threeInLine.RemoveRange(
                                    0,
                                    threeInLine.Count);
                            else
                            if (piece.shape == enemyShape)
                                threeInLine.RemoveRange(
                                    0,
                                    threeInLine.Count);
                    }
                    else
                    {
                        if (player == Player.me)
                            myWinCorridors.Add(new Play(col, i, win));
                        else 
                            myWinCorridors.Add(new Play(col, i, block));

                        return new FutureMove(col, shape);
                    }
                }
            }
        }

        return null;
    }
}
