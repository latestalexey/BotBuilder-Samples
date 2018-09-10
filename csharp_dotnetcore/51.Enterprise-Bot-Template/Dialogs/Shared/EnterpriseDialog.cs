﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Threading;
using System.Threading.Tasks;
using Luis;
using Microsoft.Bot.Builder.Dialogs;

namespace EnterpriseBot
{
    public class EnterpriseDialog : InterruptableDialog
    {
        // Fields
        private readonly BotServices _services;
        private readonly CancelResponses _responder = new CancelResponses();

        public EnterpriseDialog(BotServices botServices, string dialogId)
            : base(dialogId)
        {
            _services = botServices;

            AddDialog(new CancelDialog());
        }

        protected override async Task<InterruptionStatus> OnDialogInterruptionAsync(DialogContext dc, CancellationToken cancellationToken)
        {
            // check dispatch intent
            // TODO: use luis gen models
            var luisService = _services.LuisServices[typeof(EnterpriseBot_General).Name];
            var luisResult = await luisService.RecognizeAsync<EnterpriseBot_General>(dc.Context, cancellationToken);
            var intent = luisResult.TopIntent().intent;

            switch (intent)
            {
                case EnterpriseBot_General.Intent.Cancel:
                    {
                        return await OnCancel(dc);
                    }

                case EnterpriseBot_General.Intent.Help:
                    {
                        return await OnHelp(dc);
                    }
            }

            return InterruptionStatus.NoAction;
        }

        protected virtual async Task<InterruptionStatus> OnCancel(DialogContext dc)
        {
            if (dc.ActiveDialog.Id != CancelDialog.Name)
            {
                // Don't start restart cancel dialog
                await dc.BeginAsync(CancelDialog.Name);

                // Signal that the dialog is waiting on user response
                return InterruptionStatus.Waiting;
            }

            // Else, continue
            return InterruptionStatus.NoAction;
        }

        protected virtual async Task<InterruptionStatus> OnHelp(DialogContext dc)
        {
            var view = new MainResponses();
            await view.ReplyWith(dc.Context, MainResponses.Help);

            // Signal the conversation was interrupted and should immediately continue
            return InterruptionStatus.Interrupted;
        }
    }
}