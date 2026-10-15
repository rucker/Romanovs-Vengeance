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

using OpenRA.Mods.Common.Widgets;
using OpenRA.Network;
using OpenRA.Widgets;

namespace OpenRA.Mods.RA2.Widgets.Logic
{
	public class ResetSkirmishButtonLogic : ChromeLogic
	{
		[FluentReference]
		const string ResetSkirmishTitle = "dialog-reset-skirmish.title";

		[FluentReference]
		const string ResetSkirmishPrompt = "dialog-reset-skirmish.prompt";

		[FluentReference]
		const string ResetSkirmishAccept = "dialog-reset-skirmish.confirm";

		[FluentReference]
		const string ResetSkirmishCancel = "dialog-reset-skirmish.cancel";

		[ObjectCreator.UseCtor]
		public ResetSkirmishButtonLogic(Widget widget, ModData modData, OrderManager orderManager, bool skirmishMode)
		{
			var button = (ButtonWidget)widget;
			button.IsVisible = () => skirmishMode;
			button.IsDisabled = () =>
			{
				var localClient = orderManager.LocalClient;
				return localClient == null || !localClient.IsAdmin || localClient.IsReady;
			};

			button.OnClick = () => ConfirmationDialogs.ButtonPrompt(modData,
				title: ResetSkirmishTitle,
				text: ResetSkirmishPrompt,
				onConfirm: () => orderManager.IssueOrder(Order.Command("reset_skirmish")),
				confirmText: ResetSkirmishAccept,
				onCancel: () => { },
				cancelText: ResetSkirmishCancel);
		}
	}
}
