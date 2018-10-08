using System;
using UnityEngine;

namespace Obi
{
	[Serializable]
	public abstract class ObiRopeRenderMode
	{		

		protected ObiRope rope = null;

		public ObiRope Rope{
			set{rope = value;}
		}

		public abstract void Initialize();
		public abstract void Update(Camera camera);
		public abstract void TearDown();
	}
}

