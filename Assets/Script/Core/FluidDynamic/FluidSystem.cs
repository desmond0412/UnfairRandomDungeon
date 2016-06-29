//#define FLUID_DEBUG
#if UNITY_EDITOR
using UnityEditor;
#endif

using UnityEngine;
using System.Collections.Generic;

namespace Artoncode.Core.Fluid {
	public class FluidSystem : MonoBehaviour {
		// Fluid dynamic constants
		static readonly float maxParticleForce = 0.5f;
		static readonly float maxParticlePressure = 0.25f;
		static readonly float minParticleWeight = 1.0f;
		static readonly float particleStride = 0.75f;

		// Proxy tag constants
		static readonly int xTruncBits = 12;
		static readonly int yTruncBits = 12;
		static readonly int tagBits = 8 * sizeof(int) - 1;
		static readonly int yOffset = 1 << (yTruncBits - 1);
		static readonly int yShift = tagBits - yTruncBits;
		static readonly int xShift = tagBits - yTruncBits - xTruncBits;
		static readonly int xScale = 1 << xShift;
		static readonly int xOffset = xScale * (1 << (xTruncBits - 1));
        // not used anywhere
        //static readonly int yMask = ((1 << yTruncBits) - 1) << yShift;
        //static readonly int xMask = ~yMask;

		//
		public FluidSystemDef fluidSystemDef;
		public int particleIterations = 1;
		public List<FluidFixtureShape> fixtureBuffer;

		//
		bool paused;
		int timestamp;
		FluidParticleType allParticleFlags;
		bool needsUpdateAllParticleFlags;
        // not used anywhere
		//FluidParticleGroupType allGroupFlags;
		//bool needsUpdateAllGroupFlags;
		bool hasForce;
		int iterationIndex;
		float inverseDensity;
		float particleDiameter;
		float inverseDiameter;
		float squaredDiameter;

		//
		int count;
		List<FluidParticle> particleBuffer;
		List<FluidProxy> proxyBuffer;
		List<FluidParticleContact> contactBuffer;
		List<FluidParticleBodyContact> bodyContactBuffer;
		List<FluidParticleBodyContact> triggerContactBuffer;
//	b2GrowableBuffer<b2ParticlePair> m_pairBuffer;
//	b2GrowableBuffer<b2ParticleTriad> m_triadBuffer;

//	UserOverridableBuffer<int32> m_expirationTimeBuffer;
		/// List of particle indices sorted by expiration time.
//	UserOverridableBuffer<int32> m_indexByExpirationTimeBuffer;
		/// Time elapsed in 32:32 fixed point.  Each non-fractional unit of time
		/// corresponds to b2ParticleSystemDef::lifetimeGranularity seconds.
		long timeElapsed;
		/// Whether the expiration time buffer has been modified and needs to be
		/// resorted.
		bool m_expirationTimeBufferRequiresSorting;
//	int m_groupCount;
//	b2ParticleGroup* m_groupList;

		void Awake () {
			paused = false;
			timestamp = 0;
			allParticleFlags = FluidParticleType.WaterParticle;
			needsUpdateAllParticleFlags = false;
			//allGroupFlags = 0;
			//needsUpdateAllGroupFlags = false;
			hasForce = false;
			iterationIndex = 0;

			inverseDensity = 1 / fluidSystemDef.density;
			particleDiameter = fluidSystemDef.radius * 2;
			squaredDiameter = particleDiameter * particleDiameter;
			inverseDiameter = 1 / particleDiameter;

			count = 0;
			particleBuffer = new List<FluidParticle> (fluidSystemDef.maxCount);
			proxyBuffer = new List<FluidProxy> (fluidSystemDef.maxCount);
			contactBuffer = new List<FluidParticleContact> (fluidSystemDef.maxCount * 4);
			bodyContactBuffer = new List<FluidParticleBodyContact> (fluidSystemDef.maxCount * 4);
			triggerContactBuffer = new List<FluidParticleBodyContact> (fluidSystemDef.maxCount * 4);
		}

		void Start () {
			foreach (FluidFixtureShape fixture in fixtureBuffer) {
				fixture.addTo (this);
			}
		}

		void FixedUpdate () {
			solve (Time.fixedDeltaTime);
		}
	
		public int getParticleCount () {
			return count;
		}
		
		public List<FluidParticle> getParticles () {
			return particleBuffer;
		}

		public List<FluidProxy> getParticleProxies () {
			return proxyBuffer;
		}

		public int getIterationIndex () {
			return iterationIndex;
		}
	
		public List<FluidParticleContact> getContacts() {
			return contactBuffer;
		}

		public List<FluidParticleBodyContact> getBodyContacts () {
			return bodyContactBuffer;
		}

		public List<FluidParticleBodyContact> getTriggerContacts () {
			return triggerContactBuffer;
		}

		public FluidParticle createParticle (FluidParticleDef def) {
			if (count >= fluidSystemDef.maxCount) {
				return null;
			}
			count++;

			FluidParticle fp = new FluidParticle ();
			particleBuffer.Add (fp);
			setParticleFlags (fp, def.flags);
			fp.position = def.position;
			fp.prevPosition = def.position;
			fp.velocity = def.velocity;
			fp.weight = 0;
			fp.force = Vector2.zero;
			fp.mass = def.mass;
			fp.staticPressure = 0;
			fp.depth = 0;
			fp.color = def.color;
			fp.foamColor = def.foamColor;
			fp.foamWeightThreshold = def.foamWeightThreshold;
			fp.temperature = def.temperature;
			fp.freezingTemperature = def.freezingTemperature;
			fp.boilingTemperature = def.boilingTemperature;

			FluidProxy proxy = new FluidProxy ();
			proxyBuffer.Add (proxy);
			proxy.fp = fp;

//		bool finiteLifetime = def.lifetime > 0;
//		if (m_expirationTimeBuffer.data || finiteLifetime)
//		{
//			SetParticleLifetime(index, finiteLifetime ? def.lifetime :
//			                    ExpirationTimeToLifetime(
//				-GetQuantizedTimeElapsed()));
//			// Add a reference to the newly added particle to the end of the
//			// queue.
//			m_indexByExpirationTimeBuffer.data[index] = index;
//		}

			return fp;
		}

