using UnityEngine;
using System.Collections;

public static class ParticleProperties {
	// probably only applicable in Unity 4.5.x until changed in the future
	public static class Modules {
		public const string init = "InitialModule";
		public const string shape = "ShapeModule";
		public const string emission = "EmissionModule";
		public const string size = "SizeModule";
		public const string rotation = "RotationModule";
		public const string color = "ColorModule";
		public const string uv = "UVModule";
		public const string velocity = "VelocityModule";
		public const string force = "ForceModule";
		public const string externalForces = "ExternalForcesModule";
		public const string clampVelocity = "ClampVelocityModule";
		public const string sizeBySpeed = "SizeBySpeedModule";
		public const string rotationBySpeed = "RotationBySpeedModule";
		public const string colorBySpeed = "ColorBySpeedModule";
		public const string collision = "CollisionModule";
		public const string sub = "SubModule";

	}
}
