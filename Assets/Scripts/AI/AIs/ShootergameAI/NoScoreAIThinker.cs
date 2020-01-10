using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class NoScoreAIThinker : IThinker
{
    private System.Random random;

    private List<Pos> enemyPiece = new List<Pos>();

    private List<Pos> myPiece = new List<Pos>();

    private List<Pos> allPiece = new List<Pos>();

    PColor color;

    PShape shape;

    private List<Play> myWinCorridors = new List<Play>();

    struct Play
    {
        public int? pos { get; set; }
        public int score { get; set; }
        public Play(int? pos, int score)
        {
            this.pos = pos;
            this.score = score;
        }
    }

    public FutureMove Think(Board board, CancellationToken ct)
    {
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

        play = Negamax(board, board.Turn, maxDepth, ct);

        if (test != null)
        {
            if (play.pos == null)
            {
                return FutureMove.NoMove;
            }
            else
            {
                return new FutureMove((int)play.pos, PShape.Round);
            }
        }

        return new FutureMove(random.Next(0, board.cols), shape);
    }

    private Play Negamax(Board board, PColor turn, int maxDepth, CancellationToken ct)
    {
        Play bestMove = new Play(null, int.MinValue);

        PColor proxTurn =
            turn == PColor.Red ? PColor.White : PColor.Red;

        bool stup = false;

        if (ct.IsCancellationRequested)
        {
            return new Play(null, 0);
        }
        else
        {
            if (maxDepth <= 0)
            {
                return bestMove;
            }

            foreach (Play play in myWinCorridors)
            {
                for (int j = 0; j < board.cols; j++)
                {
                    int pos = j;

                    if (board[(int)play.pos, j] == null)
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

                        maxDepth--;

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
                            bestMove.pos = pos;
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
}
