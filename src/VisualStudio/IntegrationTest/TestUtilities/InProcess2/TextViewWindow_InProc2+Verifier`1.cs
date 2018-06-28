﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Shared.TestHooks;
using Xunit;

namespace Microsoft.VisualStudio.IntegrationTest.Utilities.InProcess2
{
    partial class TextViewWindow_InProc2
    {
        public class Verifier<TTextViewWindow>
            where TTextViewWindow : TextViewWindow_InProc2
        {
            protected readonly TTextViewWindow _textViewWindow;
            VisualStudioWorkspace_InProc2 _workspace;

            public Verifier(TTextViewWindow textViewWindow, VisualStudioWorkspace_InProc2 workspace)
            {
                _textViewWindow = textViewWindow;
                _workspace = workspace;
            }

            public async Task CodeActionAsync(
                string expectedItem,
                bool applyFix = false,
                bool verifyNotShowing = false,
                bool ensureExpectedItemsAreOrdered = false,
                FixAllScope? fixAllScope = null,
                bool willBlockUntilComplete = true)
            {
                var expectedItems = new[] { expectedItem };
                await CodeActionsAsync(expectedItems, applyFix ? expectedItem : null, verifyNotShowing,
                    ensureExpectedItemsAreOrdered, fixAllScope, willBlockUntilComplete);
            }

            public async Task CodeActionsAsync(
                IEnumerable<string> expectedItems,
                string applyFix = null,
                bool verifyNotShowing = false,
                bool ensureExpectedItemsAreOrdered = false,
                FixAllScope? fixAllScope = null,
                bool willBlockUntilComplete = true)
            {
                await _textViewWindow.ShowLightBulbAsync();
                await _textViewWindow.WaitForLightBulbSessionAsync();

                if (verifyNotShowing)
                {
                    await CodeActionsNotShowingAsync();
                    return;
                }

                var actions = await _textViewWindow.GetLightBulbActionsAsync();

                if (expectedItems != null && expectedItems.Any())
                {
                    if (ensureExpectedItemsAreOrdered)
                    {
                        TestUtilities.ThrowIfExpectedItemNotFoundInOrder(
                            actions,
                            expectedItems);
                    }
                    else
                    {
                        TestUtilities.ThrowIfExpectedItemNotFound(
                            actions,
                            expectedItems);
                    }
                }

                if (!string.IsNullOrEmpty(applyFix) || fixAllScope.HasValue)
                {
                    await _textViewWindow.ApplyLightBulbActionAsync(applyFix, fixAllScope, willBlockUntilComplete);

                    // wait for action to complete
                    await _workspace.WaitForAsyncOperationsAsync(FeatureAttribute.LightBulb);
                }
            }

            public async Task CodeActionsNotShowingAsync()
            {
                if (await _textViewWindow.IsLightBulbSessionExpandedAsync())
                {
                    throw new InvalidOperationException("Expected no light bulb session, but one was found.");
                }
            }

#if false
            public void CurrentTokenType(string tokenType)
            {
                _instance.Workspace.WaitForAsyncOperations(FeatureAttribute.SolutionCrawler);
                _instance.Workspace.WaitForAsyncOperations(FeatureAttribute.DiagnosticService);
                _instance.Workspace.WaitForAsyncOperations(FeatureAttribute.Classification);
                var actualTokenTypes = _textViewWindow.GetCurrentClassifications();
                Assert.Equal(actualTokenTypes.Length, 1);
                Assert.Contains(tokenType, actualTokenTypes[0]);
                Assert.NotEqual("text", tokenType);
            }

            public void CompletionItemsExist(params string[] expectedItems)
            {
                var completionItems = _textViewWindow.GetCompletionItems();
                foreach (var expectedItem in expectedItems)
                {
                    Assert.Contains(expectedItem, completionItems);
                }
            }

            public void CompletionItemsDoNotExist( params string[] unexpectedItems)
            {
                var completionItems = _textViewWindow.GetCompletionItems();
                foreach (var unexpectedItem in unexpectedItems)
                {
                    Assert.DoesNotContain(unexpectedItem, completionItems);
                }
            }
#endif

            public async Task CaretPositionAsync(int expectedCaretPosition)
            {
                var position = await _textViewWindow.GetCaretPositionAsync();
                Assert.Equal(expectedCaretPosition, position);
            }
        }
    }
}
