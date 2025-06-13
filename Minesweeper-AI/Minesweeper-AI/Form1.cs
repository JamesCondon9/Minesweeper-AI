using System.Diagnostics;
using System.Media;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Window;

namespace Minesweeper_AI
{
    public partial class Form1 : Form
    {
        private TableLayoutPanel difficultyBox;
        private TableLayoutPanel mineGrid;
        private int difficulty = 0; // 1: Beginner  2: Intermediate  3: Expert
        private int[,] mineField; // 2D array to hold minefield data
                                  // 0: Empty, 1-8: Number of adjacent mines, 9: Mine
        private int mineCount = 0;
        private int flaggedCount = 0;
        private HashSet<Point> uncoveredTiles = new HashSet<Point>();
        private bool clickedFirst = false; // Flag to check if the first tile h6as been clicked
        private bool hasWon = false;
        private int gameSeed = 18472; // Set to NULL for random seed (i think, don't know if that works)
        private bool displayingFrontier = false; // State whether or not to highlight the frontier

        // Add private readonly Bitmap fields for all images used
        private readonly Bitmap coveredTileBitmap = new(Properties.Resources.Covered_Tile, new Size(32, 32));
        private readonly Bitmap emptyTileBitmap = new(Properties.Resources.Empty_Tile, new Size(32, 32));
        private readonly Bitmap flagTileBitmap = new(Properties.Resources.Flag_Tile, new Size(32, 32));
        private readonly Bitmap mineTileBitmap = new(Properties.Resources.Mine_Tile, new Size(32, 32));
        private readonly Bitmap monkaHmmBitmap = new(Properties.Resources.monkaHmm, new Size(63, 63));
        private readonly Bitmap monkaOmegaBitmap = new(Properties.Resources.monkaOmega, new Size(63, 63));
        private readonly Bitmap pepegaBitmap = new(Properties.Resources.pepega, new Size(63, 63));
        private readonly Bitmap pepeHandsBitmap = new(Properties.Resources.pepeHands, new Size(63, 63));
        private readonly Bitmap redMineTileBitmap = new(Properties.Resources.Red_Mine_Tile, new Size(32, 32));
        private readonly Bitmap tile1Bitmap = new(Properties.Resources.Tile_1, new Size(32, 32));
        private readonly Bitmap tile2Bitmap = new(Properties.Resources.Tile_2, new Size(32, 32));
        private readonly Bitmap tile3Bitmap = new(Properties.Resources.Tile_3, new Size(32, 32));
        private readonly Bitmap tile4Bitmap = new(Properties.Resources.Tile_4, new Size(32, 32));
        private readonly Bitmap tile5Bitmap = new(Properties.Resources.Tile_5, new Size(32, 32));
        private readonly Bitmap tile6Bitmap = new(Properties.Resources.Tile_6, new Size(32, 32));
        private readonly Bitmap tile7Bitmap = new(Properties.Resources.Tile_7, new Size(32, 32));
        private readonly Bitmap tile8Bitmap = new(Properties.Resources.Tile_8, new Size(32, 32));
        private readonly Bitmap emptyCounterBitmap = new(Properties.Resources.Empty_Counter, new Size(50, 100));
        private readonly Bitmap counter0Bitmap = new(Properties.Resources.Zero_Counter, new Size(50, 100));
        private readonly Bitmap counter1Bitmap = new(Properties.Resources.One_Counter, new Size(50, 100));
        private readonly Bitmap counter2Bitmap = new(Properties.Resources.Two_Counter, new Size(50, 100));
        private readonly Bitmap counter3Bitmap = new(Properties.Resources.Three_Counter, new Size(50, 100));
        private readonly Bitmap counter4Bitmap = new(Properties.Resources.Four_Counter, new Size(50, 100));
        private readonly Bitmap counter5Bitmap = new(Properties.Resources.Five_Counter, new Size(50, 100));
        private readonly Bitmap counter6Bitmap = new(Properties.Resources.Six_Counter, new Size(50, 100));
        private readonly Bitmap counter7Bitmap = new(Properties.Resources.Seven_Counter, new Size(50, 100));
        private readonly Bitmap counter8Bitmap = new(Properties.Resources.Eight_Counter, new Size(50, 100));
        private readonly Bitmap counter9Bitmap = new(Properties.Resources.Nine_Counter, new Size(50, 100));
        private HashSet<Bitmap> uncoveredTileBitmaps;


