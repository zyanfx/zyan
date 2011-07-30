﻿using System;
using System.Collections.Generic;
using Zyan.Communication;

namespace Zyan.Communication.SessionMgmt
{
	#region Synonym definitions
#if !FX3
	using SessionDictionary = System.Collections.Concurrent.ConcurrentDictionary<Guid, ServerSession>;
	using SessionVariableDictionary = System.Collections.Concurrent.ConcurrentDictionary<Guid, System.Collections.Concurrent.ConcurrentDictionary<string, object>>;
	using VariableDictionary = System.Collections.Concurrent.ConcurrentDictionary<string, object>;
#else
	using SessionDictionary = Zyan.Communication.Toolbox.ConcurrentDictionary<Guid, ServerSession>;
	using SessionVariableDictionary = Zyan.Communication.Toolbox.ConcurrentDictionary<Guid, Zyan.Communication.Toolbox.ConcurrentDictionary<string, object>>;
	using VariableDictionary = Zyan.Communication.Toolbox.ConcurrentDictionary<string, object>;
#endif
	#endregion

	/// <summary>
	/// Slim in-process session manager.
	/// </summary>
	public class InProcSessionManager : SessionManagerBase
	{
		volatile SessionDictionary sessions = new SessionDictionary();
		volatile SessionVariableDictionary variables = new SessionVariableDictionary();

		/// <summary>
		/// Checks whether the given session exists.
		/// </summary>
		/// <param name="sessionID">Session unique identifier.</param>
		/// <returns>True if <see cref="ServerSession"/> with the given identifier exists, otherwise, false.</returns>
		public override bool ExistSession(Guid sessionID)
		{
			return sessions.ContainsKey(sessionID);
		}

		/// <summary>
		/// Returns <see cref="ServerSession"/> identified by sessionID.
		/// </summary>
		/// <param name="sessionID">Session unique identifier.</param>
		/// <returns><see cref="ServerSession"/> or null, if session with the given identifier is not found.</returns>
		public override ServerSession GetSessionBySessionID(Guid sessionID)
		{
			ServerSession session;

			if (sessions.TryGetValue(sessionID, out session))
			{
				return session;
			}

			return null;
		}

		/// <summary>
		/// Stores the given <see cref="ServerSession"/> to the session list.
		/// </summary>
		/// <param name="session"><see cref="ServerSession"/> to store.</param>
		public override void StoreSession(ServerSession session)
		{
			sessions[session.SessionID] = session;
		}

		/// <summary>
		/// Removes the given session from the session list.
		/// </summary>
		/// <param name="sessionID">Session unique identifier.</param>
		public override void RemoveSession(Guid sessionID)
		{
			ServerSession session;
			sessions.TryRemove(sessionID, out session);
		}

		/// <summary>
		/// Returns list of all sessions.
		/// </summary>
		protected override IEnumerable<ServerSession> AllSessions
		{
			get { return sessions.Values; }
		}

		/// <summary>
		/// Returns the value of the session variable.
		/// </summary>
		/// <param name="sessionID">Session unique identifier.</parparam>
		/// <param name="name">Variable name.</param>
		/// <returns>Value of the given session variable or null, if the variable is not defined.</returns>
		public override object GetSessionVariable(Guid sessionID, string name)
		{
			VariableDictionary sessionVars;

			if (variables.TryGetValue(sessionID, out sessionVars))
			{
				object value;

				if (sessionVars.TryGetValue(name, out value))
					return value;
			}

			return null;
		}

		/// <summary>
		/// Sets the new value of the session variable.
		/// </summary>
		/// <param name="sessionID">Session unique identifier.</parparam>
		/// <param name="name">Variable name.</param>
		/// <param name="value">Variable value.</param>
		public override void SetSessionVariable(Guid sessionID, string name, object value)
		{
			var sessionVars = variables.GetOrAdd(sessionID, x => new VariableDictionary());

			sessionVars[name] = value;
		}
	}
}
