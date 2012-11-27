﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Zyan.Tests
{
	/// <summary>
	/// Dummy attribute used by unit test platform abstraction layer.
	/// </summary>
	[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
	internal class DummyAttribute : Attribute
	{
	}
}
