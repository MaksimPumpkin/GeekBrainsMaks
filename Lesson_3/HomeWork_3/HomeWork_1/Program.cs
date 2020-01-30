using System;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Drawing;
using System.Media;

/// <summary>
/// Выполнил задания 1, 3-4.
/// 
/// Сделал возможность выпускать несколько пуль (с помощью кода из методички пуля могла существовать лишь в одном экземпляре) с помощью коллекции.
/// Вопрос: Верен ли мой способ? Можно ли сделать проще?
/// </summary>
namespace HomeWork_3_Maksim_Galkin
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
		public static List<Bullet> bullet = new List<Bullet>(3); // Коллекция для пуль.
		public static Ship ship = new Ship(new Point(10,400), new Point(20,20), new Size(50,25));
		public static int score = 0;
		private static Timer timer = new Timer() { Interval = 100};
		public static Random rnd = new Random();
		public static HealBoost heal;

		public static int Width { get; set; }
		public static int Height { get; set; }

		static Game()
		{

		}

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

			timer.Start();

			timer.Tick += Timer_Tick;
			form.KeyDown += Form_KeyDown;
			Ship.MessageDie += Finish;
		}

		private static void Form_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.ControlKey) bullet.Add(new Bullet(new Point(ship.Rect.X + 10, ship.Rect.Y + 4), new Point(10, 0), new Size(4, 1))); // Добавление новой пули в коллекцию
			if (e.KeyCode == Keys.Up) ship.Up();
			if (e.KeyCode == Keys.Down) ship.Down();
		}

		private static void Timer_Tick(object sender, EventArgs e)
		{
			Draw();
			Update();
		}

		public static void Load()
		{
			asteroid = new Asteroid[3];
			_obj = new Star[30];

			for (int i = 0; i < _obj.Length; i++)
			{
				int r = rnd.Next(5, 50);
				_obj[i] = new Star(new Point(1000, rnd.Next(0, Height)), new Point(-r, r), new Size(5, 5));
			}

			for (int i = 0; i < asteroid.Length; i++)
			{
				int r = rnd.Next(25, 80);
				asteroid[i] = new Asteroid(new Point(Width, rnd.Next(0, Height)), new Point(-r/5, r), new Size(r, r));
			}

			for (int i = 0; i < bullet.Count; i++)
			{
				bullet[i] = null;
			}

			heal = new HealBoost(new Point(Width, rnd.Next(0, Height)), new Point(10, 0), new Size(50, 50));

			planet = new Planet(new Point(600, 20), new Point(10, 0), new Size(100, 100));
		}

		/// <summary>
		/// Задание 4: Ведётся подсчёт сбитых астероидов в переменную score, значение которой выводится на экран
		/// </summary>
		public static void Draw()
		{
			Buffer.Graphics.Clear(Color.Black);

			foreach (Star obj in _obj) obj.Draw();

			foreach (Asteroid a in asteroid) a?.Draw();

			for (int i = 0; i<bullet.Count; i++) bullet[i]?.Draw();

			ship?.Draw();

			if (ship != null) Buffer.Graphics.DrawString($"Energy: {ship.Energy}", SystemFonts.DefaultFont, Brushes.White, 0, 0);
			if (ship != null) Buffer.Graphics.DrawString($"Score: {score}", SystemFonts.DefaultFont, Brushes.White, 0, 50);

			heal.Draw();

			planet.Draw();

			Buffer.Render();
		}

		public static void Update()
		{
			foreach (Star obj in _obj) obj.Update();

			for (int i = 0; i < bullet.Count; i++) bullet[i]?.Draw();

			heal.Update();
			if (ship.Collision(heal)) ship?.EnergyUp(rnd.Next(1, 10));

			for (int i = 0; i < asteroid.Length; i++)
			{
				if (asteroid == null) continue;
				asteroid[i].Update();
				for (int j = 0; j < bullet.Count; j++)
				{
					if (bullet[j] == null) continue;
					bullet[j].Update();
					if (bullet[j] != null && bullet[j].Collision(asteroid[i]))
					{
						int r = rnd.Next(25, 80);
						SystemSounds.Hand.Play();
						bullet.Remove(bullet[j]); // Удаление пули из коллекции при столкновении с астероидом
						asteroid[i] = new Asteroid(new Point(Width, rnd.Next(0, Height)), new Point(-r, r), new Size(r, r));
						score++;
						continue;
					}
					if (bullet[j].Chek()) bullet.Remove(bullet[j]); // Удаление пули из коллекции при достижении конца экрана (проверка методом Chek())
				}
				if (!ship.Collision(asteroid[i])) continue;
				ship?.EnergyLow(rnd.Next(1, 10));
				SystemSounds.Asterisk.Play();
				if (ship.Energy <= 0) ship.Die();
			}

			planet.Update();
		}

		public static void Finish()
		{
			timer.Stop();
			Buffer.Graphics.DrawString("The End", new Font(FontFamily.GenericSansSerif, 60, FontStyle.Underline), Brushes.White, Width/2, Height/2);
			Buffer.Render();
		}
	}

	abstract class BaseObject : ICollision
	{
		protected Point Pos;
		protected Point Dir;
		protected Size Size;
		public delegate void Message();

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
			Game.Buffer.Graphics.DrawEllipse(Pens.Blue, Pos.X, Pos.Y, Size.Width, Size.Height);
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

	class Asteroid : BaseObject, ICloneable, IComparable<Asteroid>
	{
		public int Power { get; set; } = 3;
		public Asteroid(Point pos, Point dir, Size size) : base(pos, dir, size)
		{
			Power = 1;
		}

		public object Clone()
		{
			return new Asteroid(new Point(Pos.X, Pos.Y), new Point(Dir.X, Dir.Y), new Size(Size.Width, Size.Height)) { Power = Power };
		}

		public override void Draw()
		{
			Game.Buffer.Graphics.FillEllipse(Brushes.White, Pos.X, Pos.Y, Size.Width, Size.Height);
		}

		int IComparable<Asteroid>.CompareTo(Asteroid obj)
		{
				if (Power > obj.Power) return 1;
				if (Power < obj.Power) return -1;
				else return 0;
		}

		public override void Update()
		{
			Pos.X = Pos.X - Dir.X;
			if (Pos.X > Game.Width || Pos.X < 0) Dir.X = -Dir.X;
			if (Pos.X < 0) Pos.X = Game.Width;
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
			Pos.X = Pos.X + Dir.X;
		}

		public bool Chek()
		{
			if (Pos.X > Game.Width) return true;
			else return false;
		}
	}

	/// <summary>
	/// Задание 1: Добавил корабль по описанию урока, но изменил форму, цвет и размеры.
	/// </summary>
	class Ship: BaseObject
	{
		private int _energy = 100;
		public int Energy => _energy;
		public static event Message MessageDie;

		public void EnergyLow(int n)
		{
			_energy -= n;
		}

		public void EnergyUp(int n)
		{
			_energy += n;
		}

		public Ship(Point pos, Point dir, Size size) : base(pos, dir, size)
		{

		}

		public override void Draw()
		{
			Game.Buffer.Graphics.FillRectangle(Brushes.Green, Pos.X, Pos.Y, Size.Width, Size.Height);
		}

		public override void Update()
		{
			
		}

		public void Up()
		{
			if (Pos.Y > 0) Pos.Y = Pos.Y - Dir.Y;
 		}

		public void Down()
		{
			if (Pos.Y < Game.Height) Pos.Y = Pos.Y + Dir.Y;
		}

		public void Die()
		{
			MessageDie?.Invoke();
		}
	}

	/// <summary>
	/// Задание 3: Аптечка, при соприкосновении с которой корабль восстанавливает энергию. 
	/// </summary>
	class HealBoost: BaseObject
	{
		public HealBoost(Point pos, Point dir, Size size) : base(pos, dir, size)
		{

		}

		public override void Draw()
		{
			Game.Buffer.Graphics.FillEllipse(Brushes.Red, Pos.X, Pos.Y, Size.Width, Size.Height);
		}

		public override void Update()
		{
			Pos.X = Pos.X - Dir.X;
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