		public void createParticlesFillShapeForGroup (FluidFixtureShape shape, FluidParticleDef def) {
			float stride = getParticleStride ();
			FluidAABB aabb = shape.getAABB ();

//			Color c = def.color;
			for (float y = Mathf.Floor(aabb.lowerBound.y / stride) * stride; y < aabb.upperBound.y; y += stride) {
				for (float x = Mathf.Floor(aabb.lowerBound.x / stride) * stride; x < aabb.upperBound.x; x += stride) {
					Vector2 p = new Vector2 (x, y);
					if (shape.testPoint (p)) {
						def.position = p;
//						def.color = new Color(Random.Range(0,2),Random.Range(0,2),Random.Range(0,2));
						createParticle (def);
					}
				}
			}
//			def.color = c;
		}

		public List<FluidParticle> getParticlesInShape(FluidFixtureShape shape) {
			List<FluidParticle> fp = new List<FluidParticle> ();
			for (int i=0; i<count; i++) {
				if (shape.testPoint (particleBuffer [i].position)) {
					fp.Add (particleBuffer[i]);
				}
			}
			return fp;
		}

		public void destroyParticlesFillShape (FluidFixtureShape shape) {
			for (int i=0; i<count; i++) {
				if (shape.testPoint (particleBuffer [i].position)) {
					destroyParticle (particleBuffer [i], true);
				}
			}
		}

		public void destroyParticle (int index, bool callDestructionListener) {
			if (index >= count) {
				return;
			}
			destroyParticle (particleBuffer [index], callDestructionListener);
		}

		public void destroyParticle (FluidParticle fp, bool callDestructionListener) {
			FluidParticleType flags = FluidParticleType.ZombieParticle;
			if (callDestructionListener) {
				flags |= FluidParticleType.DestructionListenerParticle;
			}
			setParticleFlags (fp, fp.flags | flags);
		}

		public void addFixture (FluidFixtureShape fixture) {
			fixtureBuffer.Add (fixture);
			fixture.addTo (this);
		}

		public void removeFixture (FluidFixtureShape fixture) {
			fixtureBuffer.Remove (fixture);
			fixture.removeFrom (this);
		}

		public void applyForce (int firstIndex, int lastIndex, Vector2 force) {
			Vector2 distributedForce = force / (lastIndex - firstIndex);

			prepareForceBuffer ();

			for (int i=firstIndex; i<lastIndex; i++) {
				particleBuffer [i].force += distributedForce;
			}
		}
	
		public void particleApplyForce (int index, Vector2 force) {
			prepareForceBuffer ();
			particleBuffer [index].force += force;
		}
	
		public void particleApplyForce (FluidParticle fp, Vector2 force) {
			prepareForceBuffer ();
			fp.force += force;
		}
	
		void prepareForceBuffer () {
			if (!hasForce) {
				for (int i=0; i<count; i++) {
					particleBuffer [i].force = Vector2.zero;
				}
				hasForce = true;
			}
		}

		void solve (float dt) {
			if (count == 0) {
				return;
			}
//		if (m_expirationTimeBuffer.data)
//		{
//			SolveLifetimes(step);
//		}
			if ((allParticleFlags & FluidParticleType.ZombieParticle) > 0) {
				solveZombie ();
			}
			if (needsUpdateAllParticleFlags) {
				updateAllParticleFlags ();
			}
//		if (m_needsUpdateAllGroupFlags)
//		{
//			UpdateAllGroupFlags();
//		}
			if (paused) {
				return;
			}

			float subStep = dt / particleIterations;
			for (iterationIndex = 0; iterationIndex < particleIterations; iterationIndex++) {
				++timestamp;
				updateContacts (false);
				updateBodyContacts ();
				computeWeight ();
//			if (m_allGroupFlags & b2_particleGroupNeedsUpdateDepth)
//			{
//				ComputeDepth();
//			}
//			if (m_allParticleFlags & b2_reactiveParticle)
//			{
//				UpdatePairsAndTriadsWithReactiveParticles();
//			}
				if (hasForce) {
					solveForce (subStep);
				}
				if ((allParticleFlags & FluidParticleType.ViscousParticle) > 0) {
					solveViscous ();
				}
//			if (m_allParticleFlags & b2_repulsiveParticle)
//			{
//				SolveRepulsive(subStep);
//			}
				if ((allParticleFlags & FluidParticleType.PowderParticle) > 0) {
					solvePowder (subStep);
				}
				if ((allParticleFlags & FluidParticleType.TensileParticle) > 0) {
					solveTensile (subStep);
				}
//			if (m_allGroupFlags & b2_solidParticleGroup)
//			{
//				SolveSolid(subStep);
//			}
				if ((allParticleFlags & FluidParticleType.ColorMixingParticle) > 0) {
					solveColorMixing ();
				}
				solveGravity (subStep);
				if ((allParticleFlags & FluidParticleType.StaticPressureParticle) > 0) {
					solveStaticPressure (subStep);
				}
				solvePressure (subStep);
				solveDamping (subStep);
//			if (m_allParticleFlags & k_extraDampingFlags)
//			{
//				SolveExtraDamping();
//			}
//			// SolveElastic and SolveSpring refer the current velocities for
//			// numerical stability, they should be called as late as possible.
//			if (m_allParticleFlags & b2_elasticParticle)
//			{
//				SolveElastic(subStep);
//			}
//			if (m_allParticleFlags & b2_springParticle)
//			{
//				SolveSpring(subStep);
//			}
				limitVelocity (subStep);
//			if (m_allGroupFlags & b2_rigidParticleGroup)
//			{
//				SolveRigidDamping();
//			}
//			if (m_allParticleFlags & b2_barrierParticle)
//			{
//				SolveBarrier(subStep);
//			}
//			// SolveCollision, SolveRigid and SolveWall should be called after
//			// other force functions because they may require particles to have
//			// specific velocities.
//			solveCollision(subStep);
//			if (m_allGroupFlags & b2_rigidParticleGroup)
//			{
//				SolveRigid(subStep);
//			}
				if ((allParticleFlags & FluidParticleType.WallParticle) > 0) {
					solveWall ();
				}

//			if (iterationIndex == 0) {
//				for (int i=0; i<count; i++) {
//					FluidParticle fp = particleBuffer [i];
//					fp.prevPosition = fp.position;
//				}
//			}

				for (int i=0; i<count; i++) {
					FluidParticle fp = particleBuffer [i];
					fp.prevPosition = fp.position;
					fp.position += subStep * fp.velocity;
				}
			}
		}

