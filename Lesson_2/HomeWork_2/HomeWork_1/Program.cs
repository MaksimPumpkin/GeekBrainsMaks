using System;
using System.Windows.Forms;
using System.Drawing;
using System.Collections.Generic;
using System.Media;

/// <summary>
/// Выполнил задания 2-4 в методах Init, Game.Update и классе BaseObject
/// </summary>
namespace HomeWork_2_Maksim_Galkin
{
	class Program
	{
		static void Main(string[] args)
		{
			Form form = new Form
			{
				Width = Screen.PrimaryScreen.Bounds.Width,
			    Height = Screen.PrimaryScreen.Bounds.Height
			};

			Game.Init(form);
			form.Show();
			Game.Draw();
			Application.Run(form);
		}
	}

	interface ICollision
	{
		bool Collision(ICollision obj);
		Rectangle Rect { get; }
	}

	static class Game
	{
		private static BufferedGraphicsContext _context;
		public static BufferedGraphics Buffer;
		public static Star[] _obj;
		public static Planet planet;
		public static Asteroid[] asteroid;
		public static Bullet bullet;

		public static int Width { get; set; }
		public static int Height { get; set; }

		static Game()
		{

		}
		/// <summary>
		/// Задание 4: При попытки задать недопустимые размеры поля генерируется ошибка ArgumentOutOfRangeException
		/// </summary>
		/// <param name="form"></param>
		public static void Init(Form form)
		{
			Graphics q;

			_context = BufferedGraphicsManager.Current;
			q = form.CreateGraphics();
			try
			{
				Width = form.ClientSize.Width;
				Height = form.ClientSize.Height;
				if ((Width < 0 || Height < 0) || (Width > form.ClientSize.Width || Height > form.ClientSize.Height)) throw new ArgumentOutOfRangeException();
			}
			catch (ArgumentOutOfRangeException)
			{
				Console.WriteLine("Получены недопустимые размеры игрового поля");
			}
			
			Buffer = _context.Allocate(q, new Rectangle(0, 0, Width, Height));
			Load();

			Timer timer = new Timer { Interval = 100 };
			timer.Start();
			timer.Tick += Timer_Tick;
		}

		private static void Timer_Tick(object sender, EventArgs e)
		{
			Draw();
			Update();
		}

		public static void Load()
		{
			asteroid = new Asteroid[3];
			bullet = new Bullet(new Point(0, 200), new Point(5, 0), new Size(4, 1));
			_obj = new Star[30];
			var rnd = new Random();

			for (int i = 0; i < _obj.Length; i++)
			{
				int r = rnd.Next(5, 50);
				_obj[i] = new Star(new Point(1000, rnd.Next(0, Height)), new Point(-r, r), new Size(5, 5));
			}

			for (int i = 0; i < asteroid.Length; i++)
			{
				int r = rnd.Next(25, 80);
				asteroid[i] = new Asteroid(new Point(rnd.Next(0, Width), rnd.Next(0, Height)), new Point(-r, r), new Size(r, r));
			}

			planet = new Planet(new Point(600, 20), new Point(10, 0), new Size(100, 100));
		}

		public static void Draw()
		{
			Buffer.Graphics.Clear(Color.Black);
			foreach (Star obj in _obj)
			{
				obj.Draw();
			}
			foreach (Asteroid a in asteroid)
			{
				a.Draw();
			}
			planet.Draw();
			bullet.Draw();
			Buffer.Render();
		}
		/// <summary>
		/// Задание 3: При столкновении с пулей астероид исчезает и появляется в новой точке.
		/// </summary>
		public static void Update()
		{
			foreach (Star obj in _obj)
			{
				obj.Update();
			}

			for (int i = 0; i < asteroid.Length; i++)
			{
				asteroid[i].Update();
				if (asteroid[i].Collision(bullet))
				{
					SystemSounds.Hand.Play();
					var rnd = new Random();
					int r = rnd.Next(25, 80);
					asteroid[i] = new Asteroid(new Point(rnd.Next(0, Width), rnd.Next(0, Height)), new Point(-r, r), new Size(r, r));
				}
			}
			planet.Update();
			bullet.Update();
		}
	}

	/// <summary>
	/// Задание 2: Теперь методы Update и Draw абстрактные и имеют уникальную реализацию в каждом наследнике базового класа.
	/// </summary>
	abstract class BaseObject : ICollision
	{
		protected Point Pos;
		protected Point Dir;
		protected Size Size;

		protected BaseObject(Point pos, Point dir, Size size)
		{
			Pos = pos;
			Dir = dir;
			Size = size;
		}

		public abstract void Draw();

		public abstract void Update();

		public bool Collision(ICollision o) => o.Rect.IntersectsWith(this.Rect);
		public Rectangle Rect => new Rectangle(Pos, Size);
	}

	class Star : BaseObject
	{
		public Star(Point pos, Point dir, Size size) : base(pos, dir, size)
		{

		}

		public override void Draw()
		{
			Image image = Image.FromFile(@"Image\star.gif");
			Game.Buffer.Graphics.DrawImage(image, Pos);
		}

		public override void Update()
		{
			Pos.X = Pos.X + Dir.X;
			if (Pos.X < 1) Pos.X = Game.Width - Size.Width;
		}
	}

	class Planet: BaseObject
	{
		public Planet(Point pos, Point dir, Size size) : base(pos, dir, size)
		{

		}

		public override void Draw()
		{
			Game.Buffer.Graphics.DrawEllipse(Pens.White, Pos.X, Pos.Y, Size.Width, Size.Height);
		}

		public override void Update()
		{
			Pos.X = Pos.X - Dir.X;
			if (Pos.X < 1)
			{
				Pos.X = Game.Width;
				Pos.Y = Pos.Y + 250;
			}
			if (Pos.Y > Game.Height) Pos.Y = 0;
		}
	}

	class Asteroid : BaseObject, ICloneable
	{
		public int Power { get; set; }
		public Asteroid(Point pos, Point dir, Size size) : base(pos, dir, size)
		{
			Power = 1;
		}

		public object Clone()
		{
			Random rnd = new Random();
			Asteroid _asteroid = new Asteroid(new Point(rnd.Next(0, Game.Width), rnd.Next(0, Game.Height)), new Point(Dir.X, Dir.Y), new Size(Size.Width, Size.Height));
			_asteroid.Power = Power;
		    return _asteroid;
		}

		public override void Draw()
		{
			Game.Buffer.Graphics.FillEllipse(Brushes.White, Pos.X, Pos.Y, Size.Width, Size.Height);
		}

		public override void Update()
		{
			Pos.X = Pos.X - Dir.X;
			if (Pos.X > Game.Width || Pos.X < 0) Dir.X = -Dir.X;
			Pos.Y = Pos.Y + Dir.Y;
			if (Pos.Y > Game.Height || Pos.Y < 0) Dir.Y = -Dir.Y;
		}
	}

	class Bullet: BaseObject
	{
		public Bullet(Point pos, Point dir, Size size) : base(pos, dir, size)
		{

		}

		public override void Draw()
		{
			Game.Buffer.Graphics.DrawRectangle(Pens.OrangeRed, Pos.X, Pos.Y, Size.Width, Size.Height);
		}

		public override void Update()
		{
			Pos.X = Pos.X + 3;
		}
	}

	class ArgumentOutOfRangeException: Exception
	{
		public ArgumentOutOfRangeException()
		{
			Console.WriteLine(base.Message);
		}
	} 
}
