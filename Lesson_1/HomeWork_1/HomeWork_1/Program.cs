using System;
using System.Windows.Forms;
using System.Drawing;

/// <summary>
/// Сделал вторую часть домашнего задания. 
/// Попытался сделать объект Planet, который должен двигаться с права налево и при достижении края окна возвращатся в правую часть, но не понял как вывести его на экран. 
/// Возникли трудности с пониманием работы методов класса Game. 
/// </summary>
namespace HomeWork_1_Maksim_Galkin
{
	class Program
	{
		static void Main(string[] args)
		{
			Form form = new Form();
			form.Width = 800;
			form.Height = 800;
			Game.Init(form);
			form.Show();
			Game.Draw();
			Application.Run(form);
		}
	}

	static class Game
	{
		private static BufferedGraphicsContext _context;
		public static BufferedGraphics Buffer;
		public static BaseObject[] _obj;

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
			Width = form.ClientSize.Width;
			Height = form.ClientSize.Height;
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
			_obj = new BaseObject[30];
			for (int i = 0; i < _obj.Length; i++)
				_obj[i] = new BaseObject(new Point(600, i * 20), new Point(-i, -i), new Size(10, 10));
			for (int i = 0; i < _obj.Length / 2; i++)
				_obj[i] = new Star(new Point(600, i * 20), new Point(-i, 0), new Size(5, 5));
		}

		public static void Draw()
		{
			Buffer.Graphics.Clear(Color.Black);
			foreach (BaseObject obj in _obj)
				obj.Draw();
			Buffer.Render();
		}

		public static void Update()
		{
			foreach (BaseObject obj in _obj)
				obj.Update();
		}
	}

	class BaseObject
	{
		protected Point Pos;
		protected Point Dir;
		protected Size Size;

		public BaseObject(Point pos, Point dir, Size size)
		{
			Pos = pos;
			Dir = dir;
			Size = size;
		}

		public virtual void Draw()
		{
			Game.Buffer.Graphics.DrawEllipse(Pens.White, Pos.X, Pos.Y, Size.Width, Size.Height);
		}

		public virtual void Update()
		{
			Pos.X = Pos.X + Dir.X;
			Pos.Y = Pos.Y + Dir.Y;
			if (Pos.X < 0) Dir.X = -Dir.X;
			if (Pos.X > Game.Width) Dir.X = -Dir.X;
			if (Pos.Y < 0) Dir.Y = -Dir.Y;
			if (Pos.Y > Game.Width) Dir.Y = -Dir.Y;
		}
	}

	class Star : BaseObject
	{
		public Star(Point pos, Point dir, Size size) : base(pos, dir, size)
		{

		}

		/// <summary>
		/// Изменённый метод Draw для класса Star заменяющий крест на изображение star.gif
		/// </summary>
		public override void Draw()
		{
			Image image = Image.FromFile(@"C:\Users\madam\Desktop\C# Lvl_2\Lesson_1\HomeWork_1\Image\star.gif");
			Game.Buffer.Graphics.DrawImage(image, Pos);
		}

		public override void Update()
		{
			Pos.X = Pos.X - Dir.X;
			if (Pos.X < 0) Pos.X = Game.Width + Size.Width;
			if (Pos.X > Game.Width) Dir.X = -Dir.X;
			if (Pos.X < 1) Dir.X = -Dir.X;
		}
	}

	class Planet: BaseObject
	{
		public Planet(Point pos, Point dir, Size size) : base(pos, dir, size)
		{

		}

		public override void Update()
		{
			Pos.X = Pos.X - Dir.X;
			if (Pos.X < 1) Pos.X = Game.Width; Pos.Y = Game.Height + 20 ; 
			if (Pos.Y > Game.Height) Pos.Y = Game.Height;
		}
	}
}
