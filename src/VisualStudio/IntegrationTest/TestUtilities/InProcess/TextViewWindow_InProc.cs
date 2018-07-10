﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Editor.Implementation.Suggestions;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Operations;
using Microsoft.VisualStudio.Text.Tagging;
using Roslyn.Utilities;
using OLECMDEXECOPT = Microsoft.VisualStudio.OLE.Interop.OLECMDEXECOPT;
using ThreadHelper = Microsoft.VisualStudio.Shell.ThreadHelper;

namespace Microsoft.VisualStudio.IntegrationTest.Utilities.InProcess
{
    internal abstract class TextViewWindow_InProc : InProcComponent
    {
        public void ShowLightBulb()
        {
            InvokeOnUIThread(() =>
            {
                var shell = GetGlobalService<SVsUIShell, IVsUIShell>();
                var cmdGroup = typeof(VSConstants.VSStd2KCmdID).GUID;
                var cmdExecOpt = OLECMDEXECOPT.OLECMDEXECOPT_DONTPROMPTUSER;

                const VSConstants.VSStd2KCmdID ECMD_SMARTTASKS = (VSConstants.VSStd2KCmdID)147;
                var cmdID = ECMD_SMARTTASKS;
                object obj = null;
                shell.PostExecCommand(cmdGroup, (uint)cmdID, (uint)cmdExecOpt, ref obj);
            });
        }

        public void WaitForLightBulbSession()
        {
            ThreadHelper.JoinableTaskFactory.Run(async () =>
            {
                await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

                var view = GetActiveTextView();
                var broker = GetComponentModel().GetService<ILightBulbBroker>();
                await LightBulbHelper.WaitForLightBulbSessionAsync(broker, view);
            });
        }

        protected abstract ITextBuffer GetBufferContainingCaret(IWpfTextView view);

        public void PlaceCaret(
            string marker,
            int charsOffset,
            int occurrence,
            bool extendSelection,
            bool selectBlock)
            => ExecuteOnActiveView(view =>
            {
                var dte = GetDTE();
                dte.Find.FindWhat = marker;
                dte.Find.MatchCase = true;
                dte.Find.MatchInHiddenText = true;
                dte.Find.Target = EnvDTE.vsFindTarget.vsFindTargetCurrentDocument;
                dte.Find.Action = EnvDTE.vsFindAction.vsFindActionFind;

                var originalPosition = GetCaretPosition();
                view.Caret.MoveTo(new SnapshotPoint(GetBufferContainingCaret(view).CurrentSnapshot, 0));

                if (occurrence > 0)
                {
                    var result = EnvDTE.vsFindResult.vsFindResultNotFound;
                    for (var i = 0; i < occurrence; i++)
                    {
                        result = dte.Find.Execute();
                    }

                    if (result != EnvDTE.vsFindResult.vsFindResultFound)
                    {
                        throw new Exception("Occurrence " + occurrence + " of marker '" + marker + "' not found in text: " + view.TextSnapshot.GetText());
                    }
                }
                else
                {
                    var result = dte.Find.Execute();
                    if (result != EnvDTE.vsFindResult.vsFindResultFound)
                    {
                        throw new Exception("Marker '" + marker + "' not found in text: " + view.TextSnapshot.GetText());
                    }
                }

                if (charsOffset > 0)
                {
                    for (var i = 0; i < charsOffset - 1; i++)
                    {
                        view.Caret.MoveToNextCaretPosition();
                    }

                    view.Selection.Clear();
                }

                if (charsOffset < 0)
                {
                    // On the first negative charsOffset, move to anchor-point position, as if the user hit the LEFT key
                    view.Caret.MoveTo(new SnapshotPoint(view.TextSnapshot, view.Selection.AnchorPoint.Position.Position));

                    for (var i = 0; i < -charsOffset - 1; i++)
                    {
                        view.Caret.MoveToPreviousCaretPosition();
                    }

                    view.Selection.Clear();
                }

                if (extendSelection)
                {
                    var newPosition = view.Selection.ActivePoint.Position.Position;
                    view.Selection.Select(new VirtualSnapshotPoint(view.TextSnapshot, originalPosition), new VirtualSnapshotPoint(view.TextSnapshot, newPosition));
                    view.Selection.Mode = selectBlock ? TextSelectionMode.Box : TextSelectionMode.Stream;
                }
            });

