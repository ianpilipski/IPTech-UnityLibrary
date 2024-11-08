/*
	IPTech.Coroutines is a coroutine and debug visualizer library

    Copyright (C) 2019  Ian Pilipski

    This program is free software: you can redistribute it and/or modify
    it under the terms of the MIT license

    You should have received a copy of the MIT License
    along with this program.  If not, see <https://opensource.org/licenses/MIT>.
*/

using System;
using System.Collections.Generic;

namespace IPTech.Coroutines.Insights {
	public enum TimelineEntryState {
		Normal,
		Highlight,
		Attention
	}

	public interface ITimelineEntry {
		TimelineEntryState State { get; }
		string Name { get; }
		DateTime Start { get; }
		DateTime LastUpdated { get; }
		DateTime? End { get; }
		ITimelineEntry Parent { get; }
		IEnumerable<ITimelineEntry> Children { get; }
		
		string ExtendedInfo { get; }	
	}
}
