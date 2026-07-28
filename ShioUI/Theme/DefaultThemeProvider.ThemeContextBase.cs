using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;

using RiceTea.Core.Buffers;
using RiceTea.Core.Helpers;

namespace ShioUI.Theme;

#if NET8_0_OR_GREATER
using System.Collections.Frozen;
#endif

partial class DefaultThemeProvider
{
    private abstract class ThemeContextBase : IThemeContext
    {
#if NET8_0_OR_GREATER
        private readonly FrozenDictionary<string, IThemedColorFactory> _colorDict;
        private readonly FrozenDictionary<string, IThemedBrushFactory> _brushDict;
#else
        private readonly Dictionary<string, IThemedColorFactory> _colorDict;
        private readonly Dictionary<string, IThemedBrushFactory> _brushDict;
#endif
        private string _fontName;

        public abstract bool IsDarkTheme { get; }

        public string FontName
        {
            get => _fontName;
            set => _fontName = value;
        }

        protected ThemeContextBase()
        {
            _fontName = NullSafetyHelper.ThrowIfNull(SystemFonts.CaptionFont).Name;

            Dictionary<string, IThemedColorFactory> colorDict = new Dictionary<string, IThemedColorFactory>(StringHelper.OrdinalIgnoreCaseEqualityComparer);
            Dictionary<string, IThemedBrushFactory> brushDict = new Dictionary<string, IThemedBrushFactory>(StringHelper.OrdinalIgnoreCaseEqualityComparer);

            using PooledList<ThemedColorsBuildingHandler> colorsBuildingHandlerList = new(capacity: 0);
            using PooledList<ThemedBrushesBuildingHandler> brushesBuildingHandlerList = new(capacity: 0);

            OnThemeBuilding(colorsBuildingHandlerList, brushesBuildingHandlerList);

            using ArrayPool<ThemedColorsBuildingHandler>.RentScope colorsBuildingHandlersScope = colorsBuildingHandlerList.ToRentScope();
            using ArrayPool<ThemedBrushesBuildingHandler>.RentScope brushesBuildingHandlersScope = brushesBuildingHandlerList.ToRentScope();

            foreach (KeyValuePair<string, IThemedColorFactory> item in BuildColorFactories(QueryColorFunc))
                colorDict[item.Key] = item.Value;
            int count;
            if ((count = colorsBuildingHandlersScope.Count) > 0)
            {
                ref readonly ThemedColorsBuildingHandler colorsBuildingHandlersRef = ref colorsBuildingHandlersScope.GetReferenceOfFirstElement();
                int i = 0;
                do
                {
                    foreach (KeyValuePair<string, IThemedColorFactory> item in UnsafeHelper.AddTypedOffsetAsReadOnly(in colorsBuildingHandlersRef, i).Invoke(QueryColorFunc))
                        colorDict[item.Key] = item.Value;
                } while (++i < count);
            }
            foreach (KeyValuePair<string, IThemedBrushFactory> item in BuildBrushFactories(QueryColorFunc, QueryBrushFunc))
                brushDict[item.Key] = item.Value;
            if ((count = brushesBuildingHandlersScope.Count) > 0)
            {
                ref readonly ThemedBrushesBuildingHandler brushesBuildingHandlersRef = ref brushesBuildingHandlersScope.GetReferenceOfFirstElement();
                int i = 0;
                do
                {
                    foreach (KeyValuePair<string, IThemedBrushFactory> item in
                            UnsafeHelper.AddTypedOffsetAsReadOnly(in brushesBuildingHandlersRef, i).Invoke(QueryColorFunc, QueryBrushFunc))
                        brushDict[item.Key] = item.Value;
                } while (++i < count);
            }

#if NET8_0_OR_GREATER
            _colorDict = colorDict.ToFrozenDictionary(colorDict.Comparer);
            _brushDict = brushDict.ToFrozenDictionary(brushDict.Comparer);
#else            
            _colorDict = colorDict;
            _brushDict = brushDict;
#endif

            IThemedColorFactory QueryColorFunc(string key) => colorDict[key];

            IThemedBrushFactory QueryBrushFunc(string key) => brushDict[key];
        }

        protected ThemeContextBase(ThemeContextBase original)
        {
            _fontName = original._fontName;
            _colorDict = original._colorDict;
            _brushDict = original._brushDict;
        }

        public abstract IThemeContext Clone();

        protected abstract void OnThemeBuilding(
            PooledList<ThemedColorsBuildingHandler> colorsBuildingHandlerList,
            PooledList<ThemedBrushesBuildingHandler> brushesBuildingHandlerList);

        public bool TryGetBrushFactory(string node, [NotNullWhen(true)] out IThemedBrushFactory? brushFactory)
            => _brushDict.TryGetValue(node, out brushFactory);

        public bool TryGetColorFactory(string node, [NotNullWhen(true)] out IThemedColorFactory? colorFactory)
            => _colorDict.TryGetValue(node, out colorFactory);

        public IEnumerable<KeyValuePair<string, IThemedColorFactory>> EnumerateColorFactories() => _colorDict;

        public IEnumerable<KeyValuePair<string, IThemedBrushFactory>> EnumerateBrushFactories() => _brushDict;

        public abstract IEnumerable<KeyValuePair<string, IThemedColorFactory>> BuildColorFactories(Func<string, IThemedColorFactory> queryFunc);

        public abstract IEnumerable<KeyValuePair<string, IThemedBrushFactory>> BuildBrushFactories(
            Func<string, IThemedColorFactory> queryColorFunc, Func<string, IThemedBrushFactory> queryBrushFunc);

        private sealed class ApplyToOtherContextMethodClosureFast
        {
            private readonly ThemeContextBase _this;
            private readonly ThemeContextBase _otherContext;

            public ApplyToOtherContextMethodClosureFast(ThemeContextBase @this, ThemeContextBase otherContext)
            {
                _this = @this;
                _otherContext = otherContext;
            }

            public IThemedColorFactory GetColorFactory(string node)
            {
                if (_otherContext._colorDict.TryGetValue(node, out IThemedColorFactory? result))
                    return result;
                return _this._colorDict[node];
            }

            public IThemedBrushFactory GetBrushFactory(string node)
            {
                if (_otherContext._brushDict.TryGetValue(node, out IThemedBrushFactory? result))
                    return result;
                return _this._brushDict[node];
            }
        }

        private sealed class ApplyToOtherContextMethodClosureSlow
        {
            private readonly ThemeContextBase _this;
            private readonly IThemeContext _otherContext;

            public ApplyToOtherContextMethodClosureSlow(ThemeContextBase @this, IThemeContext otherContext)
            {
                _this = @this;
                _otherContext = otherContext;
            }

            public IThemedColorFactory GetColorFactory(string node)
            {
                if (_otherContext.TryGetColorFactory(node, out IThemedColorFactory? result))
                    return result;
                return _this._colorDict[node];
            }

            public IThemedBrushFactory GetBrushFactory(string node)
            {
                if (_otherContext.TryGetBrushFactory(node, out IThemedBrushFactory? result))
                    return result;
                return _this._brushDict[node];
            }
        }
    }
}