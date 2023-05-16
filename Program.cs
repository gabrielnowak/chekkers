using Raylib_cs;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.Design;
using System.Drawing;
using System.Numerics;
using System.Runtime.CompilerServices;
using Color = Raylib_cs.Color;

namespace Chekkers
{
    enum players
    {
        WHITE,
        BLACK
    }
    class AI
    {
        public static int difficulty;
        public static players player;
        public static int getStrength(int[,] board, players player)//funkcja zwracajaca sile pojedynczego ruchu
        {
            /*
             * ZASADY LICZENIA SIŁY:
             * PION - 1 PKT
             * PION W STRUKTURZE (PRZYLEPIONY DO INNEGO PIONA TEGO SAMEGO KOLORU) - 1 PKT
             * PION W 4 I 5 KOLUMNIE/WIERSZU - 2 PKT
             * PION W 3 I 6 KOLUMNIE/WIERSZU - 1 PKT
             * PION NARAŻONY NA BICIE - -1PKT
             */
            int whiteStrength = 0;
            int blackStrength = 0;
            if(player == players.WHITE)
            {
                //liczenie pionów
                for(int i = 0; i < board.GetLength(0);i++)
                {
                    for (int j = 0; j < board.GetLength(1); j++)
                    {
                        if (board[i, j] == 1)
                        {
                            whiteStrength++;
                            if (i == 3 || i == 4 || j == 3 || j == 4)//doliczanie za centralne miejsca
                                whiteStrength += 2;
                            else if (i == 2 || i == 5 || j == 2 || j == 5)
                                whiteStrength++;
                        }
                        else if (board[i, j] == 2)
                        {
                            blackStrength++;
                            if (i == 3 || i == 4 || j == 3 || j == 4)//doliczanie za centralne miejsca
                                blackStrength += 2;
                            else if (i == 2 || i == 5 || j == 2 || j == 5)
                                blackStrength++;
                        }
                    }
                }
                return whiteStrength - blackStrength;
            }
            else
            {
                return blackStrength - whiteStrength;
            }
            
        }
    }
    static class Program
    {
        static List<Tuple<int, int>> beatings = new List<Tuple<int, int>>();//przechowuje liste bic dla konkretnego pionka
        static List<Tuple<int, int>> piecesToRemove = new List<Tuple<int, int>>();//dla konkretnego pola zwracana jest lista pol do wykasowania przy biciu na to pole (wszystkie pionki napotkane po drodze)
        public static int maxBeatings=0;

