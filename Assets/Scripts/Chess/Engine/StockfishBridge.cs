using System;
using System.Diagnostics;
using System.IO;
using UnityEngine;

namespace MMBGame
{
    public class StockfishBridge : MonoBehaviour
    {
        private Process stockfishProcess;
        private StreamWriter input;
        private StreamReader output;
        private bool isReady;

        private const int ReadTimeoutMs = 3000;

        public void Initialize()
        {
            string exePath = Path.Combine(Application.streamingAssetsPath, "Engines", "stockfish.exe");
            if (!File.Exists(exePath))
            {
                UnityEngine.Debug.LogWarning("StockfishBridge: stockfish.exe not found at " + exePath);
                return;
            }

            stockfishProcess = new Process();
            stockfishProcess.StartInfo.FileName = exePath;
            stockfishProcess.StartInfo.UseShellExecute = false;
            stockfishProcess.StartInfo.RedirectStandardInput = true;
            stockfishProcess.StartInfo.RedirectStandardOutput = true;
            stockfishProcess.StartInfo.CreateNoWindow = true;
            stockfishProcess.Start();

            input = stockfishProcess.StandardInput;
            output = stockfishProcess.StandardOutput;

            Send("uci");
            WaitFor("uciok");
            Send("isready");
            WaitFor("readyok");
            isReady = true;
        }

        public Move GetBestMove(BoardState state, int thinkTimeMs = 500)
        {
            if (!isReady) return null;

            string fen = FenConverter.ToFen(state);
            Send("position fen " + fen);
            Send("go movetime " + thinkTimeMs);

            string bestMoveLine = WaitFor("bestmove");
            if (bestMoveLine == null) return null;

            return ParseBestMove(bestMoveLine, state);
        }

        public void Shutdown()
        {
            if (stockfishProcess == null || stockfishProcess.HasExited) return;
            Send("quit");
            stockfishProcess.WaitForExit(1000);
            if (!stockfishProcess.HasExited) stockfishProcess.Kill();
            stockfishProcess.Dispose();
            stockfishProcess = null;
            isReady = false;
        }

        private void OnDestroy()
        {
            Shutdown();
        }

        private void Send(string command)
        {
            input?.WriteLine(command);
            input?.Flush();
        }

        private string WaitFor(string keyword)
        {
            DateTime deadline = DateTime.Now.AddMilliseconds(ReadTimeoutMs);
            while (DateTime.Now < deadline)
            {
                string line = output.ReadLine();
                if (line != null && line.StartsWith(keyword))
                    return line;
            }
            return null;
        }

        private Move ParseBestMove(string bestMoveLine, BoardState state)
        {
            string[] parts = bestMoveLine.Split(' ');
            if (parts.Length < 2) return null;

            string uci = parts[1];
            if (uci == "(none)" || uci.Length < 4) return null;

            int fromFile = uci[0] - 'a';
            int fromRank = uci[1] - '1';
            int toFile   = uci[2] - 'a';
            int toRank   = uci[3] - '1';

            if (uci.Length == 5)
            {
                PieceType promo = CharToPromoType(uci[4]);
                return new Move(fromFile, fromRank, toFile, toRank, SpecialMoveType.Promotion, promo);
            }

            ChessPiece piece = state.GetPiece(fromFile, fromRank);
            if (piece != null && piece.type == PieceType.King && System.Math.Abs(toFile - fromFile) == 2)
            {
                SpecialMoveType castle = toFile > fromFile
                    ? SpecialMoveType.CastleKingside
                    : SpecialMoveType.CastleQueenside;
                return new Move(fromFile, fromRank, toFile, toRank, castle);
            }

            if (piece != null && piece.type == PieceType.Pawn
                && state.enPassantAvailable
                && toFile == state.enPassantFile && toRank == state.enPassantRank)
            {
                return new Move(fromFile, fromRank, toFile, toRank, SpecialMoveType.EnPassant);
            }

            return new Move(fromFile, fromRank, toFile, toRank);
        }

        private PieceType CharToPromoType(char c)
        {
            switch (c)
            {
                case 'r': return PieceType.Rook;
                case 'b': return PieceType.Bishop;
                case 'n': return PieceType.Knight;
                default:  return PieceType.Queen;
            }
        }
    }
}
