namespace _3DSnake.Models
{
    internal sealed class PlayFieldModel
    {
        internal CellState[,,] PlayField;
        internal short Dimension = 0;
        internal Queue<(int x, int y, int z)> Snake = new();
        internal Sides Side = Sides.FrontZX;
        internal Direction Direction 
        { 
            get => _direction; 
            set
            {
                if (!NewDirectionIsOpposite(value))
                {
                    _direction = value;
                }
            }
        }

        internal (int x, int y, int z) Food;
        internal bool GameOver = false;

        private const string EMPTY_CELL = ". ";
        private const string SNAKE_BODY_CELL = "@ ";
        private const string FOOD_CELL = "0 ";
        private Direction _direction = Direction.Right;

        internal PlayFieldModel(short dimension)
        {
            Dimension = dimension;

            PlayField = new CellState[Dimension, Dimension, Dimension];

            InitPlayField();

            InitSnake();

            PlaceFood();
        }

        private void InitPlayField()
        {
            for (var x = 0; x < Dimension; x++)
            {
                for (var y = 0; y < Dimension; y++)
                {
                    for (var z = 0; z < Dimension; z++)
                    {
                        PlayField[x, y, z] = CellState.Empty;
                    }
                }
            }
        }

        private void InitSnake()
        {
            var coordinate = Dimension / 2;

            Snake.Enqueue((coordinate, coordinate, coordinate));
            Snake.Enqueue((coordinate, coordinate - 1, coordinate));
            Snake.Enqueue((coordinate, coordinate - 2, coordinate));
            Snake.Enqueue((coordinate, coordinate - 3, coordinate));
            Snake.Enqueue((coordinate, coordinate - 4, coordinate));
        }

        private void PlaceFood()
        {
            var rand = new Random();

            var food = (rand.Next(0, Dimension), rand.Next(0, Dimension), 9);

            if (CollidesWithSnake(food))
            {
                PlaceFood();
            }

            Food = food;
        }

        internal void Update()
        {
            var head = Snake.Last();
            var newHead = head;

            switch (Direction)
            {
                case Direction.Up:
                    var i = head.y - 1 < 0 ? Dimension - 1 : head.y - 1;
                    newHead = (head.x, i, head.z);
                    break;
                case Direction.Down:
                    i = head.y + 1 > Dimension - 1 ? 0 : head.y + 1;
                    newHead = (head.x, i, head.z);
                    break;
                case Direction.Left:
                    i = head.x - 1 < 0 ? Dimension - 1 : head.x - 1;
                    newHead = (i, head.y, head.z);
                    break;
                case Direction.Right:
                    i = head.x + 1 > Dimension - 1 ? 0 : head.x + 1;
                    newHead = (i, head.y, head.z);
                    break;
                case Direction.Forward:
                    i = head.z + 1 > Dimension - 1 ? 0 : head.z + 1;
                    newHead = (head.x, head.y, i);
                    break;
                case Direction.Backward:
                    i = head.z - 1 < 0 ? Dimension - 1 : head.z - 1;
                    newHead = (head.x, head.y, i);
                    break;
            }

            if (CollidesWithSnake(newHead))
            {
                GameOver = true;
            }

            Snake.Enqueue(newHead);

            if (AteFood(newHead))
            {
                PlaceFood();
            }
            else
            {
                Snake.Dequeue();
            }
        }

        internal string RenderFrame()
        {
            var result = "";

            InitPlayField();

            PutSnakeIntoField();
            PutFoodIntoField();

            var frame = new CellState[Dimension, Dimension];

            for (var i = 0; i < Dimension; i++)
            {
                var layer = new CellState[Dimension, Dimension];

                switch (Side)
                {
                    case Sides.FrontZX: layer = GetLayer(y: i); break;
                    case Sides.TopYX: layer = GetLayer(z: i); break;
                    case Sides.RightYZ: layer = GetLayer(x: i); break;
                }

                var squashed = SquashLayer(layer);

                for (var j = 0; j < squashed.Length; j++)
                {
                    frame[i, j] = squashed[j];
                }
            }

            for (var i = 0; i < Dimension; i++)
            {
                for (var j = 0; j < Dimension; j++)
                {
                    switch (frame[i, j])
                    {
                        case CellState.Food:
                            result += FOOD_CELL;
                            break;
                        case CellState.SnakeBody:
                            result += SNAKE_BODY_CELL;
                            break;
                        case CellState.Empty:
                            result += EMPTY_CELL;
                            break;
                    }
                }

                result += "\n";
            }

            return result;
        }

        private CellState[] SquashLayer(CellState[,] layer)
        {
            var squashed = new CellState[Dimension];

            for (var i = 0; i < Dimension; i++)
            {
                squashed[i] = CellState.Empty;
            }

            for (var i = 0; i < Dimension; i++)
            {
                for (var j = 0; j < Dimension; j++)
                {
                    if (layer[i, j] == CellState.SnakeBody)
                    {
                        if (squashed[i] != CellState.Food)
                        {
                            squashed[i] = CellState.SnakeBody;
                        }
                    }

                    if (layer[i, j] == CellState.Food)
                    {
                        squashed[i] = CellState.Food;
                    }

                    if (layer[i, j] == CellState.Empty && squashed[i] != CellState.Food && squashed[i] != CellState.SnakeBody)
                    {
                        squashed[i] = CellState.Empty;
                    }
                }
            }

            return squashed;
        }

        private CellState[,] GetLayer(int x = -1, int y = -1, int z = -1)
        {
            var layer = new CellState[Dimension, Dimension];

            for (var i = 0; i < Dimension; i++)
            {
                for (var j = 0; j < Dimension; j++)
                {
                    if (x >= 0)
                    {
                        layer[i, j] = PlayField[x, i, j];
                    }

                    if (y >= 0)
                    {
                        layer[i, j] = PlayField[i, y, j];
                    }

                    if (z >= 0)
                    {
                        layer[i, j] = PlayField[i, j, z];
                    }
                }
            }

            return layer;
        }

        private void PutSnakeIntoField()
        {
            foreach (var (x, y, z) in Snake)
            {
                PlayField[x, y, z] = CellState.SnakeBody;
            }
        }

        private bool CollidesWithSnake((int, int, int) obj)
        {
            return Snake.Contains(obj);
        }

        private void PutFoodIntoField()
        {
            PlayField[Food.x, Food.y, Food.z] = CellState.Food;
        }

        private bool AteFood((int x, int y, int z) newHead)
        {
            return newHead == Food;
        }

        private bool NewDirectionIsOpposite(Direction value)
        {
            return 
                (_direction == Direction.Forward && value == Direction.Backward) ||
                (_direction == Direction.Backward && value == Direction.Forward) ||
                (_direction == Direction.Up && value == Direction.Down) ||
                (_direction == Direction.Down && value == Direction.Up) ||
                (_direction == Direction.Left && value == Direction.Right) ||
                (_direction == Direction.Right && value == Direction.Left);
        }

        internal string GetSnakeString()
        {
            var result = "";

            foreach (var (x, y, z) in Snake)
            {
                result += $"(x: {x}, y: {y}, z: {z})\n";
            }

            return result;
        }
    }
}
