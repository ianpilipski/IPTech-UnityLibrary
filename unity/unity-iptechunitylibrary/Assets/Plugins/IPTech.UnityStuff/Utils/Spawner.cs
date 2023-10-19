using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace IPTech.Unity.Utils
{
	public class Spawner : MonoBehaviour
	{
		public Transform spawnAtTransform;
		public bool ignoreTransformPosition = false;
		public bool ignoreTransformRotation = false;

		public GameObject prefabToSpawn;
		public bool SpawnOnStart = false;

		private void Start() {
			if (this.SpawnOnStart) {
				Spawn();
			}
		}

		public void Spawn() {
			GameObject instance = Instantiate(prefabToSpawn);
			SetSpawnPosition(instance);
			SetSpawnRotation(instance);
			instance.SetActive(true);
		}

		public void Spawn(Transform spawnAtTransform) {
			this.spawnAtTransform = spawnAtTransform;
			Spawn();
		}

		private void SetSpawnPosition(GameObject instance) {
			if (this.ignoreTransformPosition) return;

			if(this.spawnAtTransform!=null) {
				instance.transform.position = this.spawnAtTransform.position;
			}
		}

		private void SetSpawnRotation(GameObject instance) {
			if (this.ignoreTransformRotation) return;

			if(this.spawnAtTransform!=null) {
				instance.transform.rotation = this.spawnAtTransform.rotation;
			}
		}
	}
}