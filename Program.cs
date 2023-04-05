using Raylib_cs;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Numerics;
using System.Runtime.CompilerServices;
using Color = Raylib_cs.Color;

namespace Chekkers
{

    static class Program
    {
        public static int beatingCounter;
        public static void prepareBoard(int[,] board)
        {
            //0 - pusta plansza , 1 - pion bialy, 2 - pion czarny , 3 - damka biala , 4 - damka czarna
            for (int i = 0; i < board.GetLength(0); i++)
            {
                for (int j = 0; j < board.GetLength(1); j++)
                {
                    if (i <= 2 && i % 2 == 0 && j % 2 == 1)
                        board[i, j] = 2;//ustawiam czarne pionki

                    else if (i <= 2 && i % 2 == 1 && j % 2 == 0)
                        board[i, j] = 2;//ustawiam czarne pionki

                    else if (i >= 5 && i % 2 == 0 && j % 2 == 1)
                        board[i, j] = 1;//ustawiam biale pionki

                    else if (i >= 5 && i % 2 == 1 && j % 2 == 0)
                        board[i, j] = 1;//ustawiam biale pionki
                    else
                        board[i, j] = 0;
                }
            }
        }
        public static void drawBoard(int[,] board, int cellSize)
        {
            for (int i = 0; i < board.GetLength(0); i++)
            {
                for (int j = 0; j < board.GetLength(1); j++)
                {
                    if(i%2 == 0)
                    {
                        if(j%2==0)
                            Raylib.DrawRectangle(j * cellSize, i * cellSize, cellSize,cellSize, Color.GRAY);
                        else
                            Raylib.DrawRectangle(j * cellSize, i * cellSize, cellSize,cellSize, Color.BROWN);
                    }
                    if (i % 2 == 1)
                    {
                        if (j % 2 == 0)
                            Raylib.DrawRectangle(j * cellSize, i * cellSize, cellSize, cellSize, Color.BROWN);
                        else
                            Raylib.DrawRectangle(j * cellSize, i * cellSize, cellSize, cellSize, Color.GRAY);
                    }
                }
            }
        }
        public static void drawPieces(int[,] board, int cellSize)
        {
            for (int i = 0; i < board.GetLength(0); i++)
            {
                for (int j = 0; j < board.GetLength(1); j++)
                {
                    if (board[i, j] == 1)//rysuj pionki biale
                        Raylib.DrawCircle(j * cellSize + cellSize / 2, i * cellSize + cellSize / 2, cellSize / 2, Color.WHITE);
                    else if(board[i, j] == 2)//rysuj pionki czarne
                        Raylib.DrawCircle(j * cellSize + cellSize / 2, i * cellSize + cellSize / 2, cellSize / 2, Color.BLACK);
                    //TODO - DAMKI
                }
            }
        }
        public static Tuple<int,int> getCell(Vector2 mousePosition, int cellSize)
        {
            return Tuple.Create((int)mousePosition.Y/cellSize, (int)mousePosition.X/cellSize);
        }

