using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Flocking_Project {
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window {
		public MainWindow() {
			InitializeComponent();

			//żeby nie pisać tego 129576 razy
			//listOfUIElements.Add(percRadCheckBox);
			listOfUIElements.Add(percRLabel);
			listOfUIElements.Add(percRadiusTxtBox);
			listOfUIElements.Add(angleTextBox);
			listOfUIElements.Add(angleLabel);
			listOfUIElements.Add(amountTextBox);
			listOfUIElements.Add(amountLabel);
			listOfUIElements.Add(sepLabel);
			listOfUIElements.Add(sepTxtBox);
			listOfUIElements.Add(aliLabel);
			listOfUIElements.Add(aliTxtBox);
			listOfUIElements.Add(cohLabel);
			listOfUIElements.Add(cohTxtBox);
			listOfUIElements.Add(maxSpeedLabel);
			listOfUIElements.Add(maxSpeedTextBox);
			listOfUIElements.Add(toroidalCheckBox);
			listOfUIElements.Add(resetBtn);

			Boid.PR = double.Parse(percRadiusTxtBox.Text);
			Boid.PA = double.Parse(angleTextBox.Text);

			//kształt boida
			boidPoints.Add(new Point(-2.5, -4));
			boidPoints.Add(new Point(-2.5, 4));
			boidPoints.Add(new Point(1.5, -2));
			boidPoints.Add(new Point(2.5, -3));
			boidPoints.Add(new Point(6, 0));
			boidPoints.Add(new Point(2.5, 3));
			boidPoints.Add(new Point(1.5, 2));

			//domyślny kształt pola widzenia (ulegnie zmianie przy zmianie kąta)
			percAreaPoints.Add(new Point(0, 0));
			double N = 12;
			double deltaAngle = Boid.PA / N;
			for (int i = 0; i < N + 1; i++) {
				double x = Boid.PR * Math.Cos((i * deltaAngle - Boid.PA / 2) * Math.PI / 180);
				double y = Boid.PR * Math.Sin((i * deltaAngle - Boid.PA / 2) * Math.PI / 180);
				Console.WriteLine(x + ", " + y);
				percAreaPoints.Add(new Point(x, y));
			}


			dispatcherTimer.Tick += dispatcherTimer_Tick;
			dispatcherTimer.Interval = new TimeSpan(0, 0, 0, 0, 10);
			dispatcherTimer.Start();
		}

		System.Windows.Threading.DispatcherTimer dispatcherTimer = new System.Windows.Threading.DispatcherTimer();
		List<Boid> listOfBoids = new List<Boid>();
		List<UIElement> listOfUIElements = new List<UIElement>();
		bool animate = false;
		double angle;
		PointCollection boidPoints = new PointCollection();
		PointCollection percAreaPoints = new PointCollection();
		Random r = new Random();

		private void DrawPercArea(Boid boid) {
			angle = Math.Atan2(boid.Vy, boid.Vx);

			Polygon percArea = new Polygon { Points = percAreaPoints, Stroke = Brushes.Blue, StrokeThickness = 1 };

			percArea.RenderTransform = new RotateTransform(angle * 180 / Math.PI);
			canvas.Children.Add(percArea);
			Canvas.SetLeft(percArea, boid.X);
			Canvas.SetTop(percArea, boid.Y);
		}

		private void DrawBoid(Boid boid) {
			angle = Math.Atan2(boid.Vy, boid.Vx);

			Polygon graphicalBoid = new Polygon { Points = boidPoints, Fill = Brushes.Red, Stroke = Brushes.Black, StrokeThickness = 1 };
			canvas.Children.Add(graphicalBoid);
			graphicalBoid.RenderTransform = new RotateTransform(angle * 180 / Math.PI);
			Canvas.SetLeft(graphicalBoid, boid.X);
			Canvas.SetTop(graphicalBoid, boid.Y);
		}

		private void dispatcherTimer_Tick(object sender, EventArgs e) {
			if (animate) {
				canvas.Children.Clear();
				//liczymy nowe prędkości
				for (int i = 0; i < listOfBoids.Count; i++) {   //for podobno szybszy od foreach
					listOfBoids[i].Ax = 0;
					listOfBoids[i].Ay = 0;
					listOfBoids[i].Separate(listOfBoids, double.Parse(sepTxtBox.Text), (bool)toroidalCheckBox.IsChecked, canvas.Width, canvas.Height);
					listOfBoids[i].Align(listOfBoids, double.Parse(aliTxtBox.Text), (bool)toroidalCheckBox.IsChecked, canvas.Width, canvas.Height);
					listOfBoids[i].Cohere(listOfBoids, double.Parse(cohTxtBox.Text), (bool)toroidalCheckBox.IsChecked, canvas.Width, canvas.Height);
				}
				//i na podstawie prędkości nowe położenia
				for (int i = 0; i < listOfBoids.Count; i++) {
					listOfBoids[i].ApplyAcc(double.Parse(maxSpeedTextBox.Text));
					if (listOfBoids[i].A > double.Parse(maxSpeedTextBox.Text))
						listOfBoids[i].NormalizeAcc(double.Parse(maxSpeedTextBox.Text));
					if (listOfBoids[i].V > double.Parse(maxSpeedTextBox.Text))
						listOfBoids[i].NormalizeVel(double.Parse(maxSpeedTextBox.Text));
					listOfBoids[i].X += listOfBoids[i].Vx;
					listOfBoids[i].Y += listOfBoids[i].Vy;
					listOfBoids[i].Borders(canvas.Width, canvas.Height, (bool)toroidalCheckBox.IsChecked);
					if (percRadCheckBox.IsChecked == true)
						DrawPercArea(listOfBoids[i]);
					DrawBoid(listOfBoids[i]);
				}
			}
		}


		private void startStopBtn_Click(object sender, RoutedEventArgs e) {
			if (startStopBtn.Content.Equals("START")) {
				foreach (UIElement element in listOfUIElements) element.IsEnabled = false;
				startStopBtn.Content = "STOP";
				animate = true;
			} else {
				foreach (UIElement element in listOfUIElements) element.IsEnabled = true;
				startStopBtn.Content = "START";
				animate = false;
			}
		}

		private void resetBtn_Click(object sender, RoutedEventArgs e) {
			listOfBoids.Clear();
			canvas.Children.Clear();
			double x, y, angle;
			for (int i = 0; i < int.Parse(amountTextBox.Text); i++) {
				x = r.NextDouble() * canvas.Width;
				y = r.NextDouble() * canvas.Height;
				angle = r.NextDouble() * 2 * Math.PI;
				Boid boid = new Boid(x, y, Math.Sin(angle), Math.Cos(angle));
				listOfBoids.Add(new Boid(x, y, Math.Sin(angle), Math.Cos(angle)));
				DrawBoid(boid);
			}
		}


		private void angleTextBox_TextChanged(object sender, TextChangedEventArgs e) {
			try {
				Boid.PA = double.Parse(angleTextBox.Text);
				//tworzymy nowe pole widzenia
				percAreaPoints.Clear();

				percAreaPoints.Add(new Point(0, 0));
				double N = 12;
				double deltaAngle = Boid.PA / N;
				for (int i = 0; i < N + 1; i++) {
					double x = Boid.PR * Math.Cos((i * deltaAngle - Boid.PA / 2) * Math.PI / 180);
					double y = Boid.PR * Math.Sin((i * deltaAngle - Boid.PA / 2) * Math.PI / 180);
					Console.WriteLine(x + ", " + y);
					percAreaPoints.Add(new Point(x, y));
				}
			} catch (Exception) {
				angleTextBox.Text = "270";
				Boid.PA = 270;
			}
		}


		private void amountTextBox_TextChanged(object sender, TextChangedEventArgs e) {
			int amount;
			double x, y, angle;

			try {
				amount = int.Parse(amountTextBox.Text);
			} catch (Exception) {
				amount = 50;
				amountTextBox.Text = "50";
			}

			listOfBoids.Clear();
			canvas.Children.Clear();

			for (int i = 0; i < amount; i++) {
				x = r.NextDouble() * canvas.Width;
				y = r.NextDouble() * canvas.Height;
				angle = r.NextDouble() * 2 * Math.PI;
				Boid boid = new Boid(x, y, Math.Sin(angle), Math.Cos(angle));
				listOfBoids.Add(boid);
				DrawBoid(boid);
			}
		}

		private void percRadiusTxtBox_TextChanged(object sender, TextChangedEventArgs e) {
			try {
				Boid.PR = double.Parse(percRadiusTxtBox.Text);

				//tworzymy nowe pole widzenia
				percAreaPoints.Clear();

				percAreaPoints.Add(new Point(0, 0));
				double N = 12;
				double deltaAngle = Boid.PA / N;
				for (int i = 0; i < N + 1; i++) {
					double x = Boid.PR * Math.Cos((i * deltaAngle - Boid.PA / 2) * Math.PI / 180);
					double y = Boid.PR * Math.Sin((i * deltaAngle - Boid.PA / 2) * Math.PI / 180);
					Console.WriteLine(x + ", " + y);
					percAreaPoints.Add(new Point(x, y));
				}
			} catch (Exception) {
				percRadiusTxtBox.Text = "50";
				Boid.PR = 50;
			}
		}
	}
}
