using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace IPTech.Splines
{
	[CustomEditor(typeof(BezierCurveMover))]
	public class BezierCurveMoverEditor : Editor
	{
		private BezierCurveMover previewingMover = null;

		public override void OnInspectorGUI() {
			base.OnInspectorGUI();

			BezierCurveMover mover = target as BezierCurveMover;
			if (mover != null) {
				if (this.previewingMover == null) {
					if (GUILayout.Button("Preview Movement")) {
						BeginPreview(mover);
					}
				} else {
					if(GUILayout.Button("Cancel Preview")) {
						EndPreview();
					}
				}
			}
		}

		private void BeginPreview(BezierCurveMover mover) {
			if(this.previewingMover==null) {
				this.previewingMover = mover;
				this.previewingMover.Reset();
				EditorApplication.update += UpdatePreview;
			}
		}

		private void EndPreview() {
			this.previewingMover.Reset();
			this.previewingMover = null;
			EditorApplication.update -= UpdatePreview;
		}

		private void UpdatePreview() {
			if(this.previewingMover.PreviewMovementUpdate()) {
				EndPreview();
			}
		}
	}

}