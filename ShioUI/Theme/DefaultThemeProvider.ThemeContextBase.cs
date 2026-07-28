using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;

using RiceTea.Core.Buffers;
using RiceTea.Core.Helpers;

namespace ShioUI.Theme;

partial class DefaultThemeProvider
{
    private abstract class ThemeContextBase : IThemeContext
    {
        private readonly Dictionary<string, IThemedColorFactory> _colorDict;
        private readonly Dictionary<string, IThemedBrushFactory> _brushDict;
        private readonly DefaultThemeColorsBuildingFunction[] _colorsBuildingFunctions;
        private readonly DefaultThemeBrushesBuildingFunction[] _brushesBuildingFunctions;

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

            DefaultThemeColorsBuildingFunction[] colorsBuildingFunctions;
            DefaultThemeBrushesBuildingFunction[] brushesBuildingFunctions;
            DefaultThemeBuildingEventHandler? externalBuildingHandler = GetExternalThemeBuildingHandler();
            if (externalBuildingHandler is null)
            {
                colorsBuildingFunctions = Array.Empty<DefaultThemeColorsBuildingFunction>();
                brushesBuildingFunctions = Array.Empty<DefaultThemeBrushesBuildingFunction>();
            }
            else
            {
                using PooledList<DefaultThemeColorsBuildingFunction> colorsBuildingFunctionList = new();
                using PooledList<DefaultThemeBrushesBuildingFunction> brushesBuildingFunctionList = new();
                externalBuildingHandler.Invoke(colorsBuildingFunctionList, brushesBuildingFunctionList);
                colorsBuildingFunctions = colorsBuildingFunctionList.ToArray();
                brushesBuildingFunctions = brushesBuildingFunctionList.ToArray();
            }

            foreach (KeyValuePair<string, IThemedColorFactory> item in CreateColorFactories(QueryColorFunc))
                colorDict[item.Key] = item.Value;
            int length;
            if ((length = colorsBuildingFunctions.Length) > 0)
            {
                ref readonly DefaultThemeColorsBuildingFunction colorsBuildingFunctionsRef = ref UnsafeHelper.GetArrayDataReference(colorsBuildingFunctions);
                int i = 0;
                do
                {
                    foreach (KeyValuePair<string, IThemedColorFactory> item in UnsafeHelper.AddTypedOffsetAsReadOnly(in colorsBuildingFunctionsRef, i).Invoke(QueryColorFunc))
                        colorDict[item.Key] = item.Value;
                } while (++i < length);
            }
            foreach (KeyValuePair<string, IThemedBrushFactory> item in CreateBrushFactories(QueryColorFunc, QueryBrushFunc))
                brushDict[item.Key] = item.Value;
            if ((length = brushesBuildingFunctions.Length) > 0)
            {
                ref readonly DefaultThemeBrushesBuildingFunction brushesBuildingFunctionsRef = ref UnsafeHelper.GetArrayDataReference(brushesBuildingFunctions);
                int i = 0;
                do
                {
                    foreach (KeyValuePair<string, IThemedBrushFactory> item in
                            UnsafeHelper.AddTypedOffsetAsReadOnly(in brushesBuildingFunctionsRef, i).Invoke(QueryColorFunc, QueryBrushFunc))
                        brushDict[item.Key] = item.Value;
                } while (++i < length);
            }

            _colorDict = colorDict;
            _brushDict = brushDict; 
            _colorsBuildingFunctions = colorsBuildingFunctions;
            _brushesBuildingFunctions = brushesBuildingFunctions;


            IThemedColorFactory QueryColorFunc(string key) => colorDict[key];

            IThemedBrushFactory QueryBrushFunc(string key) => brushDict[key];
        }

        protected ThemeContextBase(ThemeContextBase original)
        {
            _fontName = original._fontName;
            _colorDict = new(original._colorDict);
            _brushDict = new(original._brushDict); 
            _colorsBuildingFunctions = original._colorsBuildingFunctions;
            _brushesBuildingFunctions = original._brushesBuildingFunctions;
        }

        public abstract IThemeContext Clone();

