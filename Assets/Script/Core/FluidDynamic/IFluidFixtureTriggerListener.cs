using UnityEngine;
using System.Collections;

namespace Artoncode.Core.Fluid {
	public interface IFluidFixtureTriggerListener {
		void fluidOnTrigger (FluidParticleBodyContact bodyContact);
	}
}
