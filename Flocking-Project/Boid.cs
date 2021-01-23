using System;
using System.Collections.Generic;

namespace Flocking_Project {
	class Boid {
		private double x, y, vx, vy, ax = 0, ay = 0;
		static private double radius = 10, perceptionRadius = 50, perceptionAngle = 270;

		public Boid(double x, double y, double vx, double vy) {
			this.x = x;
			this.y = y;
			this.vx = vx;
			this.vy = vy;
		}

		//odległość do punktu jako wektor przesunięcia
		private double[] VecDistTo(double x, double y, bool toroidal, double maxX, double maxY) {
			double dx = x - this.x;
			double dy = y - this.y;

			if (toroidal) {
				if (dx > maxX / 2 || dx < -maxX / 2)
					dx = maxX - dx;
				if (dy > maxY / 2 || dy < -maxY / 2)
					dy = maxY - dy;
			}

			double[] dist = { dx, dy };
			return dist;
		}

		private double[] VecDistTo(Boid other, bool toroidal, double maxX, double maxY) {
			return VecDistTo(other.x, other.y, toroidal, maxX, maxY);
		}

		//odległość do punktu jako wartość bezwzględna
		private double DistTo(double x, double y, bool toroidal, double maxX, double maxY) {
			double dx = Math.Abs(x - this.x);
			double dy = Math.Abs(y - this.y);
			if (toroidal) {
				if (dx > maxX / 2)
					dx = maxX - dx;
				if (dy > maxY / 2)
					dy = maxY - dy;
			}
			return Math.Sqrt(dx * dx + dy * dy);
		}

		private bool isInPercRadius(double x, double y, bool toroidal, double maxX, double maxY) {
			//szukamy w kącie widzenia - liczymy kąt między wektorem prędkości a wektorem prowadzącym z naszego położenia do (x,y)
			double[] r = VecDistTo(x, y, toroidal, maxX, maxY);
			double product = vx * r[0] + vy * r[1];
			product /= Math.Sqrt(vx * vx + vy * vy) * Math.Sqrt(r[0] * r[0] + r[1] * r[1]);

			double angle = Math.Acos(product);

			double dist = DistTo(x, y, toroidal, maxX, maxY);
			if (dist < perceptionRadius && dist > 0 && angle < perceptionAngle / 2)
				return true;
			else
				return false;
		}

		private bool isInPercRadius(Boid other, bool toroidal, double maxX, double maxY) {
			return isInPercRadius(other.x, other.y, toroidal, maxX, maxY);
		}

		public void Borders(double maxX, double maxY, bool toroidal) {
			if (toroidal) {
				if (x < 0) x = maxX;
				if (x > maxX) x = 0;
				if (y < 0) y = maxY;
				if (y > maxY) y = 0;
			} else {
				if (x < 0) {
					x = 0;
					vx *= -1;
				}
				if (x > maxX) {
					x = maxX;
					vx *= -1;
				}
				if (y < 0) {
					y = 0;
					vy *= -1;
				}
				if (y > maxY) {
					y = maxY;
					vy *= -1;
				}
			}
		}

		public void NormalizeVel(double maxSpeed) {
			double len = Math.Sqrt(vx * vx + vy * vy);
			vx /= len;
			vy /= len;
			vx *= maxSpeed;
			vy *= maxSpeed;
		}

		public void NormalizeAcc(double maxAcc) {
			double len = Math.Sqrt(ax * ax + ay * ay);
			ax /= len;
			ay /= len;
			ax *= maxAcc;
			ay *= maxAcc;
		}

		public void Separate(List<Boid> boids, double factor, bool toroidal, double maxX, double maxY) {
			double steerX = 0, steerY = 0;
			double[] dist;
			foreach (Boid boid in boids) {
				if (isInPercRadius(boid, toroidal, maxX, maxY)) {
					dist = VecDistTo(boid, toroidal, maxX, maxY);
					steerX -= dist[0];
					steerY -= dist[1];
				}
			}
			ax += steerX * factor;
			ay += steerY * factor;
		}

		public void Align(List<Boid> boids, double factor, bool toroidal, double maxX, double maxY) {
			double steerX = 0, steerY = 0;
			foreach (Boid boid in boids) {
				if (isInPercRadius(boid, toroidal, maxX, maxY)) {
					steerX += boid.vx;
					steerY += boid.vy;
				}
			}
			ax += steerX * factor;
			ay += steerY * factor;
		}

		public void Cohere(List<Boid> boids, double factor, bool toroidal, double maxX, double maxY) {
			double steerX = 0, steerY = 0;
			double[] dist;
			foreach (Boid boid in boids) {
				if (isInPercRadius(boid, toroidal, maxX, maxY)) {
					dist = VecDistTo(boid, toroidal, maxX, maxY);
					steerX += dist[0];   //dodajemy położenie sąsiedniego boidu względem tego boidu
					steerY += dist[1];
				}
			}
			ax += steerX * factor;
			ay += steerY * factor;
		}

		public void ApplyAcc(double maxSpeed) {
			double len = Math.Sqrt(ax * ax + ay * ay);
			if (len > maxSpeed) {
				ax /= len;
				ay /= len;
				ax *= maxSpeed;
				ay *= maxSpeed;
			}

			vx += ax;
			vy += ay;
		}

		#region setters/getters

		public static double PA {
			set { perceptionAngle = value; }
			get => perceptionAngle; 
		}

		public double Ax {
			set { ax = value; }
		}

		public double Ay {
			set { ay = value; }
		}

		public double X {
			set {
				x = value;
			}
			get => x;
		}

		public double Y {
			set {
				y = value;
			}
			get => y;
		}

		public double Vx {
			set {
				vx = value;
			}
			get => vx;
		}

		public double Vy {
			set {
				vy = value;
			}
			get => vy;
		}

		public static double R {
			set {
				radius = value;
			}
			get => radius;
		}

		public static double PR {
			set {
				perceptionRadius = value;
			}
			get => perceptionRadius;
		}
		public double V {
			get => Math.Sqrt(vx * vx + vy * vy);
			
		}

		public double A {
			get => Math.Sqrt(ax * ax + ay * ay);
		}
		#endregion
	}
}
