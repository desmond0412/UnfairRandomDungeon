using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Collections;

namespace Artoncode.Core.Fluid {
	public class FluidParticleDef : ScriptableObject {

		public FluidParticleType flags;
		public Vector2 position;
		public Vector2 velocity;
		public Color color;
		public Color foamColor;
		public float foamWeightThreshold;
		public float lifetime;
		public float mass;
		public float temperature;
		public float freezingTemperature;
		public float boilingTemperature;

		#if UNITY_EDITOR
		[MenuItem("Assets/Create/Fluid Particle Definition", false, 1000)]
		public static void createFluidParticleDef() {
			string path = EditorUtility.SaveFilePanel ("Save", "Assets/", "ParticleDef", "asset");
			if (path == "")
				return;
			
			FluidParticleDef asset = ScriptableObject.CreateInstance<FluidParticleDef> ();
			path = FileUtil.GetProjectRelativePath (path);
			AssetDatabase.CreateAsset (asset, path);
			AssetDatabase.SaveAssets ();
		}
		#endif
	}
}