		float getCriticalVelocity (float dt) {
			return particleDiameter / dt;
		}

		float getCriticalVelocitySquared (float dt) {
			float velocity = getCriticalVelocity (dt);
			return velocity * velocity;
		}

		float getCriticalPressure (float dt) {
			return fluidSystemDef.density * getCriticalVelocitySquared (dt);
		}

		float getParticleInvMass () {
			float inverseStride = inverseDiameter * (1.0f / particleStride);
			return inverseDensity * inverseStride * inverseStride;
		}

		float getParticleMass () {
			float stride = getParticleStride ();
			return fluidSystemDef.density * stride * stride;
		}

		float getParticleStride () {
			return particleStride * particleDiameter;
		}

		void computeWeight () {
			for (int i=0; i<count; i++) {
				particleBuffer [i].weight = 0;
			}
			for (int i=0; i<bodyContactBuffer.Count; i++) {
				FluidParticleBodyContact contact = bodyContactBuffer [i];
				contact.fp.weight += contact.weight;
			}
			for (int i=0; i<contactBuffer.Count; i++) {
				FluidParticleContact contact = contactBuffer [i];
				contact.a.weight += contact.weight;
				contact.b.weight += contact.weight;
			}
		}

		void updateAllParticleFlags () {
			allParticleFlags = 0;
			for (int i = 0; i < count; i++) {
				allParticleFlags |= particleBuffer [i].flags;
			}
			needsUpdateAllParticleFlags = false;
		}

		void updateContacts (bool exceptZombie) {
			updateProxies ();
			sortProxies ();
		
//		b2ParticlePairSet particlePairs(&m_world->m_stackAllocator);
//		NotifyContactListenerPreContact(&particlePairs);
//		
			findContacts ();
//		FilterContacts(m_contactBuffer);
//		
//		NotifyContactListenerPostContact(particlePairs);
//		
//		if (exceptZombie)
//		{
//			m_contactBuffer.RemoveIf(b2ParticleContactIsZombie);
//		}
		}

		void updateProxies () {
			for (int i=0; i<count; i++) {
				FluidProxy proxy = proxyBuffer [i];
				Vector2 p = proxy.fp.position;
				proxy.tag = computeTag (inverseDiameter * p.x, inverseDiameter * p.y);
			}
		}

		void sortProxies () {
			proxyBuffer.Sort ((x, y) => x.tag < y.tag ? -1 : 1);
		}

		int computeTag (float x, float y) {
			return ((int)(-y + yOffset) << yShift) + (int)(xScale * x + xOffset);
		}

		int computeRelativeTag (int tag, int x, int y) {
			return tag + (y << yShift) + (x << xShift);
		}

		void setParticleFlags (FluidParticle fp, FluidParticleType newFlags) {
			if ((fp.flags & ~newFlags) > 0) {
				needsUpdateAllParticleFlags = true;
			}

			if ((~allParticleFlags & newFlags) > 0) {
				allParticleFlags |= newFlags;
			}
			fp.flags = newFlags;
		}

		void addContact (FluidParticle a, FluidParticle b) {
			Vector2 d = b.position - a.position;
			float distBtParticlesSq = d.sqrMagnitude;
			if (distBtParticlesSq < squaredDiameter) {
				float dist = Mathf.Sqrt (distBtParticlesSq);
				FluidParticleContact contact = new FluidParticleContact ();
				contactBuffer.Add (contact);
				contact.a = a;
				contact.b = b;
				contact.flags = a.flags | b.flags;
				contact.weight = 1 - dist * inverseDiameter;// * (b.mass / a.mass);
				contact.normal = d / dist;
			}
		}

