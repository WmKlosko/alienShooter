using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using System.Windows.Threading;

namespace alienGameWPF
{
 
    public partial class MainWindow : Window
    {

        DispatcherTimer gameTimer = new DispatcherTimer();
        bool moveLeft, moveRight;
        List<Rectangle> itemRemover = new List<Rectangle>();

        Random rand = new Random();

        int enemySpriteCounter = 0;
        int enemyCounter = 100;
        int playerSpeed = 12;
        int limit = 50;
        int score = 0;
        int damage = 0;
        int enemySpeed = 10;
        int backgroundHitCounter = 0;
        int playerHitCounter = 0;
        int frenCounter = 0;

        Rect playerHitBox;

               ImageBrush bg = new ImageBrush();

        public MainWindow()
        {
            InitializeComponent();

            gameTimer.Interval = TimeSpan.FromMilliseconds(20);
            gameTimer.Tick += GameLoop;
            gameTimer.Start();

            MyCanvas.Focus();

            setBackground(MyCanvas, "pack://application:,,,/Obrazki/tlo.png");
            // ImageBrush bg = new ImageBrush();

            //  bg.ImageSource = new BitmapImage(new Uri("pack://application:,,,/Obrazki/tlo.png"));
            //  bg.TileMode = TileMode.Tile;
            // bg.Viewport = new Rect(0, 0, 0.15, 0.15);
            // bg.ViewportUnits = BrushMappingMode.RelativeToBoundingBox;
            // MyCanvas.Background = bg;

            setPlayerImage(player, "pack://application:,,,/Obrazki/gracz.png");

        }

        // Funkcja przyjmuje obiekt canvas i ustawia jego tlo na podstawie podanego uri do obrazka
        private void setBackground(Canvas canvas, string uri)
        {
            ImageBrush bg = new ImageBrush();
            bg.ImageSource = new BitmapImage(new Uri(uri));
            bg.TileMode = TileMode.Tile;
            bg.Viewport = new Rect(0, 0, 0.15, 0.15);
            bg.ViewportUnits = BrushMappingMode.RelativeToBoundingBox;
            canvas.Background = bg;
        }

        // Funkcja przyjmuje obiekt Ractangle reprezentujacy statek gracza i zmienia jego grafike na podstawie podanego uri
        private void setPlayerImage(Rectangle player, string uri)
        {
            ImageBrush playerImage = new ImageBrush();
            playerImage.ImageSource = new BitmapImage(new Uri(uri));
            player.Fill = playerImage;
        }

