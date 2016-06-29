using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Collections;

namespace Artoncode.Core.Fluid {
	public class FluidSystemDef : ScriptableObject {
		public bool strictContactCheck;
		public float density;
		public float gravityScale;
		public float radius;
		public int maxCount;

		/// Increases pressure in response to compression
		/// Smaller values allow more compression
		public float pressureStrength;
	
		/// Reduces velocity along the collision normal
		/// Smaller value reduces less
		public float dampingStrength;
	
		/// Restores shape of elastic particle groups
		/// Larger values increase elastic particle velocity
		public float elasticStrength;
	
		/// Restores length of spring particle groups
		/// Larger values increase spring particle velocity
		public float springStrength;
	
		/// Reduces relative velocity of viscous particles
		/// Larger values slow down viscous particles more
		public float viscousStrength;
	
		/// Produces pressure on tensile particles
		/// 0~0.2. Larger values increase the amount of surface tension.
		public float surfaceTensionPressureStrength;
	
		/// Smoothes outline of tensile particles
		/// 0~0.2. Larger values result in rounder, smoother, water-drop-like
		/// clusters of particles.
		public float surfaceTensionNormalStrength;
	
		/// Produces additional pressure on repulsive particles
		/// Larger values repulse more
		/// Negative values mean attraction. The range where particles behave
		/// stably is about -0.2 to 2.0.
		public float repulsiveStrength;
	
		/// Produces repulsion between powder particles
		/// Larger values repulse more
		public float powderStrength;
	
		/// Pushes particles out of solid particle group
		/// Larger values repulse more
		public float ejectionStrength;
	
		/// Produces static pressure
		/// Larger values increase the pressure on neighboring partilces
		/// For a description of static pressure, see
		/// http://en.wikipedia.org/wiki/Static_pressure#Static_pressure_in_fluid_dynamics
		public float staticPressureStrength;
	
		/// Reduces instability in static pressure calculation
		/// Larger values make stabilize static pressure with fewer iterations
		public float staticPressureRelaxation;

		/// Upward velocity for particles with lower density
		public float densityStrength;

		/// Upward velocity for particles with higher temperature
		public float temperatureStrength;

		/// Computes static pressure more precisely
		/// See SetStaticPressureIterations for details
		public int staticPressureIterations;
	
		/// Determines how fast colors are mixed
		/// 1.0f ==> mixed immediately
		/// 0.5f ==> mixed half way each simulation step (see b2World::Step())
		public float colorMixingStrength;

		public float temperaturMixingStrength;
	
		/// Whether to destroy particles by age when no more particles can be
		/// created.  See #b2ParticleSystem::SetDestructionByAge() for
		/// more information.
		public bool destroyByAge;
	
		/// Granularity of particle lifetimes in seconds.  By default this is
		/// set to (1.0f / 60.0f) seconds.  b2ParticleSystem uses a 32-bit signed
		/// value to track particle lifetimes so the maximum lifetime of a
		/// particle is (2^32 - 1) / (1.0f / lifetimeGranularity) seconds.
		/// With the value set to 1/60 the maximum lifetime or age of a particle is
		/// 2.27 years.
		public float lifetimeGranularity;

		#if UNITY_EDITOR
		[MenuItem("Assets/Create/Fluid System Definition", false, 1000)]
		public static void createFluidParticleDef() {
			string path = EditorUtility.SaveFilePanel ("Save", "Assets/", "FluidSystemDef", "asset");
			if (path == "")
				return;

			FluidSystemDef asset = ScriptableObject.CreateInstance<FluidSystemDef> ();
			path = FileUtil.GetProjectRelativePath (path);
			AssetDatabase.CreateAsset (asset, path);
			AssetDatabase.SaveAssets ();
		}
		#endif
	}
}