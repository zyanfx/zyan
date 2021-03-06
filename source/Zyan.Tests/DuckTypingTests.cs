﻿using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting;
using Zyan.Communication.Toolbox;
using Zyan.Communication;
using Zyan.Communication.Protocols.Null;

namespace Zyan.Tests
{
	#region Unit testing platform abstraction layer
#if NUNIT
	using NUnit.Framework;
	using TestClass = NUnit.Framework.TestFixtureAttribute;
	using TestMethod = NUnit.Framework.TestAttribute;
	using ClassInitializeNonStatic = NUnit.Framework.OneTimeSetUpAttribute;
	using ClassInitialize = DummyAttribute;
	using ClassCleanupNonStatic = NUnit.Framework.OneTimeTearDownAttribute;
	using ClassCleanup = DummyAttribute;
	using TestContext = System.Object;
#else
	using Microsoft.VisualStudio.TestTools.UnitTesting;
	using ClassInitializeNonStatic = DummyAttribute;
	using ClassCleanupNonStatic = DummyAttribute;
#endif
	#endregion

	/// <summary>
	/// Test cases for duck typing support.
	/// </summary>
	[TestClass]
	public class DuckTypingTests
	{
		#region Test classes and interfaces

		public interface IDuck
		{
			string Quack(string message, int duration);

			int Walk(int direction);

			void Swim();
		}

		public class RealDuck : IDuck
		{
			public string Quack(string message, int duration)
			{
				return message;
			}

			public int Walk(int direction)
			{
				return direction;
			}

			public void Swim()
			{
			}
		}

		public class ExplicitDuck : IDuck
		{
			string IDuck.Quack(string message, int duration)
			{
				return message;
			}

			int IDuck.Walk(int direction)
			{
				return direction;
			}

			public void Swim()
			{
			}
		}

		public class Platypus
		{
			public void Hunt(string animals)
			{
			}

			public string Quack(string message, int duration)
			{
				return "Growl";
			}

			public int Walk(int direction)
			{
				return direction;
			}

			public void Swim()
			{
			}
		}

		public class Chicken
		{
			public int Walk(int direction)
			{
				return direction;
			}

			public void Dispose()
			{
			}
		}

		#endregion

		#region Initialization and cleanup

		public TestContext TestContext { get; set; }

		static ZyanComponentHost ZyanHost { get; set; }

		static ZyanConnection ZyanConnection { get; set; }

		[ClassInitializeNonStatic]
		public void Initialize()
		{
			StartServer(null);
		}

		[ClassCleanupNonStatic]
		public void Cleanup()
		{
			StopServer();
		}

		[ClassInitialize]
		public static void StartServer(TestContext ctx)
		{
			var serverSetup = new NullServerProtocolSetup(1234);
			ZyanHost = new ZyanComponentHost("DuckTypingServer", serverSetup);

			// registration-time check
			ZyanHost.RegisterComponent<IDuck, Platypus>("Platypus");

			// invocation-time check (object factory can't be verified during registration)
			ZyanHost.RegisterComponent<IDuck>("Chicken", () => new Chicken()); 

			ZyanConnection = new ZyanConnection("null://NullChannel:1234/DuckTypingServer");
		}

		[ClassCleanup]
		public static void StopServer()
		{
			ZyanConnection.Dispose();
			ZyanHost.Dispose();
		}

		#endregion

		[TestMethod]
		public void TypeComparer_RealDuckIsAValidIDuck()
		{
			new TypeComparer<IDuck, RealDuck>().Validate();
		}

		[TestMethod]
		public void TypeComparer_ExplicitDuckIsAValidIDuck()
		{
			new TypeComparer<IDuck, ExplicitDuck>().Validate();
		}

		[TestMethod]
		public void TypeComparer_PlatypusIsNotAValidIDuck()
		{
			new TypeComparer<IDuck, Platypus>().Validate();
		}

		[TestMethod]
		public void TypeComparer_ChickenIsNotAValidIDuck()
		{
			Assert.Throws<MissingMethodException>(() =>
				new TypeComparer<IDuck, Chicken>().Validate());
		}

		[TestMethod]
		public void TypeComparer_ChickenIsDisposable()
		{
			new TypeComparer<IDisposable, Chicken>().Validate();
		}

		[TestMethod]
		public void TypeComparer_PlatypusIsNotDisposable()
		{
			Assert.Throws<MissingMethodException>(() =>
				new TypeComparer<IDisposable, Platypus>().Validate());
		}

		[TestMethod]
		public void ZyanComponentHost_ChickenWillNotBeRegisteredV1()
		{
			Assert.Throws<MissingMethodException>(() =>
				ZyanHost.RegisterComponent<IDuck, Chicken>());
		}

		[TestMethod]
		public void ZyanComponentHost_ChickenWillNotBeRegisteredV2()
		{
			Assert.Throws<MissingMethodException>(() =>
				ZyanHost.RegisterComponent<IDuck, Chicken>(new Chicken()));
		}

		[TestMethod]
		public void ZyanComponentHost_ChickenCannotQuack()
		{
			var proxy = ZyanConnection.CreateProxy<IDuck>("Chicken");
			Assert.Throws<MissingMethodException>(() =>
				proxy.Quack(String.Empty, default(int)));
		}

		[TestMethod]
		public void ZyanComponentHost_PlatypusCanQuack()
		{
			var proxy = ZyanConnection.CreateProxy<IDuck>("Platypus");
			var quack = proxy.Quack(String.Empty, default(int));

			Assert.AreEqual("Growl", quack);
		}
	}
}
