using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.UI.Xaml;


namespace Game
{

    // This class controls and draws the player object.
    public class Player
    {
        private int x, y, direction;

        public int Score { get; set; }
        public int NewDirection { get; set; }

        // Call this method to set starting position and the direction that the baddie will start going in.
        public Player(int start_x, int start_y, int start_direction)
        {
            x = start_x;
            y = start_y;
            direction = start_direction;
        }


        public void Move(Maze gamemaze)
        {
            switch (direction)
            {
                case 1: y -= 8; break;
                case 2: x += 8; break;
                case 4: y += 8; break;
                case 8: x -= 8; break;

                default: break;
            }

            // Test to see if the player is in the middle of a square in the maze, and then we can decide
            // to change direction or not depending on whether a key is pressed or if they've run into a wall.
            // It turns out this is a little trickier than you might expect - the player might be trying to 
            // turn in a direction that isn't good just yet, but will be soon - so we need to keep trying without
            // stopping them until it's good to go.

            if ((x % 64 == 0) && (y % 64 == 0))
            {
                // Map the screen co-ordinates to the map co-ordinates 
                // (the screen is 1024 by 1024, the maze data is 16 by 16)

                int mx = (x / 64);
                int my = (y / 64);

                // Check for a dot being eaten
                if ((gamemaze.GetTile(my, mx) & 16) != 0)
                {
                    gamemaze.SetTile(my, mx, gamemaze.GetTile(my, mx) - 16);
                    Score += 1;
                }


                // Get the number that tells us the possible directions at this tile
                int possible_directions = gamemaze.GetTile(my, mx);

                int current_direction = direction;


                // If the player has pressed a key, let's consider that a request to change direction.
                // We can check if it's possible because we know for any tile what directions are available.
                // If the player cannot change in that direction, they'll just stop.

                if (NewDirection == 0)
                {
                    // The player hasn't requested a change in direction
                    // So let's see if they can move in the current direction

                    if ((direction & possible_directions) == 0)
                    {
                        // No, the player would hit a wall. Better stop!
                        direction = 0;
                    }
                }
                else // The player has requested a change in direction
                {
                    // Is it ok?
                    if ((NewDirection & possible_directions) != 0)
                    {
                        // yes, it's allowed
                        direction = NewDirection;
                    }
                    else
                    {
                        // No, player can't change direction in that way, but we can't just stop
                        // because the current direction might be good.  

                        if ((current_direction & possible_directions) == 0)
                        {
                            // No, carrying on isn't allowed! Better stop!
                            direction = 0;
                        }
                    }
                }
            }
        }

        // Draw the player. Needs a reference to the canvas
        public void Draw(CanvasDrawEventArgs args, CanvasBitmap ninjacat)
        {
            args.DrawingSession.DrawImage(ninjacat, x, y);
        }

        public bool Check(Baddie badguy)
        {
            // Check the distance between the player and a Baddie
            if ((((x - badguy.x) * (x - badguy.x)) < 256) && (((y - badguy.y) * (y - badguy.y)) < 256))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

    }
}