        public Form1()
        {
            InitializeComponent();

            uncoveredTileBitmaps = new HashSet<Bitmap>
                        {
                            tile1Bitmap, tile2Bitmap, tile3Bitmap, tile4Bitmap,
                            tile5Bitmap, tile6Bitmap, tile7Bitmap, tile8Bitmap,
                            emptyTileBitmap
                        };

            loadMenu();
        }


        private void loadMenu()
        {
            // Show title
            PictureBox title = new PictureBox();
            title.Image = Properties.Resources.title;
            title.Dock = DockStyle.Fill;
            title.SizeMode = PictureBoxSizeMode.CenterImage;
            upperBarPanel.Controls.Add(title, 1, 0);

            // Show difficulty options
            difficultyBox = new TableLayoutPanel();
            difficultyBox.Anchor = AnchorStyles.None;
            difficultyBox.Size = new Size(350, 300);
            difficultyBox.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));

            for (int i = 0; i < 3; i++)
            {
                difficultyBox.RowStyles.Add(new RowStyle(SizeType.Percent, 20F));
            }
            difficultyBox.RowStyles.Add(new RowStyle(SizeType.Percent, 40F));

            difficultyBox.BackColor = Color.Red;

            RadioButton beginnerButton = new RadioButton();
            beginnerButton.Text = "Beginner (9x9, 10 Mines)";
            beginnerButton.Enabled = true;
            beginnerButton.Dock = DockStyle.Fill;
            beginnerButton.CheckedChanged += DifficultyChanged;

            RadioButton intermediateButton = new RadioButton();
            intermediateButton.Text = "Intermediate (16x16, 40 Mines)";
            intermediateButton.Enabled = true;
            intermediateButton.Dock = DockStyle.Fill;
            intermediateButton.CheckedChanged += DifficultyChanged;

            RadioButton expertButton = new RadioButton();
            expertButton.Text = "Expert (16x30, 99 Mines)";
            expertButton.Enabled = true;
            expertButton.Dock = DockStyle.Fill;
            expertButton.CheckedChanged += DifficultyChanged;

            Button startButton = new Button();
            startButton.Image = Properties.Resources.beginButton;
            startButton.Dock = DockStyle.Fill;

            startButton.Click += startGame;

            difficultyBox.Controls.Add(beginnerButton, 0, 0);
            difficultyBox.Controls.Add(intermediateButton, 0, 1);
            difficultyBox.Controls.Add(expertButton, 0, 2);
            difficultyBox.Controls.Add(startButton, 0, 3);

