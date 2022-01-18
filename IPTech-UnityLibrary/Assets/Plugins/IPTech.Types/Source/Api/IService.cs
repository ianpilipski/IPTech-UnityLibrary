using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IPTech.Types
{
	public interface IService : IIsReady
	{
		void Start();
		void Stop();
	}
}