        private void GameLoop(object sender, EventArgs e)
        {

            // Sprawdzanie wyswietlania czerwonego tla
            if(backgroundHitCounter > 0)
            {
                backgroundHitCounter -= 1;
            } 
            else 
            {
                setBackground(MyCanvas, "pack://application:,,,/Obrazki/tlo.png");
            }

            // Sprawdzanie wyswietlania uszkodzenia statku
            if (playerHitCounter > 0)
            {
                playerHitCounter -= 1;
            }
            else
            {
                setPlayerImage(player, "pack://application:,,,/Obrazki/gracz.png");
            }

            // Wypuszczanie leczacych statkow
            if(frenCounter > 200)
            {
                frenCounter = 0;
                MakeFrens();
            } else
            {
                frenCounter += 1;
            }

            playerHitBox = new Rect(Canvas.GetLeft(player), Canvas.GetTop(player), player.Width, player.Height);

            enemyCounter -= 1;

            scoreText.Content = "Wynik: " + score;
            damageText.Content = "Obrazenia " + damage + " / 100";

            if (enemyCounter < 0)
            {
                MakeEnemies();
                enemyCounter = limit;
            }    

            if (moveLeft == true && Canvas.GetLeft(player) > 0)
            {
                Canvas.SetLeft(player, Canvas.GetLeft(player) - playerSpeed);
            }

            if (moveRight == true && Canvas.GetLeft(player) + 90 < Application.Current.MainWindow.Width)
            {
                Canvas.SetLeft(player, Canvas.GetLeft(player) + playerSpeed);
            }

            foreach (var x in MyCanvas.Children.OfType<Rectangle>())
            {
                if (x is Rectangle && (string)x.Tag == "bullet")
                {
                    Canvas.SetTop(x, Canvas.GetTop(x) - 20);

                    Rect bulletHitBox = new Rect(Canvas.GetLeft(x), Canvas.GetTop(x), x.Width, x.Height);

                    //usuwanie zbędnych pocisków
                    if (Canvas.GetTop(x) < 10)
                    {
                        itemRemover.Add(x);
                    }

                    foreach (var y in MyCanvas.Children.OfType<Rectangle>())
                    {
                        if (y is Rectangle && (string)y.Tag == "enemy")
                        {
                            Rect enemyHit = new Rect(Canvas.GetLeft(y), Canvas.GetTop(y), y.Width, y.Height);

                            if (bulletHitBox.IntersectsWith(enemyHit))
                            {
                                itemRemover.Add(x);
                                itemRemover.Add(y);
                                score++;
                            }
                        }
                    }

                }

                if (x is Rectangle && (string)x.Tag == "enemy")
                {
                    Canvas.SetTop(x, Canvas.GetTop(x) + enemySpeed);

                    //przepuszczenie wroga
                    if (Canvas.GetTop(x) > 550)
                    {

                        if (backgroundHitCounter == 0)
                        {
                            backgroundHitCounter = 2;
                            setBackground(MyCanvas, "pack://application:,,,/Obrazki/tlo_hit.png");
                        }
                        itemRemover.Add(x);
                        damage += 10;

                    }

                    Rect enemyHitBox = new Rect(Canvas.GetLeft(x), Canvas.GetTop(x), x.Width, x.Height);

                    //zderzenie z graczem
                    if (playerHitBox.IntersectsWith(enemyHitBox))
                    {

                        if (playerHitCounter == 0)
                        {
                            playerHitCounter = 2;
                            setPlayerImage(player, "pack://application:,,,/Obrazki/graczHit.png");
                        }

                        itemRemover.Add(x);
                        damage += 20;
                    }
                }

                if (x is Rectangle && (string)x.Tag == "fren")
                {
                    Canvas.SetTop(x, Canvas.GetTop(x) + enemySpeed - (enemySpeed*0.2));

                    //przepuszczenie wroga
                    if (Canvas.GetTop(x) > 550)
                    {
                        itemRemover.Add(x);
                    }

                    Rect frenHitBox = new Rect(Canvas.GetLeft(x), Canvas.GetTop(x), x.Width, x.Height);

                    //zderzenie z graczem
                    if (playerHitBox.IntersectsWith(frenHitBox))
                    {

                        if (playerHitCounter == 0)
                        {
                            playerHitCounter = 2;
                            setPlayerImage(player, "pack://application:,,,/Obrazki/graczHeal.png");
                        }

                        itemRemover.Add(x);
                        if ((damage - 20) > 0)
                        {
                            damage -= 10;
                        } else
                        {
                            damage = 0;
                        }
                       
                    }
                }
            }

            foreach (Rectangle i in itemRemover)
            {
                MyCanvas.Children.Remove(i);
            }    

            if(score > 5)
            {
                limit = 24;
                enemySpeed = 13;
            }
            if (score > 10)
            {
                limit = 25;
                enemySpeed = 14;
            }
            if (score > 20)
            {
                limit = 25;
                enemySpeed = 15;
            }
            if (score > 30)
            {
                limit = 25;
                enemySpeed = 17;
            }
            if (score > 50)
            {
                limit = 25;
                enemySpeed = 20;
            }
            if (score > 75)
            {
                limit = 28;
                enemySpeed = 22;
            }
            if (score > 100)
            {
                limit = 30;
                enemySpeed = 24;
            }
            if (score > 120)
            {
                limit = 40;
                enemySpeed = 30;
            }

            if (damage > 99)
            {
                setPlayerImage(player, "pack://application:,,,/Obrazki/GraczEnd.png");
                gameTimer.Stop();
                damageText.Content = "Obrazenia: 100 / 100";
                damageText.Foreground = Brushes.Red;
                if (score < 10)
                {
                    scoreText.Foreground = Brushes.Red;
                    MessageBox.Show("Slabo Ci poszlo. Twoj wynik to: " + score + Environment.NewLine + "Kliknij OK zeby poprawic swoje umiejetnosci!", "<<<Transmisja>>>");

                    System.Diagnostics.Process.Start(Application.ResourceAssembly.Location);
                    Application.Current.Shutdown();
                }

                if (score >= 10 && score < 20)
                {
                    scoreText.Foreground = Brushes.Orange;
                    MessageBox.Show("Lacznie zestrzeliles " + score + " statkow obcych" + Environment.NewLine + "Nacisnij OK, zeby sprobowac jeszcze raz!", "<<<Transmisja>>>");

                    System.Diagnostics.Process.Start(Application.ResourceAssembly.Location);
                    Application.Current.Shutdown();
                }

                if (score >= 20 && score < 30)
                {
                    scoreText.Foreground = Brushes.Yellow;
                    MessageBox.Show("Dobry wynik! Zniszczyles " + score + " wrogich pojazdow" + Environment.NewLine + "Sprobuj jeszcze raz naciskajac OK!", "<<<Transmisja>>>");

                    System.Diagnostics.Process.Start(Application.ResourceAssembly.Location);
                    Application.Current.Shutdown();
                }

                if (score >= 30 && score < 50)
                {
                    scoreText.Foreground = Brushes.Green;
                    MessageBox.Show("Bardzo dobry wynik! Zestrzeliles lacznie " + score + " statkow obcych." + Environment.NewLine + "Mozesz sprobowac zestrzelic jeszcze wiecej statkow klikajac OK.", "<<<Transmisja>>>");

                    System.Diagnostics.Process.Start(Application.ResourceAssembly.Location);
                    Application.Current.Shutdown();
                }
                if (score >= 50)
                {
                    scoreText.Foreground = Brushes.Green;
                    MessageBox.Show("Niesamowite! Zniszczyles az " + score + " statkow obcych" + Environment.NewLine + "Mozesz grac dalej po nacisnieciu OK", "<<<Transmisja>>>");

                    System.Diagnostics.Process.Start(Application.ResourceAssembly.Location);
                    Application.Current.Shutdown();
                }

            }


        }


