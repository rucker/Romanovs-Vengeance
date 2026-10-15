#region Copyright & License Information
/*
 * Copyright (c) The OpenRA Developers and Contributors
 * This file is part of OpenRA, which is free software. It is made
 * available to you under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of
 * the License, or (at your option) any later version. For more
 * information, see COPYING.
 */
#endregion

using System.Linq;
using OpenRA.Mods.Common.Server;
using OpenRA.Network;
using OpenRA.Server;
using OpenRA.Traits;
using S = OpenRA.Server.Server;

namespace OpenRA.Mods.RA2.Server
{
	// Handles "reset_skirmish": restores the skirmish lobby to the state of a fresh skirmish
	// (default options, one default bot, default player slot settings), keeping the current map.
	// Each step reuses an existing lobby command so validation and syncing behave as usual,
	// and SkirmishLogic saves the resulting state.
	public class ResetSkirmishLogic : ServerTrait, IInterpretCommand
	{
		const string Command = "reset_skirmish";

		public bool InterpretCommand(S server, Connection conn, Session.Client client, string cmd)
		{
			if (cmd != Command)
				return false;

			if (server.Type != ServerType.Skirmish || client == null || !client.IsAdmin ||
				!LobbyCommands.ValidateCommand(server, conn, client, cmd))
				return true;

			lock (server.LobbyInfo)
			{
				// Remove all bots and reopen any closed slots
				foreach (var slot in server.LobbyInfo.Slots.Keys.ToList())
				{
					var occupant = server.LobbyInfo.ClientInSlot(slot);
					if (occupant != null ? occupant.Bot != null : server.LobbyInfo.Slots[slot].Closed)
						server.InterpretCommand($"slot_open {slot}", conn);
				}

				server.InterpretCommand("reset_options", conn);

				// Defaults for a newly joined client (see Server.cs)
				server.InterpretCommand($"faction {client.Index} Random", conn);
				server.InterpretCommand($"team {client.Index} 0", conn);
				server.InterpretCommand($"handicap {client.Index} 0", conn);
				if (client.SpawnPoint != 0)
					server.InterpretCommand($"clear_spawn {client.SpawnPoint}", conn);

				if (server.LobbyInfo.DisabledSpawnPoints.Count > 0)
				{
					server.LobbyInfo.DisabledSpawnPoints.Clear();
					server.SyncLobbyInfo();
				}

				// Add the same default bot as a fresh skirmish (see SkirmishLogic.ClientJoined)
				var botSlot = server.LobbyInfo.FirstEmptyBotSlot();
				var bot = server.Map.PlayerActorInfo.TraitInfos<IBotInfo>().Select(t => t.Type).FirstOrDefault();
				if (botSlot != null && bot != null)
					server.InterpretCommand($"slot_bot {botSlot} {client.Index} {bot}", conn);
			}

			return true;
		}
	}
}