		void findContacts () {
			contactBuffer.Clear ();
			for (int a=0, c=0; a<count; a++) {
				FluidProxy proxy = proxyBuffer [a];
				int rightTag = computeRelativeTag (proxy.tag, 1, 0);
				for (int b=a+1; b<count; b++) {
					if (rightTag < proxyBuffer [b].tag) {
						break;
					}
					addContact (proxy.fp, proxyBuffer [b].fp);
				}

				int bottomLeftTag = computeRelativeTag (proxy.tag, -1, 1);
				for (; c<count; c++) {
					if (bottomLeftTag <= proxyBuffer [c].tag) {
						break;
					}
				}

				int bottomRightTag = computeRelativeTag (proxy.tag, 1, 1);
				for (int b=c; b<count; b++) {
					if (bottomRightTag < proxyBuffer [b].tag) {
						break;
					}
					addContact (proxy.fp, proxyBuffer [b].fp);
				}
			}
		}

		void updateBodyContacts () {
//		// If the particle contact listener is enabled, generate a set of
//		// fixture / particle contacts.
//		FixtureParticleSet fixtureSet(&m_world->m_stackAllocator);
//		NotifyBodyContactListenerPreContact(&fixtureSet);
//		
//		if (m_stuckThreshold > 0)
//		{
//			const int32 particleCount = GetParticleCount();
//			for (int32 i = 0; i < particleCount; i++)
//			{
//				// Detect stuck particles, see comment in
//				// b2ParticleSystem::DetectStuckParticle()
//				m_bodyContactCountBuffer.data[i] = 0;
//				if (m_timestamp > (m_lastBodyContactStepBuffer.data[i] + 1))
//				{
//					m_consecutiveContactStepsBuffer.data[i] = 0;
//				}
//			}
//		}
//		m_bodyContactBuffer.SetCount(0);
//		m_stuckParticleBuffer.SetCount(0);
//		
			bodyContactBuffer.Clear ();
		
			UpdateBodyContactsCallback callback = new UpdateBodyContactsCallback (this);
			FluidAABB aabb;
			computeAABB (out aabb);

			queryAABB (callback, aabb);
//		
//		if (m_def.strictContactCheck)
//		{
//			RemoveSpuriousBodyContacts();
//		}
//		
//		NotifyBodyContactListenerPostContact(fixtureSet);
		}

		void queryAABB (FluidFixtureParticleQueryCallback callback, FluidAABB aabb) {
			if (iterationIndex == 0) {
				triggerContactBuffer.Clear ();
			}
			for (int i=0; i<fixtureBuffer.Count; i++) {
				FluidFixtureShape fixture = fixtureBuffer [i];
				if (fixture != null) {
					if (fixture.enabled && fixture.gameObject.activeInHierarchy) {
						if (!fixture.isTrigger() || iterationIndex == 0) {
							FluidAABB fixtureAABB = fixture.getAABB ();
							if (fixtureAABB.isOverlap (aabb)) {
								int lowerTag = computeTag (inverseDiameter * fixtureAABB.lowerBound.x - 1,
						                           inverseDiameter * fixtureAABB.upperBound.y + 1);
								int upperTag = computeTag (inverseDiameter * fixtureAABB.upperBound.x + 1,
						                           inverseDiameter * fixtureAABB.lowerBound.y - 1);

								int beginProxy = 0;
								int endProxy = count - 1;
								int firstProxy = findLowerBoundProxy (beginProxy, endProxy, lowerTag);
								int lastProxy = findUpperBoundProxy (beginProxy, endProxy, upperTag);

								for (int j=firstProxy; j<lastProxy; j++) {
									callback.reportFixtureAndParticle (fixture, proxyBuffer [j].fp);
								}
							}
						}
					}
				}
				else {
//					removeFixture(fi
					fixtureBuffer.RemoveAt(i);
					i--;
				}
			}
		}

		int findLowerBoundProxy (int first, int last, int val) {
			int it;
			int step;
			int c = last - first + 1;
			while (c > 0) {
				it = first;
				step = c / 2;
				it += step;
				if (proxyBuffer [it].tag < val) {
					first = ++it;
					c -= step + 1;
				} else {
					c = step;
				}
			}
			return first;
		}

		int findUpperBoundProxy (int first, int last, int val) {
			int it;
			int step;
			int c = last - first + 1;
			while (c > 0) {
				it = first;
				step = c / 2;
				it += step;
				if (!(val < proxyBuffer [it].tag)) {
					first = ++it;
					c -= step + 1;
				} else {
					c = step;
				}
			}
			return first;
		}

		void computeAABB (out FluidAABB aabb) {
			aabb = new FluidAABB ();

			aabb.lowerBound.x = +float.MaxValue;
			aabb.lowerBound.y = +float.MaxValue;
			aabb.upperBound.x = -float.MaxValue;
			aabb.upperBound.y = -float.MaxValue;
		
			for (int i=0; i<count; i++) {
				Vector2 p = particleBuffer [i].position;
				aabb.lowerBound = Vector2.Min (aabb.lowerBound, p);
				aabb.upperBound = Vector2.Max (aabb.upperBound, p);
			}
			aabb.lowerBound.x -= particleDiameter;
			aabb.lowerBound.y -= particleDiameter;
			aabb.upperBound.x += particleDiameter;
			aabb.upperBound.y += particleDiameter;
		}

	#region SOLVER
		void solveGravity (float dt) {
			Vector2 gravity = dt * fluidSystemDef.gravityScale * Physics2D.gravity;
			for (int i=0; i<count; i++) {
				particleBuffer [i].velocity += gravity * particleBuffer [i].mass;
			}
		}

		void solveWall () {
			for (int i=0; i<count; i++) {
				FluidParticle fp = particleBuffer [i];
				if ((fp.flags & FluidParticleType.WallParticle) > 0) {
					fp.velocity = Vector2.zero;
				}
			}
		}

