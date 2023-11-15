using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace IPTech.Types
{
	[Serializable]
	public class RName : ISerializable
	{
		private static List<WeakReference> rNameList;

		public static RName Name(string name) {
			bool needsCacheCleaned = false;
			RName returnValue = null;
			foreach(WeakReference weakReference in RName.rNameList) {
				RName intName = (RName)weakReference.Target;
				if(intName!=null) { 
					if (intName.name.Equals(name, StringComparison.OrdinalIgnoreCase)) {
						returnValue = intName;
						break;
					}
				} else {
					needsCacheCleaned = true;
				}
			}

			if(needsCacheCleaned) {
				CleanCache();
			}

			return returnValue==null ? new RName(name) : returnValue;
		}

		public static int CacheSize() {
			return RName.rNameList.Count();
		}

		private readonly string name;

		static RName() {
			RName.rNameList = new List<WeakReference>();
		}

		private RName(string name) {
			this.name = name;
			RName.AddNameToCache(this);
		}

		protected RName(SerializationInfo info, StreamingContext context) {
			this.name = info.GetString("name");
			RName.AddNameToCache(this);
		}

		private static void AddNameToCache(RName rName) {
			RName.CleanCache();
			RName.rNameList.Add(new WeakReference(rName));
		}

		private static void CleanCache() {
			for(int i=RName.rNameList.Count-1;i>=0;i--) {
				WeakReference weakRef = RName.rNameList[i];
				if(!weakRef.IsAlive) {
					RName.rNameList.RemoveAt(i);
				}
			}
		}
		public override string ToString() {
			return this.name;
		}

		public void GetObjectData(SerializationInfo info, StreamingContext context) {
			info.AddValue("name", this.name);
		}

		/* Don't want this implict so that it's obvious that you are not getting the optimization
		 * of the RName when casting to string.
		public static implicit operator string(RName rname) {
			return rname.name;
		}
		*/
	}
}