        //public static int beatingCounter;
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
                    else if (board[i, j] == 2)//rysuj pionki czarne
                        Raylib.DrawCircle(j * cellSize + cellSize / 2, i * cellSize + cellSize / 2, cellSize / 2, Color.BLACK);
                    //TODO - DAMKI
                    else if (board[i, j] == 3)//damka biala
                    {
                        Raylib.DrawCircle(j * cellSize + cellSize / 2, i * cellSize + cellSize / 2, cellSize / 2, Color.GOLD);
                        Raylib.DrawCircle(j * cellSize + cellSize / 2, i * cellSize + cellSize / 2, cellSize / 3, Color.WHITE);
                    }
                    else if (board[i, j] == 4)//damka czarna
                    {
                        Raylib.DrawCircle(j * cellSize + cellSize / 2, i * cellSize + cellSize / 2, cellSize / 2, Color.GOLD);
                        Raylib.DrawCircle(j * cellSize + cellSize / 2, i * cellSize + cellSize / 2, cellSize / 3, Color.BLACK);
                    }
                    
                        

                }
            }
        }
        public static Tuple<int,int> getCell(Vector2 mousePosition, int cellSize)
        {
            return Tuple.Create((int)mousePosition.Y/cellSize, (int)mousePosition.X/cellSize);
        }

        public static void searchPieces(Tuple<int, int> start, Tuple<int, int> destination, Tuple<int, int> prevPos, int[,] board, int opponent)
        {
            
            int x = start.Item2;
            int y = start.Item1;
            Console.WriteLine("tttt: " + x + " " + y + " | " + destination.Item2 + " " + destination.Item1+" | "+ piecesToRemove.Count +" "+maxBeatings);
            if (x == destination.Item2 && y == destination.Item1 && piecesToRemove.Count == maxBeatings)
            {
                Console.WriteLine("tescik  "+piecesToRemove.Count);
                foreach (var piece in piecesToRemove)
                {
                    board[piece.Item1, piece.Item2] = 0;
                }
            }
            if (x - 2 >= 0 && y - 2 >= 0 && board[y - 2, x - 2] == 0 && opponent == board[y - 1, x - 1] && (prevPos.Item2 != x - 2 || prevPos.Item1 != y - 2))//czy nie wychodzimy poza tablice !!!! BOARD != dziala dopoki nie ma damek na planszy!!
            {
                Tuple<int, int> pieceToRemove = new Tuple<int, int>(y - 1, x - 1);
                piecesToRemove.Add(pieceToRemove);
                Console.WriteLine("xxxxxxxxxxxxxxxx1");
                Tuple<int, int> nextMove = new Tuple<int, int>(y - 2, x - 2);
                Console.WriteLine(nextMove.Item2+" "+nextMove.Item1);
                searchPieces(nextMove, destination, start, board, opponent);
                piecesToRemove.RemoveAt(piecesToRemove.Count - 1);
            }
            if (x - 2 >= 0 && y + 2 < board.GetLength(0) && board[y + 2, x - 2] == 0  && opponent == board[y + 1, x - 1] && (prevPos.Item2 != x - 2 || prevPos.Item1 != y + 2))
            {
                
                Tuple<int, int> pieceToRemove = new Tuple<int, int>(y + 1, x - 1);
                piecesToRemove.Add(pieceToRemove);
                Console.WriteLine("xxxxxxxxxxxxxxxx2");
                Tuple<int, int> nextMove = new Tuple<int, int>(y +2 , x - 2);
                searchPieces(nextMove, destination, start, board, opponent);
                piecesToRemove.RemoveAt(piecesToRemove.Count - 1);
            }
            if (x + 2 < board.GetLength(0) && y - 2 >= 0 && board[y - 2, x + 2] == 0 && opponent == board[y - 1, x + 1] && (prevPos.Item2 != x + 2 || prevPos.Item1 != y - 2))
            {
                Tuple<int, int> pieceToRemove = new Tuple<int, int>(y - 1, x + 1);
                piecesToRemove.Add(pieceToRemove);
                Console.WriteLine("xxxxxxxxxxxxxxxx3");
                Tuple<int, int> nextMove = new Tuple<int, int>(y - 2 , x + 2);
                searchPieces(nextMove, destination, start, board, opponent);
            }
            if (x + 2 < board.GetLength(0) && y + 2 < board.GetLength(1) && board[y + 2, x + 2] == 0 && opponent == board[y + 1, x + 1] && (prevPos.Item2 != x + 2 || prevPos.Item1 != y + 2))
            {
                Tuple<int, int> pieceToRemove = new Tuple<int, int>(y + 1, x + 1);
                piecesToRemove.Add(pieceToRemove);
                Console.WriteLine("xxxxxxxxxxxxxxxx4");
                Tuple<int, int> nextMove = new Tuple<int, int>(y + 2, x + 2);
                searchPieces(nextMove, destination, start, board, opponent);
                piecesToRemove.RemoveAt(piecesToRemove.Count - 1);
            }

        }
         public static bool beatingAvaliable(Tuple<int, int> pieceClicked, int [,] board, int opponent)
        {
            int x = pieceClicked.Item2;
            int y = pieceClicked.Item1;
            if (x - 2 >= 0 && y - 2 >= 0 && board[y - 2, x - 2] == 0 && (opponent == board[y - 1, x - 1] || opponent + 2 == board[y - 1, x - 1]))//czy nie wychodzimy poza tablice !!!! BOARD != dziala dopoki nie ma damek na planszy!!
                return true;
            if (x - 2 >=0 && y + 2 < board.GetLength(0) && board[y + 2, x - 2] == 0 && (opponent == board[y + 1, x - 1] || opponent + 2 == board[y + 1, x - 1]))
                    return true;
            if (x + 2 < board.GetLength(0) && y - 2 >= 0 && board[y - 2, x + 2] == 0 && (opponent == board[y - 1, x + 1] || opponent + 2 == board[y - 1, x + 1]))//czy nie wychodzimy poza tablice !!!! BOARD != dziala dopoki nie ma damek na planszy!!
                return true;
            if (x + 2 < board.GetLength(1) && y + 2 < board.GetLength(0) && board[y + 2, x + 2] == 0 && (opponent == board[y + 1, x + 1] || opponent + 2 == board[y + 1, x + 1]))
                    return true;

            return false;
        }
        public static void getBeatingsV2(Tuple<int, int> pieceClicked, int[,] board, int opponent, int length, Tuple<int, int> prevPos, Tuple<int,int> startPos)
        {
            int x = pieceClicked.Item2;
            int y = pieceClicked.Item1;
            //List<Tuple<int, int>> beatings = new List<Tuple<int, int>>();
            if (pieceClicked.Item1 == startPos.Item1 && pieceClicked.Item2 == startPos.Item2 && length > 2)//znaczy ze zapetlilo sie koło - koniec bicia
            {

            }
            else
            {
                if ((x - 2 >= 0 && y - 2 >= 0 && board[y - 2, x - 2] == 0 && opponent == board[y - 1, x - 1] && (prevPos.Item2 != x - 2 || prevPos.Item1 != y - 2)) || (x - 2 >= 0 && y - 2 >= 0 && startPos.Item1 == y - 2 && startPos.Item2 == x - 2 && length > 2 && opponent == board[y - 1, x - 1] && (prevPos.Item2 != x - 2 || prevPos.Item1 != y - 2)))//czy nie wychodzimy poza tablice !!!! BOARD != dziala dopoki nie ma damek na planszy!!
                {
                    Tuple<int, int> legalBeating = new Tuple<int, int>(y - 2, x - 2);
                    Tuple<int, int> pieceToRemove = new Tuple<int, int>(y - 1, x - 1);
                    if (length + 1 > maxBeatings)
                    {
                        maxBeatings = length + 1;
                        //if (beatings.Count > 0)
                        beatings.Clear();

                        beatings.Add(legalBeating);

                        getBeatingsV2(legalBeating, board, opponent, length + 1, pieceClicked, startPos);
                    }
                    else if (length + 1 == maxBeatings)
                    {

                        beatings.Add(legalBeating);
                        getBeatingsV2(legalBeating, board, opponent, length + 1, pieceClicked, startPos);
                    }
                    else if (length + 1 < maxBeatings)
                    {

                        getBeatingsV2(legalBeating, board, opponent, length + 1, pieceClicked, startPos);
                    }
                }

                if ((x - 2 >= 0 && y + 2 < board.GetLength(0) && board[y + 2, x - 2] == 0 && opponent == board[y + 1, x - 1] && (prevPos.Item2 != x - 2 || prevPos.Item1 != y + 2)) || (x - 2 >= 0 && y + 2 >= 0 && startPos.Item1 == y + 2 && startPos.Item2 == x - 2 && length > 2 && opponent == board[y + 1, x - 1] && (prevPos.Item2 != x - 2 || prevPos.Item1 != y + 2)))
                {
                    Tuple<int, int> legalBeating = new Tuple<int, int>(y + 2, x - 2);

                    if (length + 1 > maxBeatings)
                    {
                        maxBeatings = length + 1;
                        //if(beatings.Count>0)
                        beatings.Clear();
                        beatings.Add(legalBeating);
                        getBeatingsV2(legalBeating, board, opponent, length + 1, pieceClicked, startPos);
                    }
                    else if (length + 1 == maxBeatings)
                    {
                        beatings.Add(legalBeating);
                        getBeatingsV2(legalBeating, board, opponent, length + 1, pieceClicked, startPos);
                    }
                    else if (length + 1 < maxBeatings)
                    {
                        getBeatingsV2(legalBeating, board, opponent, length + 1, pieceClicked, startPos);
                    }
                }
                if ((x + 2 < board.GetLength(0) && y - 2 >= 0 && board[y - 2, x + 2] == 0 && opponent == board[y - 1, x + 1] && (prevPos.Item2 != x + 2 || prevPos.Item1 != y - 2)) || (x + 2 >= 0 && y - 2 >= 0 && startPos.Item1 == y - 2 && startPos.Item2 == x + 2 && length > 2 && opponent == board[y - 1, x + 1] && (prevPos.Item2 != x + 2 || prevPos.Item1 != y - 2)))//czy nie wychodzimy poza tablice !!!! BOARD != dziala dopoki nie ma damek na planszy!!
                {
                    Tuple<int, int> legalBeating = new Tuple<int, int>(y - 2, x + 2);
                    if (length + 1 > maxBeatings)
                    {
                        maxBeatings = length + 1;
                        //if (beatings.Count > 0)
                        beatings.Clear();
                        beatings.Add(legalBeating);
                        getBeatingsV2(legalBeating, board, opponent, length + 1, pieceClicked, startPos);
                    }
                    else if (length + 1 == maxBeatings)
                    {
                        beatings.Add(legalBeating);
                        getBeatingsV2(legalBeating, board, opponent, length + 1, pieceClicked, startPos);
                    }
                    else if (length + 1 < maxBeatings)
                    {
                        getBeatingsV2(legalBeating, board, opponent, length + 1, pieceClicked, startPos);
                    }
                }
                if ((x + 2 < board.GetLength(0) && y + 2 < board.GetLength(1) && board[y + 2, x + 2] == 0 && opponent == board[y + 1, x + 1] && (prevPos.Item2 != x + 2 || prevPos.Item1 != y + 2)) || (x + 2 >= 0 && y + 2 >= 0 && startPos.Item1 == y + 2 && startPos.Item2 == x + 2 && length > 2 && opponent == board[y + 1, x + 1] && (prevPos.Item2 != x + 2 || prevPos.Item1 != y + 2)))
                {
                    Tuple<int, int> legalBeating = new Tuple<int, int>(y + 2, x + 2);
                    if (length + 1 > maxBeatings)
                    {
                        maxBeatings = length + 1;
                        //if (beatings.Count > 0) 
                        beatings.Clear();
                        beatings.Add(legalBeating);
                        getBeatingsV2(legalBeating, board, opponent, length + 1, pieceClicked, startPos);
                    }
                    else if (length + 1 == maxBeatings)
                    {
                        beatings.Add(legalBeating);
                        getBeatingsV2(legalBeating, board, opponent, length + 1, pieceClicked, startPos);
                    }
                    else if (length + 1 < maxBeatings)
                    {
                        getBeatingsV2(legalBeating, board, opponent, length + 1, pieceClicked, startPos);
                    }
                }
            }
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
        public static void promotion (Tuple<int,int> position, int[,] board)
        {
            if(position.Item1 == 7 && board[position.Item1, position.Item2] == 2)//czarny na koncu planszy - promocja
            {
                board[position.Item1, position.Item2] = 4;
                Console.WriteLine("PROMOTED!");
            }
            if (position.Item1 == 0 && board[position.Item1, position.Item2] == 1)//bialy na koncu - promocja
            {

                board[position.Item1, position.Item2] = 3;
                Console.WriteLine("PROMOTED!");

            }
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
            //obsluga damek
            if (board[y, x] == 3)
            {
                board[y, x] = 0;
                board[yDestination, xDestination] = 3;
            }
            if (board[y, x] == 4)
            {
                board[y, x] = 0;
                board[yDestination, xDestination] = 4;
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
                    //beatingCounter = 1;
                    beatings.Clear();
                    maxBeatings = 0;
                    getBeatingsV2(pieceClicked, board, 2, 0, pieceClicked, pieceClicked);
                    moves = beatings;
                    Console.WriteLine(maxBeatings);
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
                    //beatingCounter = 1;
                    beatings.Clear();
                    maxBeatings = 0;
                    getBeatingsV2(pieceClicked, board, 1,0, pieceClicked, pieceClicked);
                    moves = beatings;
                    
                    Console.WriteLine(maxBeatings);
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
                    if (x + 1 < board.GetLength(1) && y + 1 < board.GetLength(0))//czy nie wychodzimy poza tablice
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
                if (beatingAvaliable(pieceClicked, board, 2))
                {
                    //beatingCounter = 1;
                    beatings.Clear();
                    maxBeatings = 0;
                    getBeatingsV2(pieceClicked, board, 2, 0, pieceClicked, pieceClicked);
                    moves = beatings;
                    Console.WriteLine(maxBeatings);
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
                    if (x - 1 >= 0 && y + 1 < board.GetLength(0))//czy nie wychodzimy poza tablice
                    {
                        if (board[y + 1, x - 1] == 0)//sprawdzam, czy pole na ukos w lewo jest wolne
                        {
                            Tuple<int, int> legalMove = new Tuple<int, int>(y + 1, x - 1);
                            moves.Insert(i, legalMove);
                            i++;
                        }

                    }
                    if (x + 1 < board.GetLength(1) && y + 1 < board.GetLength(0))//czy nie wychodzimy poza tablice
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
            else if (board[y, x] == 4)
            {
                //obsluga ruchow czarnej damki
                if (beatingAvaliable(pieceClicked, board, 1))
                {
                    //beatingCounter = 1;
                    beatings.Clear();
                    maxBeatings = 0;
                    getBeatingsV2(pieceClicked, board, 1, 0, pieceClicked, pieceClicked);
                    moves = beatings;

                    Console.WriteLine(maxBeatings);
                }
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
                    if (x - 1 >= 0 && y + 1 < board.GetLength(0))//czy nie wychodzimy poza tablice
                    {
                        if (board[y + 1, x - 1] == 0)//sprawdzam, czy pole na ukos w lewo jest wolne
                        {
                            Tuple<int, int> legalMove = new Tuple<int, int>(y + 1, x - 1);
                            moves.Insert(i, legalMove);
                            i++;
                        }

                    }
                    if (x + 1 < board.GetLength(1) && y + 1 < board.GetLength(0))//czy nie wychodzimy poza tablice
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
            //System.Console.WriteLine("Possible moves: ");
            foreach (var move in moves)
            {
                Console.WriteLine(move);
            }
            return moves;
        }

        public static Tuple<int, int> countPieces(int[,] board, int n)
        {
            int white = 0, black = 0;
            for(int i = 0; i<n; i++)
            {
                for(int j = 0; j < n; j++)
                {
                    if (board[i, j] == 1 || board[i,j]==3)
                        white++;
                    if (board[i, j] == 2 || board[i,j]==4)
                        black++;

                }
            }
            return new Tuple<int, int>(white, black);
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
            Tuple<int, int> pieces = new Tuple<int,int> (12,12);//item1 - white, item2 - black
            

            Raylib.InitWindow(cellSize*board.GetLength(1)+200, cellSize * board.GetLength(0), "Chekkers");
            Raylib.SetTargetFPS(60);

            while (!Raylib.WindowShouldClose())
            {
                
                mousePosition = Raylib.GetMousePosition();
                Raylib.BeginDrawing();
                Raylib.ClearBackground(Color.BLUE);
                if (pieces.Item1 == 0 || pieces.Item2 == 0)
                    gameInProgress = false;
                if (!gameInProgress)
                {
                    if (pieces.Item2 == 0)
                    {
                        Raylib.DrawText("WHITE WON! ", 0, 100, 20, Color.WHITE);
                        
                    }
                    if (pieces.Item1 == 0)
                    {
                        Raylib.DrawText("BLACK WON! ", 0, 100, 20, Color.BLACK);
                        
                    }
                    Raylib.DrawText("Press ENTER to Start New Game or ESC to Exit",0, 0, 20, Color.WHITE);
                    if (Raylib.IsKeyPressed(KeyboardKey.KEY_ENTER))
                    {
                        gameInProgress = true;
                        pieces = new Tuple<int, int>(12, 12);
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
                        /*optimalPieces = getOptimalPieces(board);
                         * napisac funkcje zwracajaca liste pionow z optymalnymi ruchami 
                         * (pusta - wszystkie piony bez mozliwosci ruchu(0 ruchow) - wygrywa druzyna ktorej nie jest dana tura!)
                         * funkcja powinna uzywac juz napisanych funkcji (wyszukanie w petli wszystkich pionow druzyny ktorej jest tura
                         * i dla kazdego piona zwrocenie ilosci ruchow
                         * wykorzystac liste optimalPieces do zakreślenia w ramke pieces z optymalna iloscia ruchow
                        
                         */
                        if(pieceChoosen)
                        {
                            drawLegalMoves(moves, board, cellSize);
                            
                        }
                            
                        if (Raylib.IsMouseButtonPressed(MouseButton.MOUSE_BUTTON_LEFT)&&mousePosition.X<cellSize*8)
                        {
                            cellClicked = getCell(mousePosition, cellSize);
                            System.Console.WriteLine("Cell clicked: " + cellClicked.Item1 + " " + cellClicked.Item2);
                            Raylib.DrawCircleV(mousePosition, cellSize / 2, Color.RED);
                            if (pieceChoosen && moves.Contains(cellClicked))//wykonaj ruch
                            {
                                if(beatings.Contains(cellClicked)) //mamy bicie - DO POPRAWY - nie uwzględnia wielokrotnych bić! napisać funkcję WRÓĆ (szukającą droge od cell clicked do piece clicked i zerującą wszystkie napotkane pionki po drodze)
                                {
                                    piecesToRemove.Clear();
                                    if(playerColor==2)
                                        searchPieces(pieceClicked, cellClicked, pieceClicked, board, 1);
                                    if (playerColor == 1)
                                        searchPieces(pieceClicked, cellClicked, pieceClicked, board, 2);

                                    //removePiece(pieceClicked, cellClicked, board);
                                    pieces = countPieces(board, 8);
                                    System.Console.WriteLine("pieces remaining: W" + pieces.Item1 + " B" + pieces.Item2);
                                }
                                movePiece(pieceClicked.Item1, pieceClicked.Item2, cellClicked.Item1, cellClicked.Item2, board);
                                promotion(cellClicked, board);
                                turn = !turn;//zmiana tury
                                if (playerColor == 1)
                                    playerColor = 2;
                                else playerColor = 1;
                                pieceChoosen = false;

                                
                            }
                            if (board[cellClicked.Item1, cellClicked.Item2] == playerColor || board[cellClicked.Item1, cellClicked.Item2] == playerColor+2)
                            {
                                pieceClicked = cellClicked;
                                pieceChoosen = true;
                                //if dany pionek in listaPionkowZOptymalnymiBiciami or listaPionkowZOptymalnymiBiciami is empty, w przeciwnym wypadku nie rysuj ruchow (mimo ze są) bo nie są legalne (nie są optymalne)
                                moves = getLegalMoves(pieceClicked, board);
                                drawLegalMoves(moves, board, cellSize);
                                //drawPieces(board, cellSize);
                            }
                        }

                    }
                    /* Warunki konca:
                     * - jesli gracz A nie ma ruchu - wygrywa gracz B
                     * - jesli gracz B nie ma pionkow - wygrywa gracz A
                     * - jesli nastapiło 15 ruchów damką ze strony jednego z graczy bez ruszenia piona- remis
                     * - jesli 3x pojawilo sie to samo ustawienie na planszy - remis -> zastanowic sie czy wprowadzac - ciezkie do zaimplementowania
                     * 
                     * TODO (znane problemy):
                     * - zapetlone bicia - wykonuje ruch ale nie usuwa z planszy bitych pionow przy zapetlonym biciu
                     * - pozwolić tylko na optymalne ruchy
                     * - dodatkowy warunek konca - brak ruchow 
                     * - wypisywanie kto wygral i dlaczego (+ w tle drukowanie planszy zeby bylo wiadomo co sie zadziało)
                     * - wypisywanie zasad po prawej (tylko optymalne ruchy, dama porusza sie na dowolna odleglosc, ruchy tylko do przodu, bicia do tylu dozwolone)
                     * - AI
                     * - OOP 
                     * - MENU
                     * - WYBÓR OPCJI GRY
                     * 
                     * - poprawki wizualne - przy zapetlonym biciu (nie wyswietla pionka tylko zielone pole), dodac zolte pola przy wielokrotnych biciach (aby bylo widac ktoredy bijemy)
                     */
                    /*optimalPieces = getOptimalPieces(board);
                         * napisac funkcje zwracajaca liste pionow z optymalnymi ruchami 
                         * (pusta - wszystkie piony bez mozliwosci ruchu(0 ruchow) - wygrywa druzyna ktorej nie jest dana tura!)
                         * funkcja powinna uzywac juz napisanych funkcji (wyszukanie w petli wszystkich pionow druzyny ktorej jest tura
                         * i dla kazdego piona zwrocenie ilosci ruchow
                         * wykorzystac liste optimalPieces do zakreślenia w ramke pieces z optymalna iloscia ruchow
                        
                         */
                }
                Raylib.EndDrawing();
            }

            Raylib.CloseWindow();
        }
    }
}