		void solvePowder (float dt) {
			float powderStrength = fluidSystemDef.powderStrength * getCriticalVelocity (dt);
			float minWeight = 1.0f - particleStride;

			for (int i=0; i<contactBuffer.Count; i++) {
				FluidParticleContact contact = contactBuffer [i];
				if ((contact.flags & FluidParticleType.PowderParticle) > 0) {
					if (contact.weight > minWeight) {
						Vector2 f = powderStrength * (contact.weight - minWeight) * contact.normal;
						contact.a.velocity -= f;
						contact.b.velocity += f;
					}
				}
			}
		}

		void solveColorMixing () {
			float strength = fluidSystemDef.colorMixingStrength;
			if (strength > 0) {
				for (int i=0; i<contactBuffer.Count; i++) {
					FluidParticleContact contact = contactBuffer [i];
					if ((contact.a.flags & contact.b.flags & FluidParticleType.ColorMixingParticle) > 0) {
						Color a = contact.a.color;
						Color b = contact.b.color;
						float dr = (b.r - a.r) * strength;
						float dg = (b.g - a.g) * strength;
						float db = (b.b - a.b) * strength;
						float da = (b.a - a.a) * strength;
						a.r += dr;
						a.g += dg;
						a.b += db;
						a.a += da;
						b.r -= dr;
						b.g -= dg;
						b.b -= db;
						b.a -= da;
						contact.a.color = a;
						contact.b.color = b;

						float dm = (contact.b.mass - contact.a.mass) * strength;
						contact.a.mass += dm;
						contact.b.mass -= dm;
					}
				}
			}
		}

		void solveForce (float dt) {
			float velocityPerForce = dt * getParticleInvMass ();
			for (int i=0; i<count; i++) {
				FluidParticle fp = particleBuffer [i];
				fp.velocity += velocityPerForce * fp.force;
			}
			hasForce = false;
		}

		void solveStaticPressure (float dt) {
			float criticalPressure = getCriticalPressure (dt);
			float pressurePerWeight = fluidSystemDef.staticPressureStrength * criticalPressure;
			float maxPressure = maxParticlePressure * criticalPressure;
			float relaxation = fluidSystemDef.staticPressureRelaxation;

			for (int t=0; t<fluidSystemDef.staticPressureIterations; t++) {
				for (int i=0; i<count; i++) {
					particleBuffer [i].accumulation = 0;
				}
				for (int i=0; i<contactBuffer.Count; i++) {
					FluidParticleContact contact = contactBuffer [i];
					if ((contact.flags & FluidParticleType.StaticPressureParticle) > 0) {
						contact.a.accumulation += contact.weight * contact.b.staticPressure;
						contact.b.accumulation += contact.weight * contact.a.staticPressure;
					}
				}
				for (int i=0; i<count; i++) {
					FluidParticle fp = particleBuffer [i];
					if ((fp.flags & FluidParticleType.StaticPressureParticle) > 0) {
						float h = (fp.accumulation + pressurePerWeight * (fp.weight - minParticleWeight)) / (fp.weight + relaxation);
						fp.staticPressure = Mathf.Clamp (h, 0.0f, maxPressure);
					} else {
						fp.staticPressure = 0;
					}
				}
			}
		}

		void solveTensile (float dt) {
			for (int i=0; i<count; i++) {
				particleBuffer [i].accumulation2 = Vector2.zero;
			}
			for (int i=0; i<contactBuffer.Count; i++) {
				FluidParticleContact contact = contactBuffer [i];
				if ((contact.flags & FluidParticleType.TensileParticle) > 0) {
					Vector2 weightedNormal = (1 - contact.weight) * contact.weight * contact.normal;
					contact.a.accumulation2 -= weightedNormal;
					contact.b.accumulation2 += weightedNormal;
				}
			}
			float criticalVelocity = getCriticalVelocity (dt);
			float pressureStrength = fluidSystemDef.surfaceTensionPressureStrength * criticalVelocity;
			float normalStrength = fluidSystemDef.surfaceTensionNormalStrength * criticalVelocity;
			float maxVelocityVariation = maxParticleForce * criticalVelocity;
			for (int i = 0; i < contactBuffer.Count; i++) {
				FluidParticleContact contact = contactBuffer [i];
				if ((contact.flags & FluidParticleType.TensileParticle) > 0) {
					float h = contact.a.weight + contact.b.weight;
					Vector2 s = contact.b.accumulation2 - contact.a.accumulation2;
					float fn = Mathf.Min (
					pressureStrength * (h - 2) + normalStrength * Vector2.Dot (s, contact.normal),
					maxVelocityVariation
					) * contact.weight;
					Vector2 f = fn * contact.normal;
					contact.a.velocity -= f;
					contact.b.velocity += f;
				}
			}
		}
	
