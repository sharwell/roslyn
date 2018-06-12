﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using UIAutomationClient;

namespace Microsoft.VisualStudio.IntegrationTest.Utilities.Interop.AutomationRetry
{
    internal static class AutomationRetryWrapper
    {
        private static readonly Dictionary<Type, Func<object, object>> _wrapperFunctions =
            new Dictionary<Type, Func<object, object>>
            {
                { typeof(IUIAutomationAndCondition), obj => new UIAutomationAndCondition((IUIAutomationAndCondition)obj).RuntimeCallableWrapper },
                { typeof(IUIAutomationAnnotationPattern), obj => new UIAutomationAnnotationPattern((IUIAutomationAnnotationPattern)obj).RuntimeCallableWrapper },
                { typeof(IUIAutomationBoolCondition), obj => new UIAutomationBoolCondition((IUIAutomationBoolCondition)obj).RuntimeCallableWrapper },
                { typeof(IUIAutomationCondition), obj => new UIAutomationCondition((IUIAutomationCondition)obj).RuntimeCallableWrapper },
                { typeof(IUIAutomationDockPattern), obj => new UIAutomationDockPattern((IUIAutomationDockPattern)obj).RuntimeCallableWrapper },
                { typeof(IUIAutomationDragPattern), obj => new UIAutomationDragPattern((IUIAutomationDragPattern)obj).RuntimeCallableWrapper },
                { typeof(IUIAutomationDropTargetPattern), obj => new UIAutomationDropTargetPattern((IUIAutomationDropTargetPattern)obj).RuntimeCallableWrapper },
                { typeof(IUIAutomationExpandCollapsePattern), obj => new UIAutomationExpandCollapsePattern((IUIAutomationExpandCollapsePattern)obj).RuntimeCallableWrapper },
                { typeof(IUIAutomationGridItemPattern), obj => new UIAutomationGridItemPattern((IUIAutomationGridItemPattern)obj).RuntimeCallableWrapper },
                { typeof(IUIAutomationGridPattern), obj => new UIAutomationGridPattern((IUIAutomationGridPattern)obj).RuntimeCallableWrapper },
                { typeof(IUIAutomationInvokePattern), obj => new UIAutomationInvokePattern((IUIAutomationInvokePattern)obj).RuntimeCallableWrapper },
                { typeof(IUIAutomationItemContainerPattern), obj => new UIAutomationItemContainerPattern((IUIAutomationItemContainerPattern)obj).RuntimeCallableWrapper },
                { typeof(IUIAutomationLegacyIAccessiblePattern), obj => new UIAutomationLegacyIAccessiblePattern((IUIAutomationLegacyIAccessiblePattern)obj).RuntimeCallableWrapper },
                { typeof(IUIAutomationMultipleViewPattern), obj => new UIAutomationMultipleViewPattern((IUIAutomationMultipleViewPattern)obj).RuntimeCallableWrapper },
                { typeof(IUIAutomationNotCondition), obj => new UIAutomationNotCondition((IUIAutomationNotCondition)obj).RuntimeCallableWrapper },
                { typeof(IUIAutomationObjectModelPattern), obj => new UIAutomationObjectModelPattern((IUIAutomationObjectModelPattern)obj).RuntimeCallableWrapper },
                { typeof(IUIAutomationOrCondition), obj => new UIAutomationOrCondition((IUIAutomationOrCondition)obj).RuntimeCallableWrapper },
                { typeof(IUIAutomationPropertyCondition), obj => new UIAutomationPropertyCondition((IUIAutomationPropertyCondition)obj).RuntimeCallableWrapper },
                { typeof(IUIAutomationRangeValuePattern), obj => new UIAutomationRangeValuePattern((IUIAutomationRangeValuePattern)obj).RuntimeCallableWrapper },
                { typeof(IUIAutomationScrollItemPattern), obj => new UIAutomationScrollItemPattern((IUIAutomationScrollItemPattern)obj).RuntimeCallableWrapper },
                { typeof(IUIAutomationScrollPattern), obj => new UIAutomationScrollPattern((IUIAutomationScrollPattern)obj).RuntimeCallableWrapper },
                { typeof(IUIAutomationSelectionItemPattern), obj => new UIAutomationSelectionItemPattern((IUIAutomationSelectionItemPattern)obj).RuntimeCallableWrapper },
                { typeof(IUIAutomationSelectionPattern), obj => new UIAutomationSelectionPattern((IUIAutomationSelectionPattern)obj).RuntimeCallableWrapper },
                { typeof(IUIAutomationSpreadsheetItemPattern), obj => new UIAutomationSpreadsheetItemPattern((IUIAutomationSpreadsheetItemPattern)obj).RuntimeCallableWrapper },
                { typeof(IUIAutomationSpreadsheetPattern), obj => new UIAutomationSpreadsheetPattern((IUIAutomationSpreadsheetPattern)obj).RuntimeCallableWrapper },
                { typeof(IUIAutomationStylesPattern), obj => new UIAutomationStylesPattern((IUIAutomationStylesPattern)obj).RuntimeCallableWrapper },
                { typeof(IUIAutomationSynchronizedInputPattern), obj => new UIAutomationSynchronizedInputPattern((IUIAutomationSynchronizedInputPattern)obj).RuntimeCallableWrapper },
                { typeof(IUIAutomationTableItemPattern), obj => new UIAutomationTableItemPattern((IUIAutomationTableItemPattern)obj).RuntimeCallableWrapper },
                { typeof(IUIAutomationTablePattern), obj => new UIAutomationTablePattern((IUIAutomationTablePattern)obj).RuntimeCallableWrapper },
                { typeof(IUIAutomationTextChildPattern), obj => new UIAutomationTextChildPattern((IUIAutomationTextChildPattern)obj).RuntimeCallableWrapper },
                { typeof(IUIAutomationTextPattern), obj => new UIAutomationTextPattern((IUIAutomationTextPattern)obj).RuntimeCallableWrapper },
                { typeof(IUIAutomationTextPattern2), obj => new UIAutomationTextPattern2((IUIAutomationTextPattern2)obj).RuntimeCallableWrapper },
                { typeof(IUIAutomationTogglePattern), obj => new UIAutomationTogglePattern((IUIAutomationTogglePattern)obj).RuntimeCallableWrapper },
                { typeof(IUIAutomationTransformPattern), obj => new UIAutomationTransformPattern((IUIAutomationTransformPattern)obj).RuntimeCallableWrapper },
                { typeof(IUIAutomationTransformPattern2), obj => new UIAutomationTransformPattern2((IUIAutomationTransformPattern2)obj).RuntimeCallableWrapper },
                { typeof(IUIAutomationValuePattern), obj => new UIAutomationValuePattern((IUIAutomationValuePattern)obj).RuntimeCallableWrapper },
                { typeof(IUIAutomationVirtualizedItemPattern), obj => new UIAutomationVirtualizedItemPattern((IUIAutomationVirtualizedItemPattern)obj).RuntimeCallableWrapper },
                { typeof(IUIAutomationWindowPattern), obj => new UIAutomationWindowPattern((IUIAutomationWindowPattern)obj).RuntimeCallableWrapper },
            };

        public static T WrapIfNecessary<T>(T value)
        {
            if (!_wrapperFunctions.TryGetValue(typeof(T), out var wrapperFunction))
            {
                // Objects which are not recognized automation objects are not wrapped
                return value;
            }

            return (T)wrapperFunction(value);
        }

        public static T Unwrap<T>(T automationObject)
        {
            if (automationObject is IRetryWrapper retryWrapper)
            {
                return (T)retryWrapper.WrappedObject;
            }

            return automationObject;
        }
    }
}
