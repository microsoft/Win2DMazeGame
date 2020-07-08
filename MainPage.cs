using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.UI.Xaml;
using System;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;


namespace Game
{
    public sealed partial class MainPage : Page
    {
        // The Maze Game has three components:
        // 1. The player - an image which the player can move around the maze, eating dots. Player.cs
        // 2. The Baddies - an image which roams the maze, and will catch the player if close enough. Baddie.cs
        // 3. The Maze - an array which describes the maze.

        // Graphics are displayed using the Win2D library, which provides support for quickly drawing images and other shapes.

        // A timer fires 15 times a second and on every 'tick' the player and baddies are updated.

        // The Canvas object defined in mainpage.xaml also displays a single bitmap create in a paint program that renders
        // the maze. The player and baddies objects, and the dots in the maze the player must eat, are displayed on
        // top of this single image. The Maze object itself consists of an array of 16 by 16 numbers, each which tells
        // the moving objects which directions they are free to move. 


        // The graphics for the player and the baddies
        CanvasBitmap ninjacat, dino;

        public DispatcherTimer Timer { get; private set; }

        // Declare the player object and three baddies
        private Player player = new Player(512, 512 - 64, 0);

        private Baddie Ringo = new Baddie(64, 64, 2);
        private Baddie George = new Baddie(64, 64, 2);
        private Baddie Paul = new Baddie(64, 64, 4);


        // Create the maze object
        private Maze GameMaze = new Maze();


        public MainPage()
        {
            this.InitializeComponent();

            // Set the score to zero - the game doesn't currently check that all dots are eaten!
            // But if this is 122, the player has cleared the level.
            player.Score = 0;

            // Start the timer going - this will update the display and move the characters around.
            // When the baddies catch the player, the timer is stopped. In a real game, the "game over"
            // message would be displayed.

            this.Timer = new DispatcherTimer();
            this.Timer.Interval = new TimeSpan(0, 0, 0, 0, 15);
            this.Timer.Tick += timer_Tick;
            this.Timer.Start();

        }



        private void timer_Tick(object sender, object e)
        {
            // Draw everything by making the canvas "invalid", triggering the redraw.
            canvas.Invalidate();
        }



        // This is the Draw method used by the CanvasControl. The CanvasControl gives us the ability
        // to quickly draw images, other shapes and text. 
        void canvasControl_Draw(CanvasControl sender, CanvasDrawEventArgs args)
        {
            // The screen is cleared, so we need to redraw everything (except the background, which is a separate image)

            // First, draw the dots the player needs to collect

            for (int y = 0; y < 16; y++)
                for (int x = 0; x < 16; x++)
                {
                    if ((GameMaze.GetTile(y, x) & 16) != 0)
                    {
                        args.DrawingSession.DrawEllipse(32 + x * 64, 32 + y * 64, 4, 4, Colors.Blue, 8);
                    }
                }



            // Move the player to the new position, draw it.
            // Do the same thing for the baddies.

            player.Move(GameMaze);
            player.Draw(args, ninjacat);

            Ringo.Move(4, GameMaze);  // The first parameter is the speed - it must be 1, 2, 4,8, 16 or 32
            George.Move(8, GameMaze);
            Paul.Move(8, GameMaze);

            Ringo.Draw(args, dino);
            George.Draw(args, dino);
            Paul.Draw(args, dino);


            // Now draw the score 

            args.DrawingSession.DrawText("Score: " + player.Score.ToString("0000"), 400, 20, Colors.Yellow);


            // Check to see if the player has been caught by a dino.
            // The simplest way is to check that the distance between
            // them is smaller than a specific threshold.

            if (player.Check(Ringo) || player.Check(George) || player.Check(Paul))
            {
                // Player has been caught!
                // For now, just stop the timer.
                Timer.Stop();
            }
        }



        // This is a method called by the CanvasControl, and we use it to call the routine that loads the graphics
        // for the player and baddies.
        private void canvas_CreateResources(CanvasControl sender, Microsoft.Graphics.Canvas.UI.CanvasCreateResourcesEventArgs args)
        {
            args.TrackAsyncAction(CreateResourcesAsync(sender).AsAsyncAction());
        }

        // Load the images. Only needs doing once, when the CanvasControl is initialized.
        private async Task CreateResourcesAsync(CanvasControl sender)
        {
            ninjacat = await CanvasBitmap.LoadAsync(sender, new Uri("ms-appx:///Assets/ninjacat.png"));
            dino = await CanvasBitmap.LoadAsync(sender, new Uri("ms-appx:///Assets/dino.png"));
        }

        // The CanvasControl calls this method when the keyboard is pressed. 
        private void canvas_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            int d = 0;

            switch (e.Key)
            {
                case Windows.System.VirtualKey.W: d = 1; break;
                case Windows.System.VirtualKey.D: d = 2; break;
                case Windows.System.VirtualKey.S: d = 4; break;
                case Windows.System.VirtualKey.A: d = 8; break;
                default: d = 0; break;
            }

            player.NewDirection = d;

        }

        // A 'clean up' method required by the CanvasConrol
        void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            this.canvas.RemoveFromVisualTree();
            this.canvas = null;
        }
    }
}