		void solvePressure (float dt) {
			// calculates pressure as a linear function of density
			float criticalPressure = getCriticalPressure (dt);
			float pressurePerWeight = fluidSystemDef.pressureStrength * criticalPressure;
			float maxPressure = maxParticlePressure * criticalPressure;
			for (int i=0; i<count; i++) {
				FluidParticle fp = particleBuffer [i];
				float h = pressurePerWeight * Mathf.Max (0f, fp.weight - minParticleWeight);
				fp.accumulation = Mathf.Min (h, maxPressure);
			}
			//		// ignores particles which have their own repulsive force
			//		if (m_allParticleFlags & k_noPressureFlags)
			//		{
			//			for (int32 i = 0; i < m_count; i++)
			//			{
			//				if (m_flagsBuffer.data[i] & k_noPressureFlags)
			//				{
			//					m_accumulationBuffer[i] = 0;
			//				}
			//			}
			//		}
			// static pressure
			if ((allParticleFlags & FluidParticleType.StaticPressureParticle) > 0) {
				for (int i=0; i<count; i++) {
					FluidParticle fp = particleBuffer [i];
					if ((fp.flags & FluidParticleType.StaticPressureParticle) > 0) {
						fp.accumulation += fp.staticPressure;
					}
				}
			}
			// applies pressure between each particles in contact
			float velocityPerPressure = dt / (fluidSystemDef.density * particleDiameter);
			for (int i=0; i < bodyContactBuffer.Count; i++) {
				FluidParticleBodyContact contact = bodyContactBuffer [i];
				FluidParticle a = contact.fp;
				Rigidbody2D b = contact.body;
				Vector2 p = a.position;
				float h = a.accumulation + pressurePerWeight * contact.weight;
				Vector2 f = velocityPerPressure * contact.weight * contact.mass * h * contact.normal;
				a.velocity -= getParticleInvMass () * f;
				b.AddForceAtPosition (f / dt, p);
			}
			
			float strength = fluidSystemDef.temperaturMixingStrength;
			for (int i=0; i < contactBuffer.Count; i++) {
				FluidParticleContact contact = contactBuffer [i];
				FluidParticle a = contact.a;
				FluidParticle b = contact.b;

				float h = a.accumulation + b.accumulation;
				Vector2 f = velocityPerPressure * contact.weight * h * contact.normal;
				a.velocity -= f;
				b.velocity += f;
			
				float t = (b.temperature - a.temperature) * strength;
				a.temperature += t;
				b.temperature -= t;

				if (Mathf.Abs (a.mass - b.mass) >= 0.01f) {
					if (b.mass < a.mass) {
						a = contact.b;
						b = contact.a;
					}
					a.velocity.y += fluidSystemDef.densityStrength;
					b.velocity.y -= fluidSystemDef.densityStrength;
				}
				else if (Mathf.Abs (a.temperature - b.temperature) >= 0.1f){
					if (b.temperature > a.temperature) {
						a = contact.b;
						b = contact.a;
					}
					a.velocity.y += fluidSystemDef.temperatureStrength;
					b.velocity.y -= fluidSystemDef.temperatureStrength;
				}
			}
		}
	
		void solveDamping (float dt) {
			// reduces normal velocity of each contact
			float linearDamping = fluidSystemDef.dampingStrength;
			float quadraticDamping = 1 / getCriticalVelocity (dt);
			for (int i=0; i<bodyContactBuffer.Count; i++) {
				FluidParticleBodyContact contact = bodyContactBuffer [i];
				FluidParticle a = contact.fp;
				Vector2 p = a.position;
				Vector2 v = contact.body.velocity - a.velocity;
				float vn = Vector2.Dot (v, contact.normal);
				if (vn < 0) {
					float damping = Mathf.Max (linearDamping * contact.weight, Mathf.Min (-quadraticDamping * vn, 0.5f));
					Vector2 f = damping * contact.mass * vn * contact.normal;
					a.velocity += getParticleInvMass () * f;
					contact.body.AddForceAtPosition (-f / dt, p);
				}
			}
			for (int i=0; i<contactBuffer.Count; i++) {
				FluidParticleContact contact = contactBuffer [i];
				Vector2 v = contact.b.velocity - contact.a.velocity;
				float vn = Vector2.Dot (v, contact.normal);
				if (vn < 0) {
					float damping = Mathf.Max (linearDamping * contact.weight, Mathf.Min (-quadraticDamping * vn, 0.5f));
					Vector2 f = damping * vn * contact.normal;
					contact.a.velocity += f;
					contact.b.velocity -= f;
				}
			}
		}

		void solveViscous () {
			float viscousStrength = fluidSystemDef.viscousStrength;
			for (int i=0; i<bodyContactBuffer.Count; i++) {
				FluidParticleBodyContact contact = bodyContactBuffer [i];
				if ((contact.fp.flags & FluidParticleType.ViscousParticle) > 0) {
//				b2Vec2 v = b->GetLinearVelocityFromWorldPoint(p) -
//					m_velocityBuffer.data[a];
					Vector2 v = contact.body.velocity - contact.fp.velocity;
					Vector2 f = viscousStrength * contact.mass * contact.weight * v;
					contact.fp.velocity += getParticleInvMass () * f;
//				b->ApplyLinearImpulse(-f, p, true);
				}
			}
			for (int i = 0; i < contactBuffer.Count; i++) {
				FluidParticleContact contact = contactBuffer [i];
				if ((contact.flags & FluidParticleType.ViscousParticle) > 0) {
					Vector2 v = contact.b.velocity - contact.a.velocity;
					Vector2 f = viscousStrength * contact.weight * v;
					contact.a.velocity += f;
					contact.b.velocity -= f;
				}
			}
		}

		void limitVelocity (float dt) {
			float criticalVelocitySquared = getCriticalVelocitySquared (dt);
			for (int i=0; i<count; i++) {
				Vector2 v = particleBuffer [i].velocity;
				float v2 = v.sqrMagnitude;
				if (v2 > criticalVelocitySquared) {
					v *= Mathf.Sqrt (criticalVelocitySquared / v2);
				}
			}
		}

