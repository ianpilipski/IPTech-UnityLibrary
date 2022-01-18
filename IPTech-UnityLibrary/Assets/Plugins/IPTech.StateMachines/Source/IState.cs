using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IPTech.StateMachines {
	public interface IState {
		IEnumerator BeginState();
		IEnumerator StateTick();
		IEnumerator EndState();
	}
}
