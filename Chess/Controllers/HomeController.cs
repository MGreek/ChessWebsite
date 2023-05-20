using Chess.Models;
using Chess.Services;
using System.Collections.Generic;
using System.Web.Mvc;

namespace Chess.Controllers
{
    public class HomeController : Controller
    {
        static ChessboardService ChessboardService { get; } = 
            new ChessboardService(new ChessboardModel(), PlayerColor.White, PromotionPieceType.Queen);
        static List<Coordinates> SelectedCoordinates { get; } = new List<Coordinates>();
        public ActionResult Index()
        {
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
            ViewBag.Chessboard = ChessboardService;
            ViewBag.SelectedCoordinates = SelectedCoordinates;
            return View("Index");
        }
    }
}