		void solveCollision (float dt) {
			// This function detects particles which are crossing boundary of bodies
			// and modifies velocities of them so that they will move just in front of
			// boundary. This function function also applies the reaction force to
			// bodies as precisely as the numerical stability is kept.
			FluidAABB aabb = new FluidAABB ();
			aabb.lowerBound.x = +float.MaxValue;
			aabb.lowerBound.y = +float.MaxValue;
			aabb.upperBound.x = -float.MaxValue;
			aabb.upperBound.y = -float.MaxValue;
			for (int i=0; i<count; i++) {
				FluidParticle fp = particleBuffer [i];
				Vector2 p1 = fp.position;
				Vector2 p2 = p1 + dt * fp.velocity;
				aabb.lowerBound = Vector2.Min (aabb.lowerBound, Vector2.Min (p1, p2));
				aabb.upperBound = Vector2.Max (aabb.upperBound, Vector2.Max (p1, p2));
			}

			SolveCollisionCallback callback = new SolveCollisionCallback (this, dt);
			queryAABB (callback, aabb);
		}

		void solveZombie () {
			// removes particles with zombie flag
			allParticleFlags = 0;
			for (int i=count-1; i>=0; i--) {
				FluidProxy proxy = proxyBuffer [i];
				if ((proxy.fp.flags & FluidParticleType.ZombieParticle) > 0) {
//				b2DestructionListener * const destructionListener =
//					m_world->m_destructionListener;
//				if ((flags & b2_destructionListenerParticle) &&
//				    destructionListener)
//				{
//					destructionListener->SayGoodbye(this, i);
//				}
//				Destroy particle handle.
//					if (m_handleIndexBuffer.data)
//				{
//					b2ParticleHandle * const handle = m_handleIndexBuffer.data[i];
//					if (handle)
//					{
//						handle->SetIndex(b2_invalidParticleIndex);
//						m_handleIndexBuffer.data[i] = NULL;
//						m_handleAllocator.Free(handle);
//					}
//				}
//				newIndices[i] = b2_invalidParticleIndex;
					particleBuffer.Remove (proxy.fp);
					proxyBuffer.Remove (proxy);
					count--;
				} else {
					allParticleFlags |= proxy.fp.flags;
				}
			}
			needsUpdateAllParticleFlags = false;
		
			// update contacts
//		for (int32 k = 0; k < m_contactBuffer.GetCount(); k++)
//		{
//			b2ParticleContact& contact = m_contactBuffer[k];
//			contact.SetIndices(newIndices[contact.GetIndexA()],
//			                   newIndices[contact.GetIndexB()]);
//		}
//		m_contactBuffer.RemoveIf(Test::IsContactInvalid);
		
//		// update particle-body contacts
//		for (int32 k = 0; k < m_bodyContactBuffer.GetCount(); k++)
//		{
//			b2ParticleBodyContact& contact = m_bodyContactBuffer[k];
//			contact.index = newIndices[contact.index];
//		}
//		m_bodyContactBuffer.RemoveIf(Test::IsBodyContactInvalid);
//		
//		// update pairs
//		for (int32 k = 0; k < m_pairBuffer.GetCount(); k++)
//		{
//			b2ParticlePair& pair = m_pairBuffer[k];
//			pair.indexA = newIndices[pair.indexA];
//			pair.indexB = newIndices[pair.indexB];
//		}
//		m_pairBuffer.RemoveIf(Test::IsPairInvalid);
//		
//		// update triads
//		for (int32 k = 0; k < m_triadBuffer.GetCount(); k++)
//		{
//			b2ParticleTriad& triad = m_triadBuffer[k];
//			triad.indexA = newIndices[triad.indexA];
//			triad.indexB = newIndices[triad.indexB];
//			triad.indexC = newIndices[triad.indexC];
//		}
//		m_triadBuffer.RemoveIf(Test::IsTriadInvalid);
//		
//		// Update lifetime indices.
//		if (m_indexByExpirationTimeBuffer.data)
//		{
//			int32 writeOffset = 0;
//			for (int32 readOffset = 0; readOffset < m_count; readOffset++)
//			{
//				const int32 newIndex = newIndices[
//				                                  m_indexByExpirationTimeBuffer.data[readOffset]];
//				if (newIndex != b2_invalidParticleIndex)
//				{
//					m_indexByExpirationTimeBuffer.data[writeOffset++] = newIndex;
//				}
//			}
//		}
//		
//		// update groups
//		for (b2ParticleGroup* group = m_groupList; group; group = group->GetNext())
//		{
//			int32 firstIndex = newCount;
//			int32 lastIndex = 0;
//			bool modified = false;
//			for (int32 i = group->m_firstIndex; i < group->m_lastIndex; i++)
//			{
//				int32 j = newIndices[i];
//				if (j >= 0) {
//					firstIndex = b2Min(firstIndex, j);
//					lastIndex = b2Max(lastIndex, j + 1);
//				} else {
//					modified = true;
//				}
//			}
//			if (firstIndex < lastIndex)
//			{
//				group->m_firstIndex = firstIndex;
//				group->m_lastIndex = lastIndex;
//				if (modified)
//				{
//					if (group->m_groupFlags & b2_solidParticleGroup)
//					{
//						SetGroupFlags(group,
//						              group->m_groupFlags |
//						              b2_particleGroupNeedsUpdateDepth);
//					}
//				}
//			}
//			else
//			{
//				group->m_firstIndex = 0;
//				group->m_lastIndex = 0;
//				if (!(group->m_groupFlags & b2_particleGroupCanBeEmpty))
//				{
//					SetGroupFlags(group,
//					              group->m_groupFlags | b2_particleGroupWillBeDestroyed);
//				}
//			}
//		}

			// destroy bodies with no particles
//		for (b2ParticleGroup* group = m_groupList; group;)
//		{
//			b2ParticleGroup* next = group->GetNext();
//			if (group->m_groupFlags & b2_particleGroupWillBeDestroyed)
//			{
//				DestroyParticleGroup(group);
//			}
//			group = next;
//		}
		}
	#endregion

//		void OnGUI () {
//			GUI.Label (new Rect (10, 50, 100, 100), count + "");
//
////		for (int i=0;i<count;i++) {
////			Handles.Label(proxyBuffer[i].fp.position, i+"");
////		}
//		}

#if UNITY_EDITOR && FLUID_DEBUG
	void OnDrawGizmos () {
		if (proxyBuffer != null && count > 0) {
			int i = 0;
			foreach (FluidProxy proxy in proxyBuffer) {
				FluidParticle fp = proxy.fp;
				Handles.color = fp.color;

				//Handles.DrawSolidDisc(new Vector3 (fp.position.x, fp.position.y, 0), Vector3.back, 0.05f);
				//Handles.DotCap (0, fp.position, Quaternion.identity, 0.05f);
				//Handles.DrawLine(fp.position,fp.position + fp.velocity * 0.1f);
				Handles.Label (fp.position, (int)fp.temperature+"");
			}
		}
	}
#endif

