using strange.extensions.context.impl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IPTech.Strange
{
	class BasicBootstrap : ContextView
	{
		void Start() {
			this.context = new Context(this);
		}
	}
}