        protected abstract DefaultThemeBuildingEventHandler? GetExternalThemeBuildingHandler();

        public bool TryGetBrushFactory(string node, [NotNullWhen(true)] out IThemedBrushFactory? brushFactory)
            => _brushDict.TryGetValue(node, out brushFactory);

        public bool TryGetColorFactory(string node, [NotNullWhen(true)] out IThemedColorFactory? colorFactory)
            => _colorDict.TryGetValue(node, out colorFactory);

        public bool TrySetBrushFactory(string node, IThemedBrushFactory brushFactory, bool overrides)
        {
            Dictionary<string, IThemedBrushFactory> brushDict = _brushDict;
            if (!overrides && brushDict.ContainsKey(node))
                return false;
            brushDict[node] = brushFactory;
            return true;
        }

        public bool TrySetColorFactory(string node, IThemedColorFactory colorFactory, bool overrides)
        {
            Dictionary<string, IThemedColorFactory> colorDict = _colorDict;
            if (!overrides && colorDict.ContainsKey(node))
                return false;
            colorDict[node] = colorFactory;
            return true;
        }

        public void BuildContextForAnother(IThemeContext other, bool overrides)
        {
            Func<string, IThemedColorFactory> queryColorFunc;
            Func<string, IThemedBrushFactory> queryBrushFunc;

            if (other is ThemeContextBase otherContextBase)
            {
                ApplyToOtherContextMethodClosureFast closure = new ApplyToOtherContextMethodClosureFast(this, otherContextBase);
                queryColorFunc = closure.GetColorFactory;
                queryBrushFunc = closure.GetBrushFactory;
            }
            else
            {
                ApplyToOtherContextMethodClosureSlow closure = new ApplyToOtherContextMethodClosureSlow(this, other);
                queryColorFunc = closure.GetColorFactory;
                queryBrushFunc = closure.GetBrushFactory;
            }

            DefaultThemeColorsBuildingFunction[] colorsBuildingFunctions = _colorsBuildingFunctions;
            DefaultThemeBrushesBuildingFunction[] brushesBuildingFunctions = _brushesBuildingFunctions;
            foreach (KeyValuePair<string, IThemedColorFactory> item in CreateColorFactories(queryColorFunc))
                other.TrySetColorFactory(item.Key, item.Value, overrides);
            int length;
            if ((length = colorsBuildingFunctions.Length) > 0)
            {
                ref readonly DefaultThemeColorsBuildingFunction colorsBuildingFunctionsRef = ref UnsafeHelper.GetArrayDataReference(colorsBuildingFunctions);
                int i = 0;
                do
                {
                    foreach (KeyValuePair<string, IThemedColorFactory> item in UnsafeHelper.AddTypedOffsetAsReadOnly(in colorsBuildingFunctionsRef, i).Invoke(queryColorFunc))
                        other.TrySetColorFactory(item.Key, item.Value, overrides);
                } while (++i < length);
            }
            foreach (KeyValuePair<string, IThemedBrushFactory> item in CreateBrushFactories(queryColorFunc, queryBrushFunc))
                other.TrySetBrushFactory(item.Key, item.Value, overrides);
            if ((length = brushesBuildingFunctions.Length) > 0)
            {
                ref readonly DefaultThemeBrushesBuildingFunction brushesBuildingFunctionsRef = ref UnsafeHelper.GetArrayDataReference(brushesBuildingFunctions);
                int i = 0;
                do
                {
                    foreach (KeyValuePair<string, IThemedBrushFactory> item in
                            UnsafeHelper.AddTypedOffsetAsReadOnly(in brushesBuildingFunctionsRef, i).Invoke(queryColorFunc, queryBrushFunc))
                        other.TrySetBrushFactory(item.Key, item.Value, overrides);
                } while (++i < length);
            }
        }

        protected abstract IEnumerable<KeyValuePair<string, IThemedColorFactory>> CreateColorFactories(Func<string, IThemedColorFactory> queryFunc);

        protected abstract IEnumerable<KeyValuePair<string, IThemedBrushFactory>> CreateBrushFactories(
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