	#region CALLBACK

		class UpdateBodyContactsCallback : FluidFixtureParticleQueryCallback {
		
			public override void reportFixtureAndParticle (FluidFixtureShape fixture, FluidParticle fp) {
				float d;
				Vector2 n;
				fixture.computeDistance (fp, out d, out n);
				if ((d < fluidSystem.particleDiameter) && 
					((fp.flags & FluidParticleType.WallParticle) == 0)) {
					Rigidbody2D b = fixture.GetComponent<Rigidbody2D> ();
					//Vector2 bp = b.transform.position;
					float bm = b.mass;
				
					//				b2Vec2 bp = b->GetWorldCenter();
					//				float32 bI =
					//				b->GetInertia() - bm * b->GetLocalCenter().LengthSquared();
					float invBm = bm > 0 ? 1 / bm : 0;
					//				float32 invBI = bI > 0 ? 1 / bI : 0;
					float invAm = (fp.flags & FluidParticleType.WallParticle) > 0 ? 0 : fluidSystem.getParticleInvMass ();
					//Vector2 rp = fp.position - bp;
					//				float rpn = rp * n;
					//				float32 rpn = b2Cross(rp, n);
					//				float32 invM = invAm + invBm + invBI * rpn * rpn;
					float invM = invAm + invBm;

					    FluidParticleBodyContact contact = new FluidParticleBodyContact
					    {
					        fp = fp,
					        body = b,
					        fixture = fixture,
					        weight = 1 - d*fluidSystem.inverseDiameter,
					        normal = -n,
					        mass = invM > 0 ? 1/invM : 0
					    };

					    if (fixture.isTrigger ()) {
						fluidSystem.triggerContactBuffer.Add (contact);
						fixture.notifyOnTrigger (contact);
					} else {
						fluidSystem.bodyContactBuffer.Add (contact);
					}
					//				m_system->DetectStuckParticle(a);
				}
			}
		
			public UpdateBodyContactsCallback (FluidSystem fs) {
				fluidSystem = fs;
			}
		}
	
		class SolveCollisionCallback : FluidFixtureParticleQueryCallback {
			public float dt;
		
			public override void reportFixtureAndParticle (FluidFixtureShape fixture, FluidParticle fp) {
				//Rigidbody2D body = fixture.GetComponent<Rigidbody2D> ();
				Vector2 ap = fp.position;
				Vector2 av = fp.velocity;
				Ray2D ray = new Ray2D ();
				//RaycastHit2D hit;
				if (fluidSystem.iterationIndex == 0) {
					//					// Put 'ap' in the local space of the previous frame
					//					b2Vec2 p1 = b2MulT(body->m_xf0, ap);
					//					if (fixture->GetShape()->GetType() == b2Shape::e_circle)
					//					{
					//						// Make relative to the center of the circle
					//						p1 -= body->GetLocalCenter();
					//						// Re-apply rotation about the center of the
					//						// circle
					//						p1 = b2Mul(body->m_xf0.q, p1);
					//						// Subtract rotation of the current frame
					//						p1 = b2MulT(body->m_xf.q, p1);
					//						// Return to local space
					//						p1 += body->GetLocalCenter();
					//					}
					//					// Return to global space and apply rotation of current frame
					//					input.p1 = b2Mul(body->m_xf, p1);
					ray.origin = fp.prevPosition;
				} else {
					ray.origin = ap;
				}
				ray.direction = av;
			
				//hit = Physics2D.Raycast (ray.origin, ray.direction, av.magnitude * dt);
                Physics2D.Raycast(ray.origin, ray.direction, av.magnitude * dt);
                //if (hit) {
                //					b2Vec2 p =
                //						(1 - output.fraction) * input.p1 +
                //							output.fraction * input.p2 +
                //							b2_linearSlop * n;
                //Vector2 v = (hit.point - ap) / dt;
                //				fp.velocity = v;
                //Vector2 f = fluidSystem.getParticleMass () * (av - v);
                //				fluidSystem.particleApplyForce(fp, f);
                //}
            }
		
			public SolveCollisionCallback (FluidSystem fs, float dt) {
				fluidSystem = fs;
				this.dt = dt;
			}
		}
	#endregion
	}
}
