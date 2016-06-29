using UnityEngine;
using System.Collections;

namespace Artoncode.Core.Fluid {
	public enum FluidParticleGroupType {
		/// Prevents overlapping or leaking.
		SolidParticleGroup = 1 << 0,
		/// Keeps its shape.
		RigidParticleGroup = 1 << 1,
		/// Won't be destroyed if it gets empty.
		ParticleGroupCanBeEmpty = 1 << 2,
		/// Will be destroyed on next simulation step.
		ParticleGroupWillBeDestroyed = 1 << 3,
		/// Updates depth data on next simulation step.
		ParticleGroupNeedsUpdateDepth = 1 << 4,
		ParticleGroupInternalMask = ParticleGroupWillBeDestroyed | ParticleGroupNeedsUpdateDepth,
	};

	public class FluidParticleGroup {

	}
}