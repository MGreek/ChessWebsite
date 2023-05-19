using System.Collections.Generic;

namespace Chess.Models
{
    public enum PlayerColor
    {
        Black,
        White
    }

    public enum PieceType
    {
        King,
        Queen,
        Rook,
        Bishop,
        Knight,
        Pawn
    }
    
    public enum PromotionPieceType
    {
        Queen,
        Rook,
        Bishop,
        Knight
    }

    public class Piece
    {
        public PlayerColor Color { get; set; }
        public PieceType Type { get; set; }
    }

    public struct Coordinates
    {
        public int X { get; set; }
        public int Y { get; set; }

        public static bool operator ==(Coordinates left, Coordinates right)
        {
            return left.X == right.X && left.Y == right.Y;
        }

        public static bool operator !=(Coordinates left, Coordinates right)
        {
            return !(left == right);
        }
    }

    public struct Move
    {
        public Coordinates Source { get; set; }
        public Coordinates Destination { get; set; }
        public PromotionPieceType? PromotionType { get; set; }

        public static bool operator ==(Move left, Move right)
        {
            return left.Destination == right.Destination && left.Source == right.Source;
        }

        public static bool operator !=(Move left, Move right)
        {
            return !(left == right);
        }
    }

    public class ChessboardModel
    {
        public static Piece[,] StartingPosition { get; } = new Piece[8, 8];

        static ChessboardModel()
        {
            StartingPosition[0, 0] = new Piece { Color = PlayerColor.Black, Type = PieceType.Rook };
            StartingPosition[0, 1] = new Piece { Color = PlayerColor.Black, Type = PieceType.Knight };
            StartingPosition[0, 2] = new Piece { Color = PlayerColor.Black, Type = PieceType.Bishop };
            StartingPosition[0, 3] = new Piece { Color = PlayerColor.Black, Type = PieceType.Queen };
            StartingPosition[0, 4] = new Piece { Color = PlayerColor.Black, Type = PieceType.King };
            StartingPosition[0, 5] = new Piece { Color = PlayerColor.Black, Type = PieceType.Bishop };
            StartingPosition[0, 6] = new Piece { Color = PlayerColor.Black, Type = PieceType.Knight };
            StartingPosition[0, 7] = new Piece { Color = PlayerColor.Black, Type = PieceType.Rook };

            for (int i = 0; i < 8; i++)
            { StartingPosition[1, i] = new Piece { Color = PlayerColor.Black, Type = PieceType.Pawn }; }

            StartingPosition[7, 0] = new Piece { Color = PlayerColor.White, Type = PieceType.Rook };
            StartingPosition[7, 1] = new Piece { Color = PlayerColor.White, Type = PieceType.Knight };
            StartingPosition[7, 2] = new Piece { Color = PlayerColor.White, Type = PieceType.Bishop };
            StartingPosition[7, 3] = new Piece { Color = PlayerColor.White, Type = PieceType.Queen };
            StartingPosition[7, 4] = new Piece { Color = PlayerColor.White, Type = PieceType.King };
            StartingPosition[7, 5] = new Piece { Color = PlayerColor.White, Type = PieceType.Bishop };
            StartingPosition[7, 6] = new Piece { Color = PlayerColor.White, Type = PieceType.Knight };
            StartingPosition[7, 7] = new Piece { Color = PlayerColor.White, Type = PieceType.Rook };

            for (int i = 0; i < 8; i++)
            { StartingPosition[6, i] = new Piece { Color = PlayerColor.White, Type = PieceType.Pawn }; }
        }

        public static Move BlackKingShortCastle { get; } = new Move() { Source = new Coordinates() { X = 0, Y = 4 }, Destination = new Coordinates() { X = 0, Y = 6 } };
        public static Move BlackKingLongCastle { get; } = new Move() { Source = new Coordinates() { X = 0, Y = 4 }, Destination = new Coordinates() { X = 0, Y = 2 } };
        public static Move WhiteKingShortCastle { get; } = new Move() { Source = new Coordinates() { X = 7, Y = 4 }, Destination = new Coordinates() { X = 7, Y = 6 } };
        public static Move WhiteKingLongCastle { get; } = new Move() { Source = new Coordinates() { X = 7, Y = 4 }, Destination = new Coordinates() { X = 7, Y = 2 } };

        public static Move BlackRookShortCastle { get; } = new Move() { Source = new Coordinates() { X = 0, Y = 7 }, Destination = new Coordinates() { X = 0, Y = 5 } };
        public static Move BlackRookLongCastle { get; } = new Move() { Source = new Coordinates() { X = 0, Y = 0 }, Destination = new Coordinates() { X = 0, Y = 3 } };
        public static Move WhiteRookShortCastle { get; } = new Move() { Source = new Coordinates() { X = 7, Y = 7 }, Destination = new Coordinates() { X = 7, Y = 5 } };
        public static Move WhiteRookLongCastle { get; } = new Move() { Source = new Coordinates() { X = 7, Y = 0 }, Destination = new Coordinates() { X = 7, Y = 3 } };

        public List<Move> History { get; set; } = new List<Move>();
    }
}