        public int GetCaretPosition()
            => ExecuteOnActiveView(view =>
            {
                var subjectBuffer = GetBufferContainingCaret(view);
                var bufferPosition = view.Caret.Position.BufferPosition;
                return bufferPosition.Position;
            });

        protected T ExecuteOnActiveView<T>(Func<IWpfTextView, T> action)
            => InvokeOnUIThread(() =>
            {
                var view = GetActiveTextView();
                return action(view);
            });

        protected void ExecuteOnActiveView(Action<IWpfTextView> action)
            => InvokeOnUIThread(GetExecuteOnActionViewCallback(action));

        protected Action GetExecuteOnActionViewCallback(Action<IWpfTextView> action)
            => () =>
            {
                var view = GetActiveTextView();
                action(view);
            };

        public string GetQuickInfo()
            => ExecuteOnActiveView(view =>
            {
#pragma warning disable CS0618 // IQuickInfo* is obsolete, tracked by https://github.com/dotnet/roslyn/issues/24094
                var broker = GetComponentModelService<IQuickInfoBroker>();
#pragma warning restore CS0618 // IQuickInfo* is obsolete, tracked by https://github.com/dotnet/roslyn/issues/24094

                var sessions = broker.GetSessions(view);
                if (sessions.Count != 1)
                {
                    throw new InvalidOperationException($"Expected exactly one QuickInfo session, but found {sessions.Count}");
                }

                return QuickInfoToStringConverter.GetStringFromBulkContent(sessions[0].QuickInfoContent);
            });

        public bool IsLightBulbSessionExpanded()
       => ExecuteOnActiveView(view =>
       {
           var broker = GetComponentModel().GetService<ILightBulbBroker>();

           if (!broker.IsLightBulbSessionActive(view))
           {
               return false;
           }

           var session = broker.GetSession(view);
           if (session == null || !session.IsExpanded)
           {
               return false;
           }

           return true;
       });

        public string[] GetLightBulbActions()
        {
            return ThreadHelper.JoinableTaskFactory.Run(async () =>
            {
                await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

                var view = GetActiveTextView();
                var broker = GetComponentModel().GetService<ILightBulbBroker>();
                return (await GetLightBulbActionsAsync(broker, view)).Select(a => a.DisplayText).ToArray();
            });
        }

        private async Task<IEnumerable<ISuggestedAction>> GetLightBulbActionsAsync(ILightBulbBroker broker, IWpfTextView view)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            if (!broker.IsLightBulbSessionActive(view))
            {
                var bufferType = view.TextBuffer.ContentType.DisplayName;
                throw new Exception(string.Format("No light bulb session in View!  Buffer content type={0}", bufferType));
            }

            var activeSession = broker.GetSession(view);
            if (activeSession == null || !activeSession.IsExpanded)
            {
                var bufferType = view.TextBuffer.ContentType.DisplayName;
                throw new InvalidOperationException(string.Format("No expanded light bulb session found after View.ShowSmartTag.  Buffer content type={0}", bufferType));
            }

            if (activeSession.TryGetSuggestedActionSets(out var actionSets) != QuerySuggestedActionCompletionStatus.Completed)
            {
                actionSets = Array.Empty<SuggestedActionSet>();
            }

            return await SelectActionsAsync(actionSets);
        }

        public void ApplyLightBulbAction(string actionName, FixAllScope? fixAllScope, bool blockUntilComplete)
        {
            var lightBulbAction = GetLightBulbApplicationAction(actionName, fixAllScope, blockUntilComplete);
            var task = ThreadHelper.JoinableTaskFactory.RunAsync(async () =>
            {
                await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

                var activeTextView = GetActiveTextView();
                await lightBulbAction(activeTextView);
            });

            if (blockUntilComplete)
            {
                task.Join();
            }
        }