        public static bool beatingAvaliable(Tuple<int, int> pieceClicked, int [,] board, int opponent)
        {
            int x = pieceClicked.Item2;
            int y = pieceClicked.Item1;
            if (x - 2 >= 0 && y - 2 >= 0 && board[y - 2, x - 2] == 0 && opponent == board[y - 1, x - 1])//czy nie wychodzimy poza tablice !!!! BOARD != dziala dopoki nie ma damek na planszy!!
                return true;
            if (x - 2 >=0 && y + 2 < board.GetLength(0) && board[y + 2, x - 2] == 0 && opponent == board[y + 1, x - 1])
                    return true;
            if (x + 2 < board.GetLength(0) && y - 2 >= 0 && board[y - 2, x + 2] == 0 && opponent == board[y - 1, x + 1])//czy nie wychodzimy poza tablice !!!! BOARD != dziala dopoki nie ma damek na planszy!!
                return true;
            if (x + 2 < board.GetLength(1) && y + 2 < board.GetLength(0) && board[y + 2, x + 2] == 0 && opponent == board[y + 1, x + 1])
                    return true;

            return false;
        }
        public static List<Tuple<int, int>> getBeatings(Tuple<int, int> pieceClicked, int[,] board, int opponent)
        {
            int x = pieceClicked.Item2;
            int y = pieceClicked.Item1;
            List<Tuple<int, int>> beatings = new List<Tuple<int, int>>();
            if (x - 2 >= 0 && y - 2 >= 0 && board[y - 2, x - 2] == 0 && opponent == board[y - 1, x - 1])//czy nie wychodzimy poza tablice !!!! BOARD != dziala dopoki nie ma damek na planszy!!
            {
                Tuple<int, int> legalBeating = new Tuple<int, int>(y - 2, x - 2);
                beatings.Add(legalBeating);
            }

            if (x - 2 >= 0 && y + 2 < board.GetLength(0) && board[y + 2, x - 2] == 0 && opponent == board[y + 1, x - 1])
            {
                Tuple<int, int> legalBeating = new Tuple<int, int>(y + 2, x - 2);
                beatings.Add(legalBeating);
            }
            if (x + 2 < board.GetLength(0) && y - 2 >= 0 && board[y - 2, x + 2] == 0 && opponent == board[y - 1, x + 1])//czy nie wychodzimy poza tablice !!!! BOARD != dziala dopoki nie ma damek na planszy!!
            {
                Tuple<int, int> legalBeating = new Tuple<int, int>(y - 2, x + 2);
                beatings.Add(legalBeating);
            }
            if (x + 2 < board.GetLength(0) && y + 2 < board.GetLength(1) && board[y + 2, x + 2] == 0 && opponent == board[y + 1, x + 1])
            {
                Tuple<int, int> legalBeating = new Tuple<int, int>(y + 2, x + 2);
                beatings.Add(legalBeating);
            }
            return beatings;
        }
        public static void drawLegalMoves(List<Tuple<int, int>> moves ,  int[,] board,int cellSize)
        {
            //obsluga bicia
            foreach (var move in moves)
            {
                Raylib.DrawRectangle((move.Item2) * cellSize, (move.Item1) * cellSize, cellSize, cellSize, Color.GREEN);
            }
            /*
            if (board[y,x] == 1)
            {
                //obsluga ruchow bialego pionka
                
                if (x-1>=0 && y-1>=0)//czy nie wychodzimy poza tablice
                {
                    if(board[y - 1, x - 1] == 0)//sprawdzam, czy pole na ukos w lewo jest wolne
                        Raylib.DrawRectangle((x-1) * cellSize, (y-1) * cellSize, cellSize, cellSize, Color.GREEN);
                }
                if (x+1 <board.GetLength(1) && y-1>=0)//czy nie wychodzimy poza tablice
                {
                    if(board[y - 1, x + 1] == 0)//sprawdzam, czy pole na ukos w prawo jest wolne
                        Raylib.DrawRectangle((x+1) * cellSize, (y-1) * cellSize, cellSize, cellSize, Color.GREEN);
                }
                
            }
            else if (board[y, x] == 2)
            {
                //obsluga ruchow czarnego pionka
                if (x - 1 >= 0 && y + 1 < board.GetLength(0))//czy nie wychodzimy poza tablice
                {
                    if (board[y + 1, x - 1] == 0)//sprawdzam, czy pole na ukos w lewo jest wolne
                        Raylib.DrawRectangle((x - 1) * cellSize, (y + 1) * cellSize, cellSize, cellSize, Color.GREEN);
                }
                if (x + 1 < board.GetLength(1) && y + 1 >= 0)//czy nie wychodzimy poza tablice
                {
                    if (board[y + 1, x + 1] == 0)//sprawdzam, czy pole na ukos w prawo jest wolne
                        Raylib.DrawRectangle((x + 1) * cellSize, (y + 1) * cellSize, cellSize, cellSize, Color.GREEN);
                }
            }
            else if (board[y, x] == 3)
            {
                //obsluga ruchow bialej damki
            }
            else if (board[y, x] == 4)
            {
                //obsluga ruchow czarnej damki
            }
            //System.Console.WriteLine("Possible moves: ");

            */
        }
        public static void movePiece(int y, int x, int yDestination, int xDestination, int[,] board)
        { 
            
           
            //obsluga ruchu bialych

            System.Console.WriteLine("Destination: " + yDestination + " " + xDestination);
            if (board[y, x] == 1)
            { 
                board[y, x] = 0;
                board[yDestination, xDestination] = 1;
            }
            //obsluga ruchu czarnych

            if (board[y, x] == 2)
            {
                board[y, x] = 0;
                board[yDestination, xDestination] = 2;
            }
        }
        public static void removePiece(Tuple<int, int> pieceClicked, Tuple<int, int> cellClicked, int[,] board)
        {
            board[(pieceClicked.Item1 + cellClicked.Item1) / 2, (pieceClicked.Item2 + cellClicked.Item2) / 2] = 0;
        }
        public static List<Tuple<int,int>> getLegalMoves(Tuple<int,int>pieceClicked, int[,] board)
        {
            List <Tuple< int,int>> moves = new List<Tuple<int, int>>();
            int y = pieceClicked.Item1;
            int x = pieceClicked.Item2;
            int i = 0;
            if (board[y, x] == 1)
            {
                //obsluga bicia bialego
                if (beatingAvaliable(pieceClicked, board, 2))
                {
                    beatingCounter = 1;
                    moves = getBeatings(pieceClicked, board, 2);
                    Console.WriteLine("test");
                }
                //obsluga ruchow bialego pionka - tylko jesli nie ma bicia (dlatego else)

                else
                {
                    if (x - 1 >= 0 && y - 1 >= 0)//czy nie wychodzimy poza tablice
                    {
                        if (board[y - 1, x - 1] == 0)//sprawdzam, czy pole na ukos w lewo jest wolne
                        {
                            Tuple<int, int> legalMove = new Tuple<int, int>(y - 1, x - 1);
                            moves.Insert(i, legalMove);
                            i++;
                        }

                    }
                    if (x + 1 < board.GetLength(1) && y - 1 >= 0)//czy nie wychodzimy poza tablice
                    {
                        if (board[y - 1, x + 1] == 0)//sprawdzam, czy pole na ukos w prawo jest wolne
                        {
                            Tuple<int, int> legalMove = new Tuple<int, int>(y - 1, x + 1);
                            moves.Insert(i, legalMove);
                            i++;
                        }
                    }
                }

            }
            else if (board[y, x] == 2)
            {
                if (beatingAvaliable(pieceClicked, board, 1))
                {
                    beatingCounter = 1;
                    moves = getBeatings(pieceClicked, board, 1);
                    Console.WriteLine("test");
                }
                //obsluga ruchow czarnego pionka
                else
                {
                    if (x - 1 >= 0 && y + 1 < board.GetLength(0))//czy nie wychodzimy poza tablice
                    {
                        if (board[y + 1, x - 1] == 0)//sprawdzam, czy pole na ukos w lewo jest wolne
                        {
                            Tuple<int, int> legalMove = new Tuple<int, int>(y + 1, x - 1);
                            moves.Insert(i, legalMove);
                            i++;
                        }

                    }
                    if (x + 1 < board.GetLength(1) && y + 1 >= 0)//czy nie wychodzimy poza tablice
                    {
                        if (board[y + 1, x + 1] == 0)//sprawdzam, czy pole na ukos w prawo jest wolne
                        {
                            Tuple<int, int> legalMove = new Tuple<int, int>(y + 1, x + 1);
                            moves.Insert(i, legalMove);
                            i++;
                        }
                    }
                }
            }
            else if (board[y, x] == 3)
            {
                //obsluga ruchow bialej damki
            }
            else if (board[y, x] == 4)
            {
                //obsluga ruchow czarnej damki
            }
            //System.Console.WriteLine("Possible moves: ");
            foreach (var move in moves)
            {
                Console.WriteLine(move);
            }
            return moves;
        }
        public static void Main()
        {
            int[,] board = new int[8, 8];
            int cellSize = 100;
            bool gameInProgress = false;
            bool turn = true;//true - tura bialych , false - tura czarnych
            Vector2 mousePosition = new Vector2(0, 0);
            Tuple<int, int> cellClicked;
            Tuple<int, int> pieceClicked = new Tuple<int,int> (-1,-1);
            List <Tuple<int,int>> moves = new List<Tuple<int, int>>();
            bool pieceChoosen = false;
            prepareBoard(board);
            gameInProgress = false ;
            int playerColor = 1;//1 - biali, 2 - czarni
            int whitePieces = 12;
            int blackPieces = 12;

            Raylib.InitWindow(cellSize*board.GetLength(1)+200, cellSize * board.GetLength(0), "Chekkers");
            Raylib.SetTargetFPS(60);

            while (!Raylib.WindowShouldClose())
            {
                
                mousePosition = Raylib.GetMousePosition();
                Raylib.BeginDrawing();
                Raylib.ClearBackground(Color.BLUE);
                if (blackPieces == 0 || whitePieces == 0)
                    gameInProgress = false;
                if (!gameInProgress)
                {
                    if (blackPieces == 0)
                    {
                        Raylib.DrawText("WHITE WON! ", 0, 100, 20, Color.WHITE);
                        
                    }
                    if (whitePieces == 0)
                    {
                        Raylib.DrawText("BLACK WON! ", 0, 100, 20, Color.BLACK);
                        
                    }
                    Raylib.DrawText("Press ENTER to Start New Game or ESC to Exit",0, 0, 20, Color.WHITE);
                    if (Raylib.IsKeyPressed(KeyboardKey.KEY_ENTER))
                    {
                        gameInProgress = true;
                        whitePieces = 12; blackPieces = 12;
                        prepareBoard(board);
                        playerColor = 1;
                        turn = true;
                    }
                }
                else
                {
                    drawBoard(board, cellSize);
                    drawPieces(board, cellSize);
                    if (turn)
                        Raylib.DrawText("White turn", Raylib.GetScreenWidth() - 150, 0, 20, Color.WHITE);
                    else
                        Raylib.DrawText("Black turn", Raylib.GetScreenWidth() - 150, 0, 20, Color.BLACK);
                    if (gameInProgress)
                    {
                        if(pieceChoosen)
                        {
                            drawLegalMoves(moves, board, cellSize);
                            
                        }
                            
                        if (Raylib.IsMouseButtonPressed(MouseButton.MOUSE_BUTTON_LEFT))
                        {
                            cellClicked = getCell(mousePosition, cellSize);
                            System.Console.WriteLine("Cell clicked: " + cellClicked.Item1 + " " + cellClicked.Item2);
                            Raylib.DrawCircleV(mousePosition, cellSize / 2, Color.RED);
                            if (pieceChoosen && moves.Contains(cellClicked))//wykonaj ruch
                            {
                                if(Math.Abs(cellClicked.Item1 - pieceClicked.Item1)>1 && Math.Abs(cellClicked.Item2 - pieceClicked.Item2) > 1) //mamy bicie
                                {
                                    removePiece(pieceClicked, cellClicked, board);
                                    if (turn)
                                        blackPieces--;
                                    else
                                        whitePieces--;
                                    System.Console.WriteLine("pieces remaining: W" + whitePieces + " B" + blackPieces);
                                }
                                movePiece(pieceClicked.Item1, pieceClicked.Item2, cellClicked.Item1, cellClicked.Item2, board);
                                turn = !turn;//zmiana tury
                                if (playerColor == 1)
                                    playerColor = 2;
                                else playerColor = 1;
                                pieceChoosen = false;

                                
                            }
                            if (board[cellClicked.Item1, cellClicked.Item2] == playerColor)
                            {
                                pieceClicked = cellClicked;
                                pieceChoosen = true;
                                //if dany pionek in listaPionkowZOptymalnymiBiciami or listaPionkowZOptymalnymiBiciami is empty, w przeciwnym wypadku nie rysuj ruchow (mimo ze są) bo nie są legalne (nie są optymalne)
                                moves = getLegalMoves(pieceClicked, board);
                                drawLegalMoves(moves, board, cellSize);
                                
                            }
                        }

                    }
                    /* Warunki konca:
                     * - jesli gracz A nie ma ruchu - wygrywa gracz B
                     * - jesli gracz B nie ma pionkow - wygrywa gracz A
                     * - jesli nastapiło 15 ruchów damką ze strony jednego z graczy - remis
                     * - jesli 3x pojawilo sie to samo ustawienie na planszy - remis -> zastanowic sie czy wprowadzac - ciezkie do zaimplementowania
                     * 
                     * TODO:
                     * - wielokrotne bicia
                     * - optymalne ruchy (nie dawać zrobić ruchu jesli jest lepszy)
                     * - dodatkowy warunek konca - brak ruchow 
                     * - wypisywanie kto wygral i dlaczego (+ w tle drukowanie planszy zeby bylo wiadomo)
                     * - AI
                     * - OOP 
                     * - MENU
                     * - WYBÓR OPCJI GRY
                     */
                }
                Raylib.EndDrawing();
            }

            Raylib.CloseWindow();
        }
    }
}