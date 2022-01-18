using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace IPTech.Strange.Views
{
	class AmountViewAnimParameter : AmountViewBase
	{
		private AnimatorControllerParameter param;

		public string parameterName = null;
		public Animator animator = null;
		
		protected override void Start() {
			this.param = this.animator.parameters.First(p => p.name == parameterName);
			UpdateVisuals();
			base.Start();
		}

		protected void OnEnable() {
			UpdateVisuals();
		}

		protected override void UpdateVisuals() {
			if(!this.animator.isInitialized) {
				return;
			}

			if (this.param == null || this.Amount==null) return;

			switch(this.param.type) {
				case AnimatorControllerParameterType.Bool:
					this.animator.SetBool(parameterName, (bool)Convert.ChangeType(this.Amount, typeof(bool)));
					break;
				case AnimatorControllerParameterType.Float:
					this.animator.SetFloat(parameterName, (float)Convert.ChangeType(this.Amount, typeof(float)));
					break;
				case AnimatorControllerParameterType.Int:
					this.animator.SetInteger(parameterName, (int)Convert.ChangeType(this.Amount, typeof(int)));
					break;
				default:
					break;
			}
		}
	}
}