        private Func<IWpfTextView, Task> GetLightBulbApplicationAction(string actionName, FixAllScope? fixAllScope, bool willBlockUntilComplete)
        {
            return async view =>
            {
                await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

                var broker = GetComponentModel().GetService<ILightBulbBroker>();

                var actions = (await GetLightBulbActionsAsync(broker, view)).ToArray();
                var action = actions.FirstOrDefault(a => a.DisplayText == actionName);

                if (action == null)
                {
                    var sb = new StringBuilder();
                    foreach (var item in actions)
                    {
                        sb.AppendLine("Actual ISuggestedAction: " + item.DisplayText);
                    }

                    var bufferType = view.TextBuffer.ContentType.DisplayName;
                    throw new InvalidOperationException(
                        string.Format("ISuggestedAction {0} not found.  Buffer content type={1}\r\nActions: {2}", actionName, bufferType, sb.ToString()));
                }

                if (fixAllScope != null)
                {
                    if (!action.HasActionSets)
                    {
                        throw new InvalidOperationException($"Suggested action '{action.DisplayText}' does not support FixAllOccurrences.");
                    }

                    var actionSetsForAction = await action.GetActionSetsAsync(CancellationToken.None);
                    action = await GetFixAllSuggestedActionAsync(actionSetsForAction, fixAllScope.Value);
                    if (action == null)
                    {
                        throw new InvalidOperationException($"Unable to find FixAll in {fixAllScope.ToString()} code fix for suggested action '{action.DisplayText}'.");
                    }

                    if (willBlockUntilComplete
                        && action is FixAllSuggestedAction fixAllSuggestedAction
                        && fixAllSuggestedAction.CodeAction is FixSomeCodeAction fixSomeCodeAction)
                    {
                        // Ensure the preview changes dialog will not be shown. Since the operation 'willBlockUntilComplete',
                        // the caller would not be able to interact with the preview changes dialog, and the tests would
                        // either timeout or deadlock.
                        fixSomeCodeAction.GetTestAccessor().ShowPreviewChangesDialog = false;
                    }

                    if (string.IsNullOrEmpty(actionName))
                    {
                        return;
                    }

                    // Dismiss the lightbulb session as we not invoking the original code fix.
                    broker.DismissSession(view);
                }

                action.Invoke(CancellationToken.None);
            };
        }

        private async Task<IEnumerable<ISuggestedAction>> SelectActionsAsync(IEnumerable<SuggestedActionSet> actionSets)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            var actions = new List<ISuggestedAction>();

            if (actionSets != null)
            {
                foreach (var actionSet in actionSets)
                {
                    if (actionSet.Actions != null)
                    {
                        foreach (var action in actionSet.Actions)
                        {
                            actions.Add(action);
                            var nestedActionSets = await action.GetActionSetsAsync(CancellationToken.None);
                            var nestedActions = await SelectActionsAsync(nestedActionSets);
                            actions.AddRange(nestedActions);
                        }
                    }
                }
            }

            return actions;
        }

        private static async Task<FixAllSuggestedAction> GetFixAllSuggestedActionAsync(IEnumerable<SuggestedActionSet> actionSets, FixAllScope fixAllScope)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            foreach (var actionSet in actionSets)
            {
                foreach (var action in actionSet.Actions)
                {
                    if (action is FixAllSuggestedAction fixAllSuggestedAction)
                    {
                        var fixAllCodeAction = fixAllSuggestedAction.CodeAction as FixSomeCodeAction;
                        if (fixAllCodeAction?.FixAllState?.Scope == fixAllScope)
                        {
                            return fixAllSuggestedAction;
                        }
                    }

                    if (action.HasActionSets)
                    {
                        var nestedActionSets = await action.GetActionSetsAsync(CancellationToken.None);
                        fixAllSuggestedAction = await GetFixAllSuggestedActionAsync(nestedActionSets, fixAllScope);
                        if (fixAllSuggestedAction != null)
                        {
                            return fixAllSuggestedAction;
                        }
                    }
                }
            }

            return null;
        }

        protected abstract IWpfTextView GetActiveTextView();
    }
}
