using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets
{
    class GameGrid : MonoBehaviour
    {
        public static int Height;
        public static int Width;
        public const float XDrawStartPos = 12.5f;
        public const float ZDrawStartPos = -12.5f;

        public static string[] Text;
        public static GridPiece[,] Grid;

        public static string[] ReadGameGrid()
        {
            return File.ReadAllLines("Assets\\GameGrid.txt");
        }

        void Start()
        {
            Text = ReadGameGrid();
            Pellets.Load();

            Width = Text[0].Length;
            Height = Text.Length;
            Grid = new GridPiece[Width, Height];

            int x = 0;
            int z = 0;

            foreach (var line in Text)
            {
                foreach (char cell in line)
                {
                    switch (cell)
                    {
                        case '-':
                            Grid[x, z] = GridPiece.Pellet;
                            break;
                        case 'P':
                            Grid[x, z] = GridPiece.PowerPellet;
                            break;
                        case 'X':
                            Grid[x, z] = GridPiece.EmptySpace;
                            break;
                        case 'I':
                        case 'i':
                            Grid[x, z] = GridPiece.Intersection;
                            break;
                        case 'W':
                            Grid[x, z] = GridPiece.Wall;
                            break;
                        case 'L':
                            Grid[x, z] = GridPiece.LeftPortal;
                            break;
                        case 'R':
                            Grid[x, z] = GridPiece.RightPortal;
                            break;
                        case 'H':
                            Grid[x, z] = GridPiece.GhostHome;
                            break;
                    }
                    x++;
                }
                x = 0;
                z++;
            }
        }

        /// <summary>
        /// Calculate the draw position for an object
        /// </summary>
        /// <param name="x">Horizontal</param>
        /// <param name="y">Height</param>
        /// <param name="z">Vertical</param>
        /// <returns></returns>
        public static Vector3 Position(int x, float y, int z)
        {
            return new Vector3(GetDrawPositionX(x), y, GetDrawPositionZ(z));
        }

        public static float GetDrawPositionX(int x)
        {
            return XDrawStartPos - x;
        }

        public static float GetDrawPositionZ(int y)
        {
            return ZDrawStartPos + y;
        }

        /// <summary>
        /// Is the position next to the base
        /// </summary>
        /// <param name="x"></param>
        /// <param name="z"></param>
        /// <returns></returns>
        public static bool NextToBase(int x, int z)
        {
            return (x == 13 || x == 14) && z == 11;
        }

        public static List<Movement> GetPossibleMoves(int x, int z)
        {
            var possibleMoves = new List<Movement>();

            if (Grid[x, z] == GridPiece.LeftPortal)
            {
                return new List<Movement> {Movement.TeleportRight};
            }
            if (Grid[x, z] == GridPiece.RightPortal)
            {
                return new List<Movement> {Movement.TeleportLeft};
            }

            // Up
            int up = z - 1;
            if (up >= 0)
            {
                if(isValidMove(x,up))
                {
                    possibleMoves.Add(Movement.Up);
                }
            }

            // Down
            int down = z + 1;
            if (down < Height)
            {
                if(isValidMove(x,down))
                {
                    possibleMoves.Add(Movement.Down);
                }
            }

            // Left
            int left = x - 1;
            if (left >= 0)
            {
                if(isValidMove(left,z))
                {
                    possibleMoves.Add(Movement.Left);
                }
            }

            // Right
            int right = x + 1;
            if (right < Width)
            {
                if(isValidMove(right,z))
                {
                    possibleMoves.Add(Movement.Right);
                }
            }

            return possibleMoves;
        }

        private static bool isValidMove(int x, int z)
        {
            return
                Grid[x, z] == GridPiece.Pellet ||
                Grid[x, z] == GridPiece.PowerPellet ||
                Grid[x, z] == GridPiece.EmptySpace ||
                Grid[x, z] == GridPiece.Intersection;
        }

        public static int Up(int z)
        {
            return z - 1;
        }

        public static int Down(int z)
        {
            return z + 1;
        }

        public static int Left(int x)
        {
            return x - 1;
        }

        public static int Right(int x)
        {
            return x + 1;
        }

        public static int TeleportLeft(int x)
        {
            // TODO: Go to an exact location instead of teleporting from current location, otherwise this can lead to player being thrown out of the play area!
            return x - 24;
        }

        public static int TeleportRight(int x)
        {
            return x + 24;
        }

        public static Movement GetOppositeMovement(Movement movement)
        {
            switch (movement)
            {
                case Movement.Left:
                    return Movement.Right;
                case Movement.Right:
                    return Movement.Left;
                case Movement.Up:
                    return Movement.Down;
                default:
                case Movement.Down:
                    return Movement.Up;
            }
        }
    }

    public enum GridPiece
    {
        Pellet = 0,
        PowerPellet = 1,
        EmptySpace = 2,
        Intersection = 3,
        Wall = 4,
        LeftPortal = 5,
        RightPortal = 6,
        GhostHome = 7
    }

    public enum Movement
    {
        Up = 0,
        Down = 1,
        Left = 2,
        Right = 3,
        TeleportRight = 4,
        TeleportLeft = 5
    }
}
