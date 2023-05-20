using Chess.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Chess.Services
{
    public partial class ChessboardService
    {
        static bool CheckCoordinates(Coordinates coordinates)
        {
            return (coordinates.X >= 0) && (coordinates.X < 8) && (coordinates.Y >= 0) && (coordinates.Y < 8);
        }

        static Piece GetPieceFromPosition(Coordinates coordinates, Piece[,] position)
        {
            if (!CheckCoordinates(coordinates))
                return null;
            return position[coordinates.X, coordinates.Y];
        }

        static PieceType GetPieceTypeFromPromotionPieceType(PromotionPieceType currentPromotion)
        {
            switch (currentPromotion)
            {
                case PromotionPieceType.Queen:
                    return PieceType.Queen;
                case PromotionPieceType.Rook:
                    return PieceType.Rook;
                case PromotionPieceType.Bishop:
                    return PieceType.Bishop;
                case PromotionPieceType.Knight:
                    return PieceType.Knight;
                default:
                    throw new InvalidOperationException();
            }
        }

        void PlainMove(Move move, Piece[,] position)
        {
            position[move.Destination.X, move.Destination.Y] = position[move.Source.X, move.Source.Y];
            position[move.Source.X, move.Source.Y] = null;
        }

        Move Move(Move move, Piece[,] currentPosition)
        {
            Piece piece = GetPieceFromPosition(move.Source, currentPosition);
            Piece other = GetPieceFromPosition(move.Destination, currentPosition);
            // Handle the castling
            if (piece.Type == PieceType.King)
            {
                if (move == ChessboardModel.BlackKingShortCastle)
                { PlainMove(ChessboardModel.BlackRookShortCastle, currentPosition); }
                if (move == ChessboardModel.BlackKingLongCastle)
                { PlainMove(ChessboardModel.BlackRookLongCastle, currentPosition); }
                if (move == ChessboardModel.WhiteKingShortCastle)
                { PlainMove(ChessboardModel.WhiteRookShortCastle, currentPosition); }
                if (move == ChessboardModel.WhiteKingLongCastle)
                { PlainMove(ChessboardModel.WhiteRookLongCastle, currentPosition); }
            }
            if (piece.Type == PieceType.Pawn)
            {
                (int, int) delta = (move.Destination.X - move.Source.X, move.Destination.Y - move.Source.Y);
                (int, int)[] attackDeltas;
                if (piece.Color == PlayerColor.Black)
                { attackDeltas = new (int, int)[] { (1, -1), (1, 1) }; }
                else
                { attackDeltas = new (int, int)[] { (-1, -1), (-1, 1) }; }
                if (attackDeltas.Contains(delta) && (other == null))
                { currentPosition[move.Source.X, move.Source.Y + delta.Item2] = null; }

                if ((move.Destination.X == 0) || (move.Destination.X == 7))
                {
                    if (!move.PromotionType.HasValue)
                    { move.PromotionType = CurrentPromotion; }
                    currentPosition[move.Destination.X, move.Destination.Y] = new Piece() { Color = piece.Color, Type = GetPieceTypeFromPromotionPieceType(move.PromotionType.Value) };
                    currentPosition[move.Source.X, move.Source.Y] = null;
                    return move;
                }
            }
            PlainMove(move, currentPosition);
            return move;
        }

        List<Coordinates> GetAttackers(Coordinates coordinates, PlayerColor attackerColor)
        {
            Piece[,] currentPosition = GetCurrentPosition();
            List<Coordinates> result = new List<Coordinates>();

            (int, int)[] pawnDeltas;
            if (attackerColor == PlayerColor.Black)
            { pawnDeltas = new (int, int)[] { (-1, -1), (-1, 1) }; }
            else
            { pawnDeltas = new (int, int)[] { (1, -1), (1, 1) }; }
            foreach ((int, int) delta in pawnDeltas)
            {
                Coordinates destination = new Coordinates() { X = coordinates.X + delta.Item1, Y = coordinates.Y + delta.Item2 };
                Piece other = GetPieceFromPosition(destination, currentPosition);
                if (CheckCoordinates(destination))
                {
                    if ((other != null) && (other.Color == attackerColor) && (other.Type == PieceType.Pawn))
                    { result.Add(destination); }
                }
            }

            (int, int)[] knightDeltas = new (int, int)[]
            { (-1, 2), (-2, 1), (-2, -1), (-1, -2), (1, -2), (2, -1), (2, 1), (1, 2) };
            foreach ((int, int) delta in knightDeltas)
            {
                Coordinates destination = new Coordinates() { X = coordinates.X + delta.Item1, Y = coordinates.Y + delta.Item2 };
                Piece other = GetPieceFromPosition(destination, currentPosition);
                if (CheckCoordinates(destination))
                {
                    if ((other != null) && (other.Color == attackerColor) && (other.Type == PieceType.Knight))
                    { result.Add(destination); }
                }
            }

            (int, int)[] kingDeltas = new (int, int)[] { (-1, 1), (-1, -1), (1, -1), (1, 1), (-1, 0), (1, 0), (0, -1), (0, 1) };
            foreach ((int, int) delta in kingDeltas)
            {
                Coordinates destination = coordinates;
                destination.X += delta.Item1;
                destination.Y += delta.Item2;
                if (!CheckCoordinates(destination))
                { continue; }
                Piece other = GetPieceFromPosition(destination, currentPosition);
                if ((other != null) && (other.Color == attackerColor) && (other.Type == PieceType.King)) 
                { result.Add(destination); }
            }

            (int, int)[] bishopDeltas = new (int, int)[] { (-1, 1), (-1, -1), (1, -1), (1, 1) };
            foreach ((int, int) delta in bishopDeltas)
            {
                Coordinates destination = coordinates;
                while (true)
                {
                    destination.X += delta.Item1;
                    destination.Y += delta.Item2;
                    if (!CheckCoordinates(destination))
                    { break; }
                    Piece other = GetPieceFromPosition(destination, currentPosition);
                    if ((other != null) && (other.Color == attackerColor) && ((other.Type == PieceType.Bishop) || (other.Type == PieceType.Queen)))
                    { result.Add(destination); }
                    if (other != null)
                    { break; }
                }
            }
            (int, int)[] rookDeltas = new (int, int)[] { (-1, 0), (1, 0), (0, -1), (0, 1) };
            foreach ((int, int) delta in rookDeltas)
            {
                Coordinates destination = coordinates;
                while (true)
                {
                    destination.X += delta.Item1;
                    destination.Y += delta.Item2;
                    if (!CheckCoordinates(destination))
                    { break; }
                    Piece other = GetPieceFromPosition(destination, currentPosition);
                    if ((other != null) && (other.Color == attackerColor) && ((other.Type == PieceType.Rook) || (other.Type == PieceType.Queen)))
                    { result.Add(destination); }
                    if (other != null)
                    { break; }
                }
            }
            return result;
        }

        List<Coordinates> GetPawnMoves(Coordinates coordinates)
        {
            Piece[,] currentPosition = GetCurrentPosition();
            Piece piece = GetPieceFromPosition(coordinates, currentPosition);
            if (piece == null || piece.Type != PieceType.Pawn)
            { return new List<Coordinates>(); }

            List<Coordinates> result = new List<Coordinates>();

            (int, int)[] attackDeltas;
            if (piece.Color == PlayerColor.Black)
            { attackDeltas = new (int, int)[] { (1, -1), (1, 1) }; }
            else
            { attackDeltas = new (int, int)[] { (-1, -1), (-1, 1) }; }
            foreach ((int, int) delta in attackDeltas)
            {
                Coordinates destination = new Coordinates() { X = coordinates.X + delta.Item1, Y = coordinates.Y + delta.Item2 };
                Piece other = GetPieceFromPosition(destination, currentPosition);
                if (CheckCoordinates(destination) && (other != null) && (other.Color != piece.Color)) 
                { result.Add(destination); }
            }

            (int, int)[] enpassantDeltas = new (int, int)[] { (0, -1), (0, 1) };
            for (int i = 0; i < enpassantDeltas.Length; ++i)
            {
                (int, int) enpassantDelta = enpassantDeltas[i];
                (int, int) delta = attackDeltas[i];
                Coordinates destination = new Coordinates()
                { X = coordinates.X + delta.Item1, Y = coordinates.Y + delta.Item2 };
                Coordinates enpassantDestination = new Coordinates()
                { X = coordinates.X + enpassantDelta.Item1, Y = coordinates.Y + enpassantDelta.Item2 };
                Piece other = GetPieceFromPosition(enpassantDestination, currentPosition);
                if (CheckCoordinates(enpassantDestination) && (other != null) && (other.Color != piece.Color) &&
                    (other.Type == PieceType.Pawn) && ((Chessboard.History.Count > 0) && (Chessboard.History.Last().Destination == enpassantDestination)))
                {
                    Move lastMove = Chessboard.History.Last();
                    if (Math.Abs(lastMove.Destination.X - lastMove.Source.X) == 2)
                    { result.Add(destination); }
                }
            }

            (int, int)[] moveDeltas;
            if (piece.Color == PlayerColor.Black)
            {
                Piece other = GetPieceFromPosition(new Coordinates() { X = coordinates.X + 1, Y = coordinates.Y }, currentPosition);
                if (coordinates.X == 1 && other == null)
                { moveDeltas = new (int, int)[] { (1, 0), (2, 0) }; }
                else
                { moveDeltas = new (int, int)[] { (1, 0) }; }
            }
            else
            {
                Piece other = GetPieceFromPosition(new Coordinates() { X = coordinates.X - 1, Y = coordinates.Y }, currentPosition);
                if (coordinates.X == 6 && other == null)
                { moveDeltas = new (int, int)[] { (-1, 0), (-2, 0) }; }
                else
                { moveDeltas = new (int, int)[] { (-1, 0) }; }
            }
            foreach ((int, int) delta in moveDeltas)
            {
                Coordinates destination = new Coordinates() 
                { X = coordinates.X + delta.Item1, Y = coordinates.Y + delta.Item2 };
                Piece other = GetPieceFromPosition(destination, currentPosition);
                if (CheckCoordinates(destination) && (other == null))
                { result.Add(destination); }
            }
            return result.Where(delegate (Coordinates destination) 
            { return !CheckHangingKingMove(new Move() { Source = coordinates, Destination = destination }); }).ToList();
        }

        List<Coordinates> GetKnightMoves(Coordinates coordinates)
        {
            Piece[,] currentPosition = GetCurrentPosition();
            Piece piece = GetPieceFromPosition(coordinates, currentPosition);
            if (piece == null || piece.Type != PieceType.Knight)
            { return new List<Coordinates>(); }

            List<Coordinates> result = new List<Coordinates>();
            (int, int)[] deltas = new (int, int)[] { (-1, 2), (-2, 1), (-2, -1), (-1, -2), (1, -2), (2, -1), (2, 1), (1, 2) };
            foreach ((int, int) delta in deltas)
            {
                Coordinates destination = new Coordinates() { X = coordinates.X + delta.Item1, Y = coordinates.Y + delta.Item2 };
                Piece other = GetPieceFromPosition(destination, currentPosition);
                if (CheckCoordinates(destination) && ((other == null) || (other.Color != piece.Color)))
                { result.Add(destination); }
            }
            return result.Where(delegate (Coordinates destination) 
            { return !CheckHangingKingMove(new Move() { Source = coordinates, Destination = destination }); }).ToList();
        }

        List<Coordinates> GetBishopMoves(Coordinates coordinates)
        {
            Piece[,] currentPosition = GetCurrentPosition();
            Piece piece = GetPieceFromPosition(coordinates, currentPosition);
            if (piece == null || piece.Type != PieceType.Bishop)
            { return new List<Coordinates>(); }

            List<Coordinates> result = new List<Coordinates>();
            (int, int)[] deltas = new (int, int)[] { (-1, 1), (-1, -1), (1, -1), (1, 1) };
            foreach ((int, int) delta in deltas)
            {
                Coordinates destination = coordinates;
                while (true)
                {
                    destination.X += delta.Item1;
                    destination.Y += delta.Item2;
                    if (!CheckCoordinates(destination))
                    { break; }
                    Piece other = GetPieceFromPosition(destination, currentPosition);
                    if ((other == null) || (other.Color != piece.Color))
                    { result.Add(destination); }
                    if (other != null)
                    { break; }
                }
            }
            return result.Where(delegate (Coordinates destination) 
            { return !CheckHangingKingMove(new Move() { Source = coordinates, Destination = destination }); }).ToList();
        }

        List<Coordinates> GetRookMoves(Coordinates coordinates)
        {
            Piece[,] currentPosition = GetCurrentPosition();
            Piece piece = GetPieceFromPosition(coordinates, currentPosition);
            if (piece == null || piece.Type != PieceType.Rook)
            { return new List<Coordinates>(); }

            List<Coordinates> result = new List<Coordinates>();
            (int, int)[] deltas = new (int, int)[] { (-1, 0), (1, 0), (0, -1), (0, 1) };
            foreach ((int, int) delta in deltas)
            {
                Coordinates destination = coordinates;
                while (true)
                {
                    destination.X += delta.Item1;
                    destination.Y += delta.Item2;
                    if (!CheckCoordinates(destination))
                    { break; }
                    Piece other = GetPieceFromPosition(destination, currentPosition);
                    if ((other == null) || (other.Color != piece.Color))
                    { result.Add(destination); }
                    if (other != null)
                    { break; }
                }
            }

            return result.Where(delegate (Coordinates destination) 
            { return !CheckHangingKingMove(new Move() { Source = coordinates, Destination = destination }); }).ToList();
        }

        List<Coordinates> GetQueenMoves(Coordinates coordinates)
        {
            Piece[,] currentPosition = GetCurrentPosition();
            Piece piece = GetPieceFromPosition(coordinates, currentPosition);
            if (piece == null || piece.Type != PieceType.Queen)
            { return new List<Coordinates>(); }

            List<Coordinates> result = new List<Coordinates>();
            (int, int)[] deltas = new (int, int)[] { (-1, 1), (-1, -1), (1, -1), (1, 1), (-1, 0), (1, 0), (0, -1), (0, 1) };
            foreach ((int, int) delta in deltas)
            {
                Coordinates destination = coordinates;
                while (true)
                {
                    destination.X += delta.Item1;
                    destination.Y += delta.Item2;
                    if (!CheckCoordinates(destination))
                    { break; }
                    Piece other = GetPieceFromPosition(destination, currentPosition);
                    if ((other == null) || (other.Color != piece.Color))
                    { result.Add(destination); }
                    if (other != null)
                    { break; }
                }
            }
            return result.Where(delegate (Coordinates destination) 
            { return !CheckHangingKingMove(new Move() { Source = coordinates, Destination = destination }); }).ToList();
        } 
        
        List<Coordinates> GetKingMoves(Coordinates coordinates)
        {
            Piece[,] currentPosition = GetCurrentPosition();
            Piece piece = GetPieceFromPosition(coordinates, currentPosition);
            if (piece == null || piece.Type != PieceType.King)
            { return new List<Coordinates>(); }

            PlayerColor oppositeColor = (piece.Color == PlayerColor.White) ? PlayerColor.Black : PlayerColor.White;
            List<Coordinates> result = new List<Coordinates>();
            (int, int)[] deltas = new (int, int)[] { (-1, 1), (-1, -1), (1, -1), (1, 1), (-1, 0), (1, 0), (0, -1), (0, 1) };
            foreach ((int, int) delta in deltas)
            {
                Coordinates destination = coordinates;
                destination.X += delta.Item1;
                destination.Y += delta.Item2;
                if (!CheckCoordinates(destination))
                { continue; }
                Piece other = GetPieceFromPosition(destination, currentPosition);
                if ((other == null) || (other.Color != piece.Color)) 
                { result.Add(destination); }
            }

            // Handle castling
            Coordinates initialPosition;
            Coordinates initialRookShortCastlePosition;
            Coordinates initialRookLongCastlePosition;
            if (piece.Color == PlayerColor.Black)
            {
                initialPosition = new Coordinates() { X = 0, Y = 4 };
                initialRookShortCastlePosition = new Coordinates() { X = 0, Y = 7 };
                initialRookLongCastlePosition = new Coordinates() { X = 0, Y = 0 };
            }
            else
            {
                initialPosition = new Coordinates() { X = 7, Y = 4 };
                initialRookShortCastlePosition = new Coordinates() { X = 7, Y = 7 };
                initialRookLongCastlePosition = new Coordinates() { X = 7, Y = 0 };
            }

            bool isKingUnderAttack = GetAttackers(coordinates, oppositeColor).Count > 0;

            bool canShortCastle = !isKingUnderAttack && !Chessboard.History.Any(delegate (Move move) 
            { return initialPosition == move.Source || initialRookShortCastlePosition == move.Source; });
            if (canShortCastle)
            {
                Piece shortCastleRook = GetPieceFromPosition(initialRookShortCastlePosition, currentPosition);
                bool shortCastlePathEmpty = 
                    GetPieceFromPosition(new Coordinates() { X = coordinates.X, Y = coordinates.Y + 1 }, currentPosition) == null &&
                    GetPieceFromPosition(new Coordinates() { X = coordinates.X, Y = coordinates.Y + 2 }, currentPosition) == null;
                canShortCastle = shortCastlePathEmpty && GetAttackers(new Coordinates() { X = coordinates.X, Y = coordinates.Y + 1 }, oppositeColor).Count == 0;
                if ((shortCastleRook == null) || (shortCastleRook.Type != PieceType.Rook) || (shortCastleRook.Color != piece.Color))
                { canShortCastle = false; }
            }

            if (canShortCastle)
            { result.Add(new Coordinates() { X = coordinates.X, Y = coordinates.Y + 2 }); }
            
            bool canLongCastle = !isKingUnderAttack && !Chessboard.History.Any(delegate (Move move) 
            { return initialPosition == move.Source || initialRookLongCastlePosition == move.Source; });
            if (canLongCastle)
            {
                Piece longCastleRook = GetPieceFromPosition(initialRookLongCastlePosition, currentPosition);
                bool longCastlePathEmpty = 
                    GetPieceFromPosition(new Coordinates() { X = coordinates.X, Y = coordinates.Y - 1 }, currentPosition) == null &&
                    GetPieceFromPosition(new Coordinates() { X = coordinates.X, Y = coordinates.Y - 2 }, currentPosition) == null;
                canLongCastle = longCastlePathEmpty && GetAttackers(new Coordinates() { X = coordinates.X, Y = coordinates.Y - 1 }, oppositeColor).Count == 0;
                if ((longCastleRook == null) || (longCastleRook.Type != PieceType.Rook) || (longCastleRook.Color != piece.Color))
                { canLongCastle = false; }
            } 

            if (canLongCastle)
            { result.Add(new Coordinates() { X = coordinates.X, Y = coordinates.Y - 2 }); } 

            return result.Where(delegate (Coordinates destination) 
            { return !CheckHangingKingMove(new Move() { Source = coordinates, Destination = destination }); }).ToList();
        }

        Coordinates? GetNextPlayerKing()
        {
            Piece[,] currentPosition = GetCurrentPosition();
            for (int rank = 0; rank < 8; ++rank)
            {
                for (int file = 0; file < 8; ++file)
                {
                    Coordinates coordinates = new Coordinates() { X = rank, Y = file };
                    Piece piece = GetPieceFromPosition(coordinates, currentPosition);
                    if (piece == null)
                        continue;
                    if ((piece.Type == PieceType.King) && (piece.Color != CurrentPlayer))
                    { return coordinates; }
                }
            }
            return null;
        }

        Coordinates? GetCurrentPlayerKing()
        {
            Piece[,] currentPosition = GetCurrentPosition();
            for (int rank = 0; rank < 8; ++rank)
            {
                for (int file = 0; file < 8; ++file)
                {
                    Coordinates coordinates = new Coordinates() { X = rank, Y = file };
                    Piece piece = GetPieceFromPosition(coordinates, currentPosition);
                    if (piece == null)
                        continue;
                    if ((piece.Type == PieceType.King) && (piece.Color == CurrentPlayer))
                    { return coordinates; }
                }
            }
            return null;
        }

        bool CheckKingHanging()
        {
            Coordinates? optionalCoordinates = GetNextPlayerKing();
            if (!optionalCoordinates.HasValue)
                return true;
            return GetAttackers(optionalCoordinates.Value, CurrentPlayer).Count > 0;
        }

        bool CheckHangingKingMove(Move move)
        {
            ChessboardService sandbox = new ChessboardService(new ChessboardModel() { History = new List<Move>(Chessboard.History) }, CurrentPlayer, CurrentPromotion);
            sandbox.Chessboard.History.Add(move);
            sandbox.CurrentPlayer = (CurrentPlayer == PlayerColor.White) ? PlayerColor.Black : PlayerColor.White;
            return sandbox.CheckKingHanging();
        }

        bool AnyCurrentPlayerMoves()
        {
            Piece[,] currentPosition = GetCurrentPosition();
            for (int rank = 0; rank < 8; ++rank)
            {
                for (int file = 0; file < 8; ++file)
                {
                    Coordinates coordinates = new Coordinates() { X = rank, Y = file };
                    Piece piece = GetPieceFromPosition(coordinates, currentPosition);
                    if ((piece != null) && (piece.Color == CurrentPlayer) && GetMoves(coordinates).Count > 0)
                    { return true; }
                }
            }
            return false;
        }
    }
}