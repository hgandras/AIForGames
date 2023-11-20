using System;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;

namespace Prong
{
    class Program : GameWindow
    {
        float ballX = 0;
        float ballY = 0;
        int gridCellSize = 10;
        int paddleWidthCells = 2;
        int paddleHeightCells = 10;
        float ballSpeed = 500;
        float ballVelocityX = 400;
        float ballVelocityY = 300;
        float ballYDirection = 1;
        float plr1PaddleY = 0;
        float plr2PaddleY = 0;
        int plr1Score = 0;
        int plr2Score = 0;
        float paddle1Speed = 500;
        float paddle2Speed = 500;
        
        float clamp(float value, float min, float max)
        {
            return Math.Max(min, Math.Min(max, value));
        }

        float plr1PaddleBallBounceVelocityY()
        {
            float up = plr1PaddleY - paddleHeight() / 2.0f;
            float down = plr1PaddleY + paddleHeight() / 2.0f;
            float ball = clamp(ballY, up, down);
            float ratio = (ball - up) / (down - up);
            ratio = 2 * (ratio - 0.5f);
            ratio = clamp(ratio, -0.9f, 0.9f);
            return ratio;
        }

        float plr2PaddleBallBounceVelocityY()
        {
            float up = plr2PaddleY - paddleHeight() / 2.0f;
            float down = plr2PaddleY + paddleHeight() / 2.0f;
            float ball = clamp(ballY, up, down);
            float ratio = (ball - up) / (down - up);
            ratio = 2 * (ratio - 0.5f);
            ratio = clamp(ratio, -0.9f, 0.9f);
            return ratio;
        }

        void bounceBall(float ratio, float xDir)
        {
            ballVelocityY = ballSpeed * ratio;
            ballVelocityX = (float)Math.Sqrt(ballSpeed * ballSpeed - ballVelocityY * ballVelocityY);
            ballVelocityX *= xDir;
            ballYDirection = 1;
        }

        int plr1PaddleBounceX()
        {
            return -ClientSize.Width / 2 + paddleWidth() / 2;
        }

        int plr2PaddleBounceX()
        {
            return ClientSize.Width / 2 - paddleWidth() / 2;
        }

        int paddleWidth()
        {
            return gridCellSize * paddleWidthCells;
        }

        int paddleHeight()
        {
            return gridCellSize * paddleHeightCells;
        }

        bool ballFlyingRight()
        {
            return ballVelocityX > 0;
        }

        bool ballHitsRightPaddle()
        {
            return ballX + gridCellSize / 2 > plr2PaddleBounceX() - paddleWidth() / 2
                && ballY - gridCellSize / 2 < plr2PaddleY + paddleHeight() / 2
                && ballY + gridCellSize / 2 > plr2PaddleY - paddleHeight() / 2;
        }

        bool ballPastPlayer2Edge()
        {
            return ballX > ClientSize.Width / 2;
        }

        bool ballHitsLeftPaddle()
        {
            return ballX - gridCellSize / 2 < plr1PaddleBounceX() + paddleWidth() / 2
                && ballY - gridCellSize / 2 < plr1PaddleY + paddleHeight() / 2
                && ballY + gridCellSize / 2 > plr1PaddleY - paddleHeight() / 2;
        }

        bool ballPastPlayer1Edge()
        {
            return ballX < -ClientSize.Width / 2;
        }

        bool ballHitsTop()
        {
            return ballY + gridCellSize / 2 > ClientSize.Height / 2;
        }

        bool ballHitsBottom()
        {
            return ballY - gridCellSize / 2 < -ClientSize.Height / 2;
        }

        void moveBall(float timeDelta)
        {
            ballX = ballX + ballVelocityX * timeDelta;
            ballY = ballY + ballVelocityY * ballYDirection * timeDelta;
        }

        void resetBall()
        {
            ballX = 0;
            ballY = 0;
            ballVelocityX = 300;
            ballVelocityY = 400;
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            float timeDelta = (float)e.Time;
            moveBall(timeDelta);

            if (ballFlyingRight())
            {
                if (ballHitsRightPaddle())
                {
                    bounceBall(plr2PaddleBallBounceVelocityY(), -1);
                }
                if (ballPastPlayer2Edge())
                {
                    plr1Score += 1;
                    Console.WriteLine($"Player score 1: {plr1Score}");
                    resetBall();
                    return;
                }
            } else
            {
                if (ballHitsLeftPaddle())
                {
                     bounceBall(plr1PaddleBallBounceVelocityY(), 1);
                }
                if (ballPastPlayer1Edge())
                {
                    plr2Score += 1;
                    Console.WriteLine($"Player score 2: {plr2Score}");
                    resetBall();
                    return;
                }
            }

            if (ballVelocityY * ballYDirection > 0 && ballHitsTop())
            {
                ballYDirection *= -1;
            }

            if (ballVelocityY * ballYDirection <= 0 && ballHitsBottom())
            {
                ballYDirection *= -1;
            }


            if (Keyboard.GetState().IsKeyDown(Key.W))
            {
                plr1PaddleY = plr1PaddleY + paddle1Speed * timeDelta;
            }

            if (Keyboard.GetState().IsKeyDown(Key.S))
            {
                plr1PaddleY = plr1PaddleY - paddle1Speed * timeDelta;
            }

            if (Keyboard.GetState().IsKeyDown(Key.Up))
            {
                plr2PaddleY = plr2PaddleY + paddle2Speed * timeDelta;
            }

            if (Keyboard.GetState().IsKeyDown(Key.Down))
            {
                plr2PaddleY = plr2PaddleY - paddle2Speed * timeDelta;
            }
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            GL.Viewport(0, 0, ClientSize.Width, ClientSize.Height);

            Matrix4 projection = Matrix4.CreateOrthographic(ClientSize.Width, ClientSize.Height, 0.0f, 1.0f);
            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadMatrix(ref projection);

            GL.Clear(ClearBufferMask.ColorBufferBit);

            DrawRectangle(ballX, ballY, gridCellSize, gridCellSize, 1.0f, 1.0f, 0.0f);
            DrawRectangle(plr1PaddleBounceX(), plr1PaddleY, paddleWidth(), paddleHeight(), 1.0f, 0.0f, 0.0f);
            DrawRectangle(plr2PaddleBounceX(), plr2PaddleY, paddleWidth(), paddleHeight(), 0.0f, 0.0f, 1.0f);

            SwapBuffers();
        }

        void DrawRectangle(float x, float y, int width, int height, float r, float g, float b)
        {
            GL.Color3(r, g, b);

            GL.Begin(PrimitiveType.Quads);
            GL.Vertex2(-0.5f * width + x, -0.5f * height + y);
            GL.Vertex2(0.5f * width + x, -0.5f * height + y);
            GL.Vertex2(0.5f * width + x, 0.5f * height + y);
            GL.Vertex2(-0.5f * width + x, 0.5f * height + y);
            GL.End();
        }

        static void Main()
        {
            new Program().Run();
        }
    }
}