        private void OnKeyDown(object sender, KeyEventArgs e)
        {

            if (e.Key == Key.Left)
            {
                moveLeft = true;
            }
            if (e.Key == Key.Right)
            {
                moveRight = true;
            }

        }
        private void OnKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Left)
            {
                moveLeft = false;
            }
            if (e.Key == Key.Right)
            {
                moveRight = false;
            }

            // Tworzenie pocisku (lasera)
            if (e.Key == Key.Space)
            {
                Rectangle newBullet = new Rectangle
                {
                    Tag = "bullet",
                    Height = 20,
                    Width = 5,
                    Fill = Brushes.White,
                    Stroke = Brushes.Blue
                };

                Canvas.SetLeft(newBullet, Canvas.GetLeft(player) + player.Width / 2);
                Canvas.SetTop(newBullet, Canvas.GetTop(player) - newBullet.Height);
                MyCanvas.Children.Add(newBullet);

            }
        }

        private void MakeFrens()
        {
            ImageBrush frenSprite = new ImageBrush();
            frenSprite.ImageSource = new BitmapImage(new Uri("pack://application:,,,/Obrazki/fren.png"));

            Rectangle newFren = new Rectangle
            {
                Tag = "fren",
                Height = 50,
                Width = 56,
                Fill = frenSprite
            };

            Canvas.SetTop(newFren, -100);
            Canvas.SetLeft(newFren, rand.Next(30, 430));
            MyCanvas.Children.Add(newFren);

        }

        private void MakeEnemies()
        {
            ImageBrush enemySprite = new ImageBrush();

            enemySpriteCounter = rand.Next(1, 6);

            switch (enemySpriteCounter)
            {
                case 1:
                    enemySprite.ImageSource = new BitmapImage(new Uri("pack://application:,,,/Obrazki/statek1.png"));
                    break;
                case 2:
                    enemySprite.ImageSource = new BitmapImage(new Uri("pack://application:,,,/Obrazki/statek2.png"));
                    break;
                case 3:
                    enemySprite.ImageSource = new BitmapImage(new Uri("pack://application:,,,/Obrazki/statek3.png"));
                    break;
                case 4:
                    enemySprite.ImageSource = new BitmapImage(new Uri("pack://application:,,,/Obrazki/statek4.png"));
                    break;
                case 5:
                    enemySprite.ImageSource = new BitmapImage(new Uri("pack://application:,,,/Obrazki/statek5.png"));
                    break;
                case 6:
                    enemySprite.ImageSource = new BitmapImage(new Uri("pack://application:,,,/Obrazki/statek6.png"));
                    break;
            }

            Rectangle newEnemy = new Rectangle
            {
                //tworzenie wrogów
                Tag = "enemy",
                Height = 50,
                Width = 56,
                Fill = enemySprite
            };

            Canvas.SetTop(newEnemy, -100);
            Canvas.SetLeft(newEnemy, rand.Next(30, 430));
            MyCanvas.Children.Add(newEnemy);

        }
    }
}
