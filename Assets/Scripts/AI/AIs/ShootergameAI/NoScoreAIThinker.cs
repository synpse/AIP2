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

        return new FutureMove(random.Next(0, board.cols), shape);
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
