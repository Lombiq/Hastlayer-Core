﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ICSharpCode.NRefactory.CSharp;
using Orchard.Validation;

namespace Hast.Transformer.Services.ConstantValuesSubstitution
{
    internal class ConstantValuesTable
    {
        // The outer dictionary is keyed by value holder names. In the inner dictionary the scope is the key and 
        // the value is the primitive value.
        private Dictionary<string, Dictionary<AstNode, PrimitiveExpression>> _valueHoldersAndValueDescriptors =
            new Dictionary<string, Dictionary<AstNode, PrimitiveExpression>>();


        public ConstantValuesTable()
        {
        }

        private ConstantValuesTable(Dictionary<string, Dictionary<AstNode, PrimitiveExpression>> valueHoldersAndValueDescriptors)
        {
            _valueHoldersAndValueDescriptors = valueHoldersAndValueDescriptors;
        }


        /// <param name="scope">The node within the value should valid.</param>
        public void MarkAsPotentiallyConstant(
            AstNode valueHolder,
            PrimitiveExpression expression,
            AstNode scope,
            bool disallowDifferentValues = false)
        {
            Argument.ThrowIfNull(scope, nameof(scope));

            if (valueHolder == null) return;

            var valueDescriptors = GetOrCreateValueDescriptors(valueHolder.GetFullNameWithUnifiedPropertyName());

            if (disallowDifferentValues && expression != null)
            {
                PrimitiveExpression existingExpression;
                if (valueDescriptors.TryGetValue(scope, out existingExpression))
                {
                    // Simply using != would yield a reference equality check.
                    if (existingExpression == null || !expression.Value.Equals(existingExpression.Value))
                    {
                        expression = null;
                    }
                }
            }

            valueDescriptors[scope] = expression;
        }

        public void MarkAsNonConstant(AstNode valueHolder, AstNode scope)
        {
            if (valueHolder == null) return;

            Argument.ThrowIfNull(scope, nameof(scope));

            GetOrCreateValueDescriptors(valueHolder.GetFullNameWithUnifiedPropertyName())[scope] = null;
        }

        public bool RetrieveAndDeleteConstantValue(AstNode valueHolder, out PrimitiveExpression valueExpression)
        {
            Dictionary<AstNode, PrimitiveExpression> valueDescriptors;

            if (_valueHoldersAndValueDescriptors.TryGetValue(valueHolder.GetFullNameWithUnifiedPropertyName(), out valueDescriptors) &&
                valueDescriptors.Any())
            {
                // Finding the value defined for the scope which is closest.
                var closestValueDescriptorWithHeight = valueDescriptors
                    .Select(valueDescriptor =>
                    {
                        int height;
                        var parent = valueHolder.FindFirstParentOfType((AstNode node) => node == valueDescriptor.Key, out height);

                        return new
                        {
                            ValueDescriptor = valueDescriptor,
                            Height = parent != null ? height : int.MaxValue
                        };
                    })
                    .OrderBy(valueWithHeight => valueWithHeight.Height)
                    .First();

                valueExpression = closestValueDescriptorWithHeight.ValueDescriptor.Value;
                var suitableValueFound = valueExpression != null && closestValueDescriptorWithHeight.Height != int.MaxValue;
                if (suitableValueFound) valueDescriptors.Clear();
                return suitableValueFound;
            }

            valueExpression = null;
            return false;
        }

        public void Clear() => _valueHoldersAndValueDescriptors.Clear();

        public ConstantValuesTable Clone()
        {
            var valueHoldersAndValueDescriptors = new Dictionary<string, Dictionary<AstNode, PrimitiveExpression>>();

            foreach (var descriptor in _valueHoldersAndValueDescriptors)
            {
                var newScopeToExpression = valueHoldersAndValueDescriptors[descriptor.Key] = new Dictionary<AstNode, PrimitiveExpression>();

                foreach (var scopeToExpressions in descriptor.Value)
                {
                    newScopeToExpression[scopeToExpressions.Key] = scopeToExpressions.Value;
                }
            }

            return new ConstantValuesTable(valueHoldersAndValueDescriptors);
        }

        public void OverWrite(ConstantValuesTable source) =>
            _valueHoldersAndValueDescriptors = source._valueHoldersAndValueDescriptors;


        private Dictionary<AstNode, PrimitiveExpression> GetOrCreateValueDescriptors(string holderName)
        {
            Dictionary<AstNode, PrimitiveExpression> valueDescriptors;

            if (!_valueHoldersAndValueDescriptors.TryGetValue(holderName, out valueDescriptors))
            {
                valueDescriptors = new Dictionary<AstNode, PrimitiveExpression>();
                _valueHoldersAndValueDescriptors[holderName] = valueDescriptors;
            }

            return valueDescriptors;
        }
    }
}