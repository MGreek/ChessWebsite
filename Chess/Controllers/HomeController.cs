using Chess.Models;
using Chess.Services;
using System;
using System.Collections.Generic;
using System.Web.Mvc;

namespace Chess.Controllers
{
    public class HomeController : Controller
    {
        static PromotionPieceType CurrentPromotion = PromotionPieceType.Queen;
        static ChessboardService ChessboardService { get; set; } = 
            new ChessboardService(new ChessboardModel(), PlayerColor.White, CurrentPromotion);
        static List<Coordinates> SelectedCoordinates { get; } = new List<Coordinates>();

        public ActionResult Index()
        {
            ChessboardService.CurrentPromotion = CurrentPromotion;
            ViewBag.Chessboard = ChessboardService;
            ViewBag.SelectedCoordinates = SelectedCoordinates;
            return View("Index");
        }

        public ActionResult TileClick(string strCoordinates)
        {
            Coordinates coordinates = new Coordinates() 
            { X = int.Parse(strCoordinates.Split(';')[0]), Y = int.Parse(strCoordinates.Split(';')[1]) };
            Piece piece = ChessboardService.GetPiece(coordinates);
            if (SelectedCoordinates.Contains(coordinates))
            {
                if (SelectedCoordinates[0] != coordinates)
                { ChessboardService.ApplyMove(new Move() { Source = SelectedCoordinates[0], Destination = coordinates }); }
                SelectedCoordinates.Clear();
            }
            else if (piece != null)
            {
                if (piece.Color == ChessboardService.CurrentPlayer)
                {
                    SelectedCoordinates.Clear();
                    SelectedCoordinates.Add(coordinates);
                    foreach (Coordinates moveCoordinates in ChessboardService.GetMoves(coordinates)) 
                    { SelectedCoordinates.Add(moveCoordinates); }
                }
            }
            else
            { SelectedCoordinates.Clear(); }

            ChessboardService.CurrentPromotion = CurrentPromotion;
            ViewBag.Chessboard = ChessboardService;
            ViewBag.SelectedCoordinates = SelectedCoordinates;
            return View("Index");
        }
        
        public ActionResult ResetClick()
        {
            CurrentPromotion = PromotionPieceType.Queen;
            ChessboardService = new ChessboardService(new ChessboardModel(), PlayerColor.White, CurrentPromotion);
            SelectedCoordinates.Clear();

            ChessboardService.CurrentPromotion = CurrentPromotion;
            ViewBag.Chessboard = ChessboardService;
            ViewBag.SelectedCoordinates = SelectedCoordinates;
            return View("Index");
        }

        PromotionPieceType GetPromotionTypeFromString(string promotion)
        {
            switch (promotion)
            {
                case "queen":
                    return PromotionPieceType.Queen;
                case "rook":
                    return PromotionPieceType.Rook;
                case "bishop":
                    return PromotionPieceType.Bishop;
                case "knight":
                    return PromotionPieceType.Knight;
                default:
                    throw new InvalidOperationException();
            }
        }

        public ActionResult PromotionPieceChanged(string promotion)
        {
            if (GetPromotionTypeFromString(promotion) == CurrentPromotion)
            {
                ViewBag.Chessboard = ChessboardService;
                ViewBag.SelectedCoordinates = SelectedCoordinates;
                return View("Index");
            }
            CurrentPromotion = GetPromotionTypeFromString(promotion);

            ChessboardService.CurrentPromotion = CurrentPromotion;
            ViewBag.Chessboard = ChessboardService;
            ViewBag.SelectedCoordinates = SelectedCoordinates;
            return View("Index");
        }

        public ActionResult UndoLastMove()
        {
            ChessboardService.UndoLastMove();

            ChessboardService.CurrentPromotion = CurrentPromotion;
            ViewBag.Chessboard = ChessboardService;
            SelectedCoordinates.Clear();
            return View("Index");
        }
    }
}