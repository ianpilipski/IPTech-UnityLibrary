using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace IPTech.UI
{
	public interface ITextView : IView {
		string Text { get; set; }
	}

	public class TextView : BaseView, ITextView	{
		public Text Text;

		string ITextView.Text
		{
			get { return Text==null ? null : Text.text; }
			set {
				if(Text != null) {
					this.Text.text = value;
				}
			}
		}

		protected virtual void Reset() {
			Text = GetComponent<Text>();
		}
	}
}
