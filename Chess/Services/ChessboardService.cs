using Chess.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Chess.Services
{
    public enum MatchState
    {
        BlackWin,
        WhiteWin,
        Stalemate,
        None
    }

    public partial class ChessboardService
    {
        ChessboardModel Chessboard { get; }
        public PlayerColor CurrentPlayer { get; private set; }
        public PromotionPieceType CurrentPromotion { get; set; }

        public ChessboardService(ChessboardModel chessboard, PlayerColor currentPlayer, PromotionPieceType currentPromotion)
        {
            Chessboard = chessboard;
            CurrentPlayer = currentPlayer;
            CurrentPromotion = currentPromotion;
        } 

        public string GetPieceImgPath(int rank, int file)
        {
            Piece piece = GetPieceFromPosition(new Coordinates() { X = rank, Y = file }, GetCurrentPosition());
            if (piece == null)
            { return string.Empty; }

            switch (piece.Type)
            {
                case PieceType.King:
                    return piece.Color == PlayerColor.White ? "/src/img/WhiteKing.png" : "/src/img/BlackKing.png";
                case PieceType.Queen:
                    return piece.Color == PlayerColor.White ? "/src/img/WhiteQueen.png" : "/src/img/BlackQueen.png";
                case PieceType.Rook:
                    return piece.Color == PlayerColor.White ? "/src/img/WhiteRook.png" : "/src/img/BlackRook.png";
                case PieceType.Bishop:
                    return piece.Color == PlayerColor.White ? "/src/img/WhiteBishop.png" : "/src/img/BlackBishop.png";
                case PieceType.Knight:
                    return piece.Color == PlayerColor.White ? "/src/img/WhiteKnight.png" : "/src/img/BlackKnight.png";
                case PieceType.Pawn:
                    return piece.Color == PlayerColor.White ? "/src/img/WhitePawn.png" : "/src/img/BlackPawn.png";
                default:
                    throw new ArgumentException("Invalid piece type.");
            }
        }

        public Piece GetPiece(Coordinates coordinates)
        { return GetPieceFromPosition(coordinates, GetCurrentPosition()); }

        public List<Move> GetHistory()
        { return new List<Move>(Chessboard.History); }

        public Piece[,] GetCurrentPosition()
        {
            Piece[,] currentPosition = new Piece[8, 8];
            for (int X = 0; X < 8; ++X)
            {
                for (int Y = 0; Y < 8; ++Y)
                {
                    if (ChessboardModel.StartingPosition[X, Y] != null)
                    {
                        currentPosition[X, Y] = new Piece();
                        currentPosition[X, Y].Type = ChessboardModel.StartingPosition[X, Y].Type;
                        currentPosition[X, Y].Color = ChessboardModel.StartingPosition[X, Y].Color;
                    }
                }
            }
            foreach (Move move in Chessboard.History)
            { Move(move, currentPosition); }
            return currentPosition;
        }
        
        public List<Coordinates> GetMoves(Coordinates coordinates)
        {
            if (!CheckCoordinates(coordinates))
            { throw new InvalidOperationException(); }

            Piece[,] currentPosition = GetCurrentPosition();
            Piece piece = GetPieceFromPosition(coordinates, currentPosition);
            if (piece == null)
            { throw new InvalidOperationException(); }

            switch (piece.Type)
            {
                case PieceType.King:
                    return GetKingMoves(coordinates);
                case PieceType.Queen:
                    return GetQueenMoves(coordinates);
                case PieceType.Rook:
                    return GetRookMoves(coordinates);
                case PieceType.Bishop:
                    return GetBishopMoves(coordinates);
                case PieceType.Knight:
                    return GetKnightMoves(coordinates);
                case PieceType.Pawn:
                    return GetPawnMoves(coordinates);
                default:
                    throw new InvalidOperationException();
            }
        }

        public void ApplyMove(Move move)
        {
            if (!CheckCoordinates(move.Source) || !CheckCoordinates(move.Destination))
            { throw new InvalidOperationException(); }
            if (!GetMoves(move.Source).Contains(move.Destination))
            { throw new InvalidOperationException(); }
            Piece[,] currentPosition = GetCurrentPosition();
            Piece piece = GetPieceFromPosition(move.Source, currentPosition);
            if (piece.Color != CurrentPlayer)
            { throw new InvalidOperationException(); }

            move = Move(move, currentPosition);
            Chessboard.History.Add(move);
            CurrentPlayer = (CurrentPlayer == PlayerColor.White) ? PlayerColor.Black : PlayerColor.White;
        }

        public MatchState GetMatchState()
        {
            if (!AnyCurrentPlayerMoves())
            {
                Coordinates coordinates = GetCurrentPlayerKing().Value;
                PlayerColor opposite = (CurrentPlayer == PlayerColor.White) ? PlayerColor.Black : PlayerColor.White;
                return (GetAttackers(coordinates, opposite).Count == 0) ? 
                    MatchState.Stalemate : ((opposite == PlayerColor.Black) ? MatchState.BlackWin : MatchState.WhiteWin);
            }
            return MatchState.None;
        }

        public void UndoLastMove()
        {
            if (Chessboard.History.Count == 0)
            { return; }

            CurrentPlayer = (CurrentPlayer == PlayerColor.White) ? PlayerColor.Black : PlayerColor.White;

            if (Chessboard.History.Count >= 2)
            {
                Move last = Chessboard.History[Chessboard.History.Count - 1];
                Move prev = Chessboard.History[Chessboard.History.Count - 2];
                Move[] kingCastle = new Move[] { ChessboardModel.WhiteKingShortCastle, ChessboardModel.WhiteKingLongCastle,
                                                 ChessboardModel.BlackKingShortCastle, ChessboardModel.BlackKingLongCastle };
                Move[] rookCastle = new Move[] { ChessboardModel.WhiteRookShortCastle, ChessboardModel.WhiteRookLongCastle,
                                                 ChessboardModel.BlackRookShortCastle, ChessboardModel.BlackRookLongCastle };
                Piece king = GetPiece(last.Destination);
                if ((king == null) || (king.Type != PieceType.King))
                {
                    Chessboard.History.RemoveAt(Chessboard.History.Count - 1);
                    return;
                }
                Piece rook = GetPiece(prev.Destination);
                if ((rook == null) || (rook.Type != PieceType.Rook) || (rook.Color != king.Color))
                {
                    Chessboard.History.RemoveAt(Chessboard.History.Count - 1);
                    return;
                }
                if (kingCastle.Contains(last) && rookCastle.Contains(prev))
                {
                    Chessboard.History.RemoveRange(Chessboard.History.Count - 2, 2);
                    return;
                }
            }
            Chessboard.History.RemoveAt(Chessboard.History.Count - 1);
        }
    } 
}