            mainPanel.Controls.Add(difficultyBox);

        }

        private void DifficultyChanged(object sender, EventArgs e)
        {
            RadioButton rb = sender as RadioButton;
            if (rb != null && rb.Checked)
            {
                if (rb.Text.StartsWith("Beginner"))
                {
                    difficulty = 1;
                }
                else if (rb.Text.StartsWith("Intermediate"))
                {
                    difficulty = 2;
                }
                else if (rb.Text.StartsWith("Expert"))
                {
                    difficulty = 3;
                }
            }
        }

        /// <summary>
        /// Creates and returns a TableLayoutPanel representing the Minesweeper grid,
        /// with the specified number of rows and columns. Each tile is a button 
        /// initialized with default styling and tagged with its grid position.
        /// </summary>
        /// <param name="rows">The number of rows in the mine grid.</param>
        /// <param name="cols">The number of columns in the mine grid.</param>
        /// <param name="mineCount"> The number of mines to be placed in the grid.</param>
        /// <returns>A TableLayoutPanel filled with button controls arranged in a grid.</returns>
        private TableLayoutPanel CreateMineGrid(int rows, int cols, int mineCount)
        {

            // -- 2D ARRAY -- (Minefield data attribute)
            placeMines(rows, cols, mineCount);
            calculateTiles();

            // -- UI ELEMENTS -- (Returned value)

            // Create TableLayoutPanel of size according to chosen difficulty
            TableLayoutPanel table = new TableLayoutPanel();
            table.RowCount = rows;
            table.Anchor = AnchorStyles.None;
            table.ColumnCount = cols;
            table.Dock = DockStyle.Fill;
            table.Margin = new Padding(0);
            table.Padding = new Padding(0);
            table.CellBorderStyle = TableLayoutPanelCellBorderStyle.None;
            table.AutoSize = false;
            table.Size = new Size(cols * 32, rows * 32); // Assuming each tile is 32x32 pixels

            // Set width and height of columns and rows to 32px each
            table.RowStyles.Clear();
            table.ColumnStyles.Clear();
            for (int i = 0; i < rows; i++)
            {
                table.RowStyles.Add(new RowStyle(SizeType.Absolute, 32));
            }
            for (int j = 0; j < cols; j++)
            {
                table.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 32));
            }

            // Add tiles
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    Button tile = new Button();
                    tile.Dock = DockStyle.Fill;
                    tile.FlatStyle = FlatStyle.Flat;
                    tile.FlatAppearance.BorderSize = 0;
                    tile.Margin = new Padding(0);
                    tile.Image = coveredTileBitmap; // Use cached bitmap

                    // Tag it with its position for later use
                    tile.Tag = new Point(i, j);

                    // Add click event handler (both left and right click)
                    tile.MouseClick += Tile_Click;

                    // Scare pepe when held down
                    tile.MouseDown += scarePepe;
                    tile.MouseUp += unscarePepe;
                    tile.MouseUp += Tile_MouseUp;

                    table.Controls.Add(tile, j, i); // (column, row)
                }
            }

            return table;
        }

        private void placeMines(int rows, int cols, int mineCount) {
            mineField = new int[rows, cols];
            Random rng = new Random(gameSeed); // REMOVE SEED AFTER TO ALLOW RANDOMNESS (EMPTY PARAMETER)

            HashSet<Point> placedMines = new HashSet<Point>();
            while (placedMines.Count < mineCount)
            {
                int x = rng.Next(rows);
                int y = rng.Next(cols);
                Point mineLocation = new Point(x, y);
                if (!placedMines.Contains(mineLocation))
                {
                    placedMines.Add(mineLocation);
                    mineField[x, y] = 9; // 9 represents a mine
                }
            }
        }

        private void calculateTiles()
        {
            int rows = mineField.GetLength(0);
            int cols = mineField.GetLength(1);
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    if (mineField[i, j] == 9) // If it's a mine, skip it
                        continue;
                    int mineCount = 0;
                    for (int x = -1; x <= 1; x++)
                    {
                        for (int y = -1; y <= 1; y++)
                        {
                            if (x == 0 && y == 0) // Skip the tile itself
                                continue;
                            int newX = i + x;
                            int newY = j + y;
                            if (newX >= 0 && newX < rows && newY >= 0 && newY < cols)
                            {
                                if (mineField[newX, newY] == 9)
                                    mineCount++;
                            }
                        }
                    }
                    mineField[i, j] = mineCount;
                }
            }
        }

        private async void startGame(object sender, EventArgs e)
        {
            // Don't start game if difficulty not selected, 
            if (!(0 < difficulty && difficulty <= 3))
            {
                MessageBox.Show("Please select a difficulty before playing.");
                return;
            }

            // Initialise global variables for a new game
            if (uncoveredTiles != null)
                uncoveredTiles.Clear();
            else
                uncoveredTiles = new HashSet<Point>();

            clickedFirst = false;

            // Pause for a 0.5s to make the button press animation make sense
            await Task.Delay(500);

            // Prepare new screen elements to swap old ones with

            // RESTART BUTTON
            Button restartButton = new Button();

            restartButton.Image = monkaHmmBitmap;
            restartButton.Anchor = AnchorStyles.None;
            restartButton.Size = new Size(63, 63);
            restartButton.FlatStyle = FlatStyle.Flat;
            restartButton.FlatAppearance.BorderSize = 4;
            restartButton.FlatAppearance.BorderColor = Color.FromArgb(31, 31, 31);
            restartButton.Click += restartGame; // Restart game on click
            BackColor = Color.FromArgb(64, 64, 64);

           
            // MINE GRID
            int rows = 0, cols = 0;
            switch (difficulty)
            {
                case 1:
                    rows = 9; cols = 9; mineCount = 10; break;
                case 2:
                    rows = 16; cols = 16; mineCount = 40; break;
                case 3:
                    rows = 16; cols = 30; mineCount = 99; break;
                default:
                    MessageBox.Show("Trying to generate invalid minefield size, terminating", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
            }

            // REMAINING MINES COUNTER AND TIMER
            PictureBox hundredsMineCounter = new PictureBox();
            PictureBox tensMineCounter = new PictureBox();
            PictureBox unitsMineCounter = new PictureBox();

            // Set SizeMode to Zoom so images scale proportionally (aspect ratio preserved)
            hundredsMineCounter.SizeMode = PictureBoxSizeMode.Zoom;
            tensMineCounter.SizeMode = PictureBoxSizeMode.Zoom;
            unitsMineCounter.SizeMode = PictureBoxSizeMode.Zoom;

            // Set margin to 0 for each PictureBox
            hundredsMineCounter.Margin = new Padding(0);
            tensMineCounter.Margin = new Padding(0);
            unitsMineCounter.Margin = new Padding(0);

            // Make them fill their cells
            hundredsMineCounter.Dock = DockStyle.Right;
            tensMineCounter.Dock = DockStyle.Fill;
            unitsMineCounter.Dock = DockStyle.Left;

            // Set initial image to empty counter
            hundredsMineCounter.Image = counter1Bitmap;
            tensMineCounter.Image = counter1Bitmap;
            unitsMineCounter.Image = counter1Bitmap;

            // Prepare mineRemainingCounterBox TableLayoutPanel to add PictureBoxes to
            TableLayoutPanel minesRemainingCounterBox = new TableLayoutPanel();
            minesRemainingCounterBox.ColumnCount = 3;
            minesRemainingCounterBox.RowCount = 1;

            int digitWidth = 25; // or whatever width your bitmap is

            // Set column widths to evenly distribute space (or fix width if preferred)
            minesRemainingCounterBox.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, digitWidth));
            minesRemainingCounterBox.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, digitWidth));
            minesRemainingCounterBox.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, digitWidth));
            minesRemainingCounterBox.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));

            // Ensure it sizes correctly and is centered
            minesRemainingCounterBox.AutoSize = true;
            minesRemainingCounterBox.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            minesRemainingCounterBox.Dock = DockStyle.Fill;
            minesRemainingCounterBox.Anchor = AnchorStyles.None;
            minesRemainingCounterBox.BackColor = Color.Red;

            // Set form window to exactly fit the minefield, upperBarPanel and lowerBarPanel
            int gridWidth = 32 * cols;
            int gridHeight = 32 * rows;

            int upperHeight = upperBarPanel.Height;
            int lowerHeight = lowerBarPanel.Height;

            this.ClientSize = new Size(gridWidth, gridHeight + upperHeight + lowerHeight);

            mainPanel.RowStyles[1].SizeType = SizeType.Absolute;
            mainPanel.RowStyles[1].Height = rows * 32;

            // Remove start menu stuff
            Control title = upperBarPanel.GetControlFromPosition(1, 0);
            if (title != null)
            {
                upperBarPanel.Controls.Remove(title);
            }

            if (difficultyBox != null)
            {
                mainPanel.Controls.Remove(difficultyBox);
                mainPanel.Margin = new Padding(0); // Remove margins to make mineGrid fit the window
                mainPanel.Padding = new Padding(0);
                difficultyBox.Dispose();
            }

            upperBarPanel.Controls.Add(minesRemainingCounterBox, 0, 0); // Add to upper bar panel

            // Add picture boxes to the mine remaining counter box
            minesRemainingCounterBox.Controls.Add(hundredsMineCounter, 0, 0);
            minesRemainingCounterBox.Controls.Add(tensMineCounter, 1, 0);
            minesRemainingCounterBox.Controls.Add(unitsMineCounter, 2, 0);
            
            updateRemainingMines();


            upperBarPanel.Controls.Add(minesRemainingCounterBox, 0, 0);
            upperBarPanel.Controls.Add(restartButton, 1, 0);

            // Ensure minesRemainingCounterBox is on top
            minesRemainingCounterBox.BringToFront();

            // Replace logo with reset button
            upperBarPanel.Controls.Add(restartButton, 1, 0);

            // Draw mine grid
            mineGrid = CreateMineGrid(rows, cols, mineCount);
            mainPanel.Controls.Add(mineGrid, 0, 1);


            // Draw lower bar panel
            if (lowerBarPanel.GetControlFromPosition(0, 0) == null)
            {
                // Frontier Analysis / Seed Buttons
                Button showFrontierButton = new Button();
                showFrontierButton.Text = "Frontier";

                Button setRandomSeed = new Button();
                setRandomSeed.Text = "Random";
                setRandomSeed.Click += setRandomSeed_Click;

                Button setSpecificSeed = new Button();
                setSpecificSeed.Text = "Set Seed";
                setSpecificSeed.Click += setSpecificSeed_Click;

                // Resize buttons to fit their text with some padding
                ResizeButtonToFitText(showFrontierButton);
                ResizeButtonToFitText(setRandomSeed);
                ResizeButtonToFitText(setSpecificSeed);

                ToolTip tooltip = new ToolTip();
                tooltip.SetToolTip(showFrontierButton, "Show which cells are currently being analysed");
                tooltip.SetToolTip(setRandomSeed, "Set a random seed for the minefield");
                tooltip.SetToolTip(setSpecificSeed, "Set a specific seed for the minefield");


                // Add buttons to lower bar
                lowerBarPanel.Controls.Add(showFrontierButton, 0, 0);
                lowerBarPanel.Controls.Add(setRandomSeed, 1, 0);
                lowerBarPanel.Controls.Add(setSpecificSeed, 2, 0);
            }

            Label previousGameSeedLabel = (Label)lowerBarPanel.GetControlFromPosition(3, 0);

            if (previousGameSeedLabel != null)
            {
                lowerBarPanel.Controls.Remove(previousGameSeedLabel);
            }

            // Replace the label creation and assignment with this code to ensure the text fits
            Label gameSeedLabel = new Label();
            gameSeedLabel.AutoSize = false;
            gameSeedLabel.TextAlign = ContentAlignment.MiddleLeft;
            gameSeedLabel.Dock = DockStyle.Fill;
            gameSeedLabel.Text = "Board Seed: " + gameSeed;

            // Adjust font size if text is too wide for the label
            gameSeedLabel.SizeChanged += (s, ev) =>
            {
                var label = (Label)s;
                int minFontSize = 6;
                int maxFontSize = 20;
                using (Graphics g = label.CreateGraphics())
                {
                    for (int fontSize = maxFontSize; fontSize >= minFontSize; fontSize--)
                    {
                        using (Font testFont = new Font(label.Font.FontFamily, fontSize, label.Font.Style))
                        {
                            SizeF textSize = g.MeasureString(label.Text, testFont);
                            if (textSize.Width <= label.Width)
                            {
                                label.Font = new Font(label.Font.FontFamily, fontSize, label.Font.Style);
                                break;
                            }
                        }
                    }
                }
            };



            lowerBarPanel.Controls.Add(gameSeedLabel, 3, 0);
        }

        private void restartGame(object sender, EventArgs e)
        {

            // Remove any existing control from column 0, row 0
            // Remove any existing control at column 0, row 1
            Control control2 = upperBarPanel.GetControlFromPosition(0, 0);
            if (control2 != null)
            {
                mainPanel.Controls.Remove(control2);
                control2.Dispose(); // Optional: free resources if not reused
            }


            // Remove any existing control at column 0, row 1
            Control control = mainPanel.GetControlFromPosition(0, 1);
            if (control != null)
            {
                mainPanel.Controls.Remove(control);
                control.Dispose(); // Optional: free resources if not reused
            }

            hasWon = false;

            startGame(sender, e); // Restart the game by calling startGame again
        }

        private void Tile_Click(object sender, MouseEventArgs e) {
            Button clickedTile = sender as Button;

            // Disable further clicks without disabling the button
            clickedTile.MouseClick -= Tile_Click;

            Point clickedPoint = (Point)clickedTile.Tag;
            int i = clickedPoint.X;
            int j = clickedPoint.Y;

            int value = mineField[i, j];

            if (!clickedFirst)
            {
                // If this is the first click, we need to ensure that it is an empty tile (0)
                handleFirstClick(i, j);
                return;
            }

            if (value == 0) // Empty tile
            {
                clickedTile.Image = emptyTileBitmap;
                // Click all surrounding tiles (since none will be a mine) to uncover all adjacent mines
                uncoverSurroundingTiles(i, j);
            }
            else if (1 <= value && value <= 8)
            {
                Bitmap tileImage = value switch
                {
                    1 => tile1Bitmap,
                    2 => tile2Bitmap,
                    3 => tile3Bitmap,
                    4 => tile4Bitmap,
                    5 => tile5Bitmap,
                    6 => tile6Bitmap,
                    7 => tile7Bitmap,
                    8 => tile8Bitmap,
                    _ => null
                };

                clickedTile.Image = tileImage;
            }
            else // Mine tile, lose game
            {
                clickedTile.Image = redMineTileBitmap;
                loseGame(i, j); // Handle losing the game
            }

            checkIfWon(); // Check if the game is won after each click
        }

        private (Bitmap, Bitmap, Bitmap) numberToSevenSegmentDisplay(int number) {

            int hundreds = number / 100;
            int tens = (number / 10) % 10;
            int units = number % 10;

            Bitmap[] counterBitmaps = new Bitmap[]
            {
                counter0Bitmap, counter1Bitmap, counter2Bitmap, counter3Bitmap, counter4Bitmap,
                counter5Bitmap, counter6Bitmap, counter7Bitmap, counter8Bitmap, counter9Bitmap
            };

            Bitmap hundredsCounterBitmap = counterBitmaps[hundreds];
            Bitmap tensCounterBitmap = counterBitmaps[tens];
            Bitmap unitsCounterBitmap = counterBitmaps[units];

            return (hundredsCounterBitmap, tensCounterBitmap, unitsCounterBitmap);
        }

        private void updateRemainingMines() // Relies on there already existing three PictureBoxes in the mineRemainingCounterBox in order: hundreds, tens, units
        {
            int number = mineCount - flaggedCount;
            var (hundredsCounter, tensCounter, unitsCounter) = numberToSevenSegmentDisplay(number);
            TableLayoutPanel minesRemainingCounterBox = (TableLayoutPanel)upperBarPanel.GetControlFromPosition(0, 0);

            // Replace the images in the existing PictureBoxes
            if (minesRemainingCounterBox.Controls.Count >= 3)
            {
                PictureBox existingHundreds = minesRemainingCounterBox.GetControlFromPosition(0,0) as PictureBox;
                PictureBox existingTens = minesRemainingCounterBox.GetControlFromPosition(1, 0) as PictureBox;
                PictureBox existingUnits = minesRemainingCounterBox.GetControlFromPosition(2, 0) as PictureBox;

                if (existingHundreds != null) existingHundreds.Image = hundredsCounter;
                if (existingTens != null) existingTens.Image = tensCounter;
                if (existingUnits != null) existingUnits.Image = unitsCounter;
            }
        }

        private void uncoverSurroundingTiles(int i, int j)
        {
            for (int x = -1; x <= 1; x++)
            {
                for (int y = -1; y <= 1; y++)
                {
                    if (x == 0 && y == 0) // Skip the tile itself
                        continue;
                    int newX = i + x;
                    int newY = j + y;
                    if (newX >= 0 && newX < mineField.GetLength(0) && newY >= 0 && newY < mineField.GetLength(1))
                    {
                        Button tile = mineGrid.GetControlFromPosition(newY, newX) as Button;
                        if (tile != null)
                        {
                            // Check if the tile has already been clicked to avoid infinite recursion
                            if (!uncoveredTiles.Contains(new Point(newX, newY)))
                            {
                                uncoveredTiles.Add(new Point(newX, newY)); // Mark the tile as clicked
                                Tile_Click(tile, new MouseEventArgs(MouseButtons.Left, 1, 0, 0, 0)); // Simulate a left mouse click
                            }
                        }
                    }
                }
            }
        }

        private void scarePepe(object sender, EventArgs e) { 
            Button restartButton = upperBarPanel.GetControlFromPosition(1, 0) as Button;
            restartButton.Image = monkaOmegaBitmap;
        }

        private void unscarePepe(object sender, EventArgs e) {
            Button restartButton = upperBarPanel.GetControlFromPosition(1, 0) as Button;
            restartButton.Image = monkaHmmBitmap;
        }

        private void handleFirstClick(int i, int j)
        {
            int value = mineField[i, j];
            clickedFirst = true; // Set the flag to true after the first click

            if (value == 9) // If the first click is on a mine, relocate it to an empty tile
            {
                Debug.WriteLine("HIT1");
                relocateMine(i, j, i, j);
            }
            else if(value != 0) // If first click is on a number, relocate all adjacent mines to empty tiles 
            {
                Debug.WriteLine("HIT2");
                mineField[i, j] = 0; // Set the clicked tile to empty
                for(int x=-1; x <= 1; x++) // Relocate all adjacent mines
                {
                    for(int y=-1; y <= 1; y++)
                    {
                        if (x == 0 && y == 0) // Skip the tile itself
                            continue;
                        int newX = i + x;
                        int newY = j + y;
                        if (newX >= 0 && newX < mineField.GetLength(0) && newY >= 0 && newY < mineField.GetLength(1))
                        {
                            if (mineField[newX, newY] == 9) // If it's a mine, relocate it
                            {
                                relocateMine(newX, newY, i, j);
                            }
                        }
                    }
                }
            }

            calculateTiles();
            Tile_Click(mineGrid.GetControlFromPosition(j, i), new MouseEventArgs(MouseButtons.Left, 1, 0, 0, 0)); // Simulate a left mouse click
        }

        /// <summary>
        /// Relocates a mine from its current position (i, j) to a new empty tile,
        /// ensuring that the new position is not adjacent to (originX, originY).
        /// </summary>
        private void relocateMine(int i, int j, int originX, int originY)
        {
            Debug.WriteLine("Relocating mine at " + i.ToString() + " " + j.ToString());
            // Find a new location for the mine
            for (int x = 0; x < mineField.GetLength(0); x++)
            {
                for (int y = 0; y < mineField.GetLength(1); y++)
                {
                    if((originX-1 <= x && x <= originX+1) && (originY-1 <= y && y <= originY+1)) // Don't relocate to a tile adjacent to original position
                        continue;
                    if (mineField[x, y] == 0) // Find an empty tile
                    {
                        Debug.WriteLine("Moving mine to " + x.ToString() + " " + y.ToString());
                        mineField[x, y] = 9; // Place the mine there
                        mineField[i, j] = 0; // Remove the mine from the original location
                        return;
                    }
                }
            }
        }

        private void flagTile(object? sender, MouseEventArgs e)
        {;
            if (sender is not Button clickedTile)
                return;

            if (clickedTile.Image == flagTileBitmap)
            {
                // If already flagged, unflag and make clickable
                clickedTile.Image = coveredTileBitmap;
                clickedTile.MouseClick += Tile_Click;
                flaggedCount--;
                updateRemainingMines();
            }
            else if(clickedTile.Image == coveredTileBitmap)
            {
                // If not flagged, flag it and remove click event handler
                clickedTile.Image = flagTileBitmap;
                clickedTile.MouseClick -= Tile_Click;
                flaggedCount++;
                updateRemainingMines();
            }
            else
            {
                return; // If the tile is already uncovered, do nothing
            }

            checkIfWon();

        }

        private void showFrontier(object sender, EventArgs e) {
            
        }

        private void Tile_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                flagTile(sender, e); // Handle right click to flag or unflag the tile
            }
            else if (e.Button == MouseButtons.Left)
            {
                Tile_Click(sender, e); // Handle left click normally
            }
        }

        private async void loseGame(int clickedX, int clickedY)
        {
            SoundPlayer explosionSound = new SoundPlayer(Properties.Resources.explosion);
            explosionSound.Play();

            // Make pepe cry
            Button restartButton = upperBarPanel.GetControlFromPosition(1, 0) as Button;
            if (restartButton != null)
            {
                restartButton.Image = pepeHandsBitmap;
            }

            // Make all buttons unclickable
            for (int i=0; i < mineField.GetLength(0); i++)
            {
                for (int j = 0; j < mineField.GetLength(1); j++)
                {
                    Button tile = mineGrid.GetControlFromPosition(j, i) as Button;
                    if (tile != null)
                    {
                        tile.MouseClick -= Tile_Click; // Remove click event handler
                        tile.MouseUp -= Tile_MouseUp; // Remove mouse up event handler
                        tile.MouseUp -= unscarePepe;
                        tile.MouseDown -= scarePepe;

                        if (mineField[i, j] == 9 && i != clickedX && j != clickedY) // If it's a mine, show it
                        {
                            tile.Image = mineTileBitmap;
                        }
                    }
                }
            }

            var originalLocation = this.Location;
            var rng = new Random();
            int shakeAmplitude = 10;
            int shakeDurationMs = 750;
            int shakeIntervalMs = 20;
            int elapsed = 0;

            for (; elapsed < shakeDurationMs; elapsed += shakeIntervalMs)
            {
                int offsetX = rng.Next(-shakeAmplitude, shakeAmplitude + 1);
                int offsetY = rng.Next(-shakeAmplitude, shakeAmplitude + 1);
                this.Location = new Point(originalLocation.X + offsetX, originalLocation.Y + offsetY);
                await Task.Delay(shakeIntervalMs);
            }
            this.Location = originalLocation;
        }

        private void checkIfWon() {
            if (hasWon)
            {
                return;
            }

            int markedTiles = 0;
            int correctFlags = 0;

            int mineFieldSizeX = mineField.GetLength(0);
            int mineFieldSizeY = mineField.GetLength(1);

            // Make all buttons unclickable
            for (int i = 0; i < mineFieldSizeX; i++)
            {
                for (int j = 0; j < mineFieldSizeY; j++)
                {
                    Button tile = mineGrid.GetControlFromPosition(j, i) as Button;

                    // Count number of uncovered / flagged tiles
                    if (uncoveredTileBitmaps.Contains(tile.Image) || tile.Image == flagTileBitmap)
                    {
                        markedTiles++;
                    }


                    // Count number of correctly flagged tiles
                    if (tile.Image == flagTileBitmap && mineField[i,j] == 9)
                    {
                        correctFlags++;
                    }
                }
            }

            if (correctFlags == mineCount && markedTiles == mineFieldSizeX*mineFieldSizeY)
            { // Win 6
                hasWon = true;

                SoundPlayer winSound = new SoundPlayer(Properties.Resources.win);
                winSound.Play();

                // Make pepe cry upon loss
                Button restartButton = upperBarPanel.GetControlFromPosition(1, 0) as Button;
                if (restartButton != null)
                {
                    restartButton.Image = pepegaBitmap;
                }

                // Make all buttons unclickable
                for (int i = 0; i < mineField.GetLength(0); i++)
                {
                    for (int j = 0; j < mineField.GetLength(1); j++)
                    {
                        Button tile = mineGrid.GetControlFromPosition(j, i) as Button;
                        tile.MouseClick -= Tile_Click;
                        tile.MouseUp -= Tile_MouseUp;
                        tile.MouseUp -= unscarePepe;
                        tile.MouseDown -= scarePepe;                 
                    }
                }

            }

        }

        // Helper method
        void ResizeButtonToFitText(Button button)
        {
            Size textSize = TextRenderer.MeasureText(button.Text, button.Font);
            button.Size = new Size(textSize.Width + 20, textSize.Height + 10); // Adjust padding as needed
        }

        private void setRandomSeed_Click(object sender, EventArgs e) {
            // Generate a random seed and update the game seed
            Random rng = new Random();
            gameSeed = rng.Next(1, 999999999); // Generate a random seed between 1 and int.MaxValue
            MessageBox.Show($"Random seed set to: {gameSeed}");
            restartGame(sender, e);
        }

        private void setSpecificSeed_Click(object sender, EventArgs e) {
            int? seed = ShowSeedInputDialogue("Enter a seed value:");

            if (!seed.HasValue)
            {
                // User cancelled or invalid input
                MessageBox.Show("Seed entry cancelled or invalid (Only 1 <-> 9 digit integers allowed)");
            }

            else if (seed.Value <= 999999999)
            {
                // Use the seed (seed.Value)
                MessageBox.Show($"Seed set to: {seed.Value}");
                gameSeed = seed.Value; // Update the game seed
                restartGame(sender, e);
            }
            else
            {
                // User cancelled or invalid input
                MessageBox.Show("Only integers up to 9 digits in length permitted");
            }
        }

        private int? ShowSeedInputDialogue(string prompt)
        {
            Form inputForm = new Form()
            {
                Width = 300,
                Height = 150,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                Text = "Set Specific Seed",
                StartPosition = FormStartPosition.CenterParent,
                MinimizeBox = false,
                MaximizeBox = false
            };

            Label textLabel = new Label() { Left = 10, Top = 10, Text = prompt, AutoSize = true };
            TextBox inputBox = new TextBox() { Left = 10, Top = 35, Width = 260 };
            Button okButton = new Button() { Text = "OK", Left = 115, Width = 75, Top = 70, DialogResult = DialogResult.OK };
            Button cancelButton = new Button() { Text = "Cancel", Left = 195, Width = 75, Top = 70, DialogResult = DialogResult.Cancel };

            okButton.Click += (sender, e) => { inputForm.Close(); };
            cancelButton.Click += (sender, e) => { inputForm.Close(); };

            inputForm.Controls.Add(textLabel);
            inputForm.Controls.Add(inputBox);
            inputForm.Controls.Add(okButton);
            inputForm.Controls.Add(cancelButton);
            inputForm.AcceptButton = okButton;
            inputForm.CancelButton = cancelButton;

            DialogResult result = inputForm.ShowDialog();

            if (result == DialogResult.OK && int.TryParse(inputBox.Text, out int seedValue))
            {
                return seedValue;
            }

            return null; // Cancelled or invalid
        }

        private void tableLayoutPanel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {

        }

        private void richTextBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void upperBarPanel_Paint(object sender, PaintEventArgs e)
        {

        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {

        }
    }
}
