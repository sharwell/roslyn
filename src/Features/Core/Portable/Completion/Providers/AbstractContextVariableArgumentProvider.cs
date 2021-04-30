// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Operations;
using Microsoft.CodeAnalysis.Shared.Extensions;

namespace Microsoft.CodeAnalysis.Completion
{
    /// <summary>
    /// This <see cref="ArgumentProvider"/> attempts to locate a matching value in the context of a method invocation.
    /// </summary>
    internal abstract class AbstractContextVariableArgumentProvider : ArgumentProvider
    {
        public override Task ProvideArgumentAsync(ArgumentContext context)
        {
            if (context.PreviousValue is not null)
            {
                // This argument provider does not attempt to replace arguments already in code.
                return Task.CompletedTask;
            }

            var requireExactType = context.Parameter.Type.IsSpecialType()
                || context.Parameter.RefKind != RefKind.None;
            var symbols = context.SemanticModel.LookupSymbols(context.Position);

            // First try to find a local variable
            ISymbol? bestSymbol = null;
            CommonConversion bestConversion = default;

            (ISymbol outer, ISymbol inner)? bestNestedSymbol = null;
            CommonConversion bestNestedConversion = default;

            foreach (var symbol in symbols)
            {
                ISymbol candidate;
                if (symbol.IsKind(SymbolKind.Parameter, out IParameterSymbol? parameter))
                    candidate = parameter;
                else if (symbol.IsKind(SymbolKind.Local, out ILocalSymbol? local))
                    candidate = local;
                else
                    continue;

                CheckCandidate(candidate, checkNameForSpecialTypes: false, checkMembers: true);
            }

            if (bestSymbol is not null)
            {
                context.DefaultValue = bestSymbol.Name;
                return Task.CompletedTask;
            }

            foreach (var symbol in symbols)
            {
                ISymbol candidate;
                if (symbol.IsKind(SymbolKind.Field, out IFieldSymbol? field))
                    candidate = field;
                else if (symbol.IsKind(SymbolKind.Property, out IPropertySymbol? property))
                    candidate = property;
                else
                    continue;

                // Only check nested members of fields/properties if we couldn't find a matching nested member through
                // above through a local/parameter.
                CheckCandidate(candidate, checkNameForSpecialTypes: true, checkMembers: bestNestedSymbol is null);
            }

            if (bestSymbol is not null)
            {
                context.DefaultValue = bestSymbol.Name;
                return Task.CompletedTask;
            }

            if (bestNestedSymbol is not null)
            {
                context.DefaultValue = $"{bestNestedSymbol.Value.outer.Name}.{bestNestedSymbol.Value.inner.Name}";
                return Task.CompletedTask;
            }

            return Task.CompletedTask;

            // Local functions
            void CheckCandidate(ISymbol candidate, bool checkNameForSpecialTypes, bool checkMembers)
            {
                if (candidate.GetSymbolType() is not { } symbolType)
                {
                    return;
                }

                CheckOuterCandidate(candidate, symbolType, checkNameForSpecialTypes);

                if (checkMembers && bestSymbol is null)
                {
                    CheckInnerCandidates(candidate, symbolType);
                }
            }

            void CheckOuterCandidate(ISymbol candidate, ITypeSymbol symbolType, bool checkNameForSpecialTypes)
            {
                // Require a name match for special types
                if (checkNameForSpecialTypes
                    && candidate.GetSymbolType().IsSpecialType()
                    && !string.Equals(candidate.Name, context.Parameter.Name, StringComparison.OrdinalIgnoreCase))
                {
                    return;
                }

                if (requireExactType && !SymbolEqualityComparer.Default.Equals(context.Parameter.Type, symbolType))
                {
                    return;
                }

                var conversion = context.SemanticModel.Compilation.ClassifyCommonConversion(symbolType, context.Parameter.Type);
                if (!conversion.IsImplicit)
                {
                    return;
                }

                if (bestSymbol is not null)
                {
                    if (!IsNewConversionSameOrBetter(bestConversion, conversion))
                        return;

                    if (!IsNewNameSameOrBetter(context.Parameter, bestSymbol, candidate))
                        return;
                }

                bestSymbol = candidate;
                bestConversion = conversion;
            }

            void CheckInnerCandidates(ISymbol candidate, ITypeSymbol outerSymbolType)
            {
                if (outerSymbolType.OriginalDefinition.SpecialType == SpecialType.System_Nullable_T)
                {
                    // Avoid checking members of Nullable<T>, since it always allows `bool? x` to be passed to `bool` as
                    // `x.Value`. In some cases, this is what the user will want, but we take a conservative approach
                    // for this common case right now.
                    return;
                }

                if (context.SemanticModel.GetEnclosingNamedType(context.Position, context.CancellationToken) is not { } enclosingSymbol)
                {
                    return;
                }

                foreach (var member in outerSymbolType.GetAccessibleMembersInThisAndBaseTypes<ISymbol>(enclosingSymbol))
                {
                    // Always require a name match for special types when matching nested values
                    if (member.GetSymbolType().IsSpecialType()
                        && !string.Equals(member.Name, context.Parameter.Name, StringComparison.OrdinalIgnoreCase))
                    {
                        return;
                    }

                    if (member.Kind is not SymbolKind.Property or SymbolKind.Field
                        || member.GetSymbolType() is not { } symbolType)
                    {
                        continue;
                    }

                    if (requireExactType && !SymbolEqualityComparer.Default.Equals(context.Parameter.Type, symbolType))
                    {
                        continue;
                    }

                    var conversion = context.SemanticModel.Compilation.ClassifyCommonConversion(symbolType, context.Parameter.Type);
                    if (!conversion.IsImplicit)
                    {
                        continue;
                    }

                    if (bestNestedSymbol is not null)
                    {
                        if (!IsNewConversionSameOrBetter(bestNestedConversion, conversion))
                            continue;

                        if (!IsNewNameSameOrBetter(context.Parameter, bestNestedSymbol.Value.inner, member))
                            continue;
                    }

                    bestNestedSymbol = (candidate, member);
                    bestNestedConversion = conversion;
                }
            }

            static bool IsNewConversionSameOrBetter(CommonConversion bestConversion, CommonConversion conversion)
            {
                if (bestConversion.IsIdentity && !conversion.IsIdentity)
                    return false;

                if (bestConversion.IsImplicit && !conversion.IsImplicit)
                    return false;

                return true;
            }

            static bool IsNewNameSameOrBetter(IParameterSymbol parameter, ISymbol bestSymbol, ISymbol symbol)
            {
                if (string.Equals(bestSymbol.Name, parameter.Name))
                    return string.Equals(symbol.Name, parameter.Name);

                if (string.Equals(bestSymbol.Name, parameter.Name, StringComparison.OrdinalIgnoreCase))
                    return string.Equals(symbol.Name, parameter.Name, StringComparison.OrdinalIgnoreCase);

                return true;
            }
        }
    }
}
