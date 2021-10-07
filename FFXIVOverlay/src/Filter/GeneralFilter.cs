using ff14bot.Objects;
using System;
using System.Collections.Generic;

using DrawingContext = FFXIVOverlay.Overlay.DrawingContext;
using YamLikeCommand = YamLikeConfig.Command;

namespace FFXIVOverlay.Command
{
    public interface GeneralFilterTestInterface
    {
        bool Init(string value);
        bool Validate(GameObject go);
    }
    public class GeneralFilterFactory
    {
        static Dictionary<string, Func<string, GeneralFilterTestInterface>> _filters = null;
        static void init()
        {
            if (_filters != null)
                return;

            _filters = new Dictionary<string, Func<string, GeneralFilterTestInterface>>();
            regTypes();
        }

        static void Reg<T>(string name) where T : GeneralFilterTestInterface, new()
        {
            string shortName = name.Trim().ToLower();
            _filters[shortName] = (string value) =>
            {
                T t = new T();
                if (!t.Init(value))
                    return null;

                return t;
            };
        }

        public static GeneralFilterTestInterface create(string name, string value)
        {
            if (string.IsNullOrWhiteSpace(name))
                return null;

            init();

            string shortName = name.Trim().ToLower();
            if (!_filters.ContainsKey(shortName))
                return null;

            return _filters[shortName](value);
        }
        static void regTypes()
        {
            Reg<NameTest>("name");
            Reg<SpellNameTest>("spell_name");
            Reg<SpellIdTest>("spell_id");
        }
    }

    public class GeneralFilterBase : GeneralFilterTestInterface
    {
        public string Value { get; set; }
        public virtual bool Init(string value)
        {
            Value = value;
            return true;
        }

        public virtual bool Validate(GameObject go)
        {
            if (go == null)
                return false;

            return internalValidate(go);
        }

        protected virtual bool internalValidate(GameObject go)
        {
            return true;
        }
    }

    public class GeneralFilterIntBase : GeneralFilterTestInterface
    {
        public int Value { get; set; }
        public virtual bool Init(string value)
        {
            int tmp;
            if (!int.TryParse(value, out tmp))
                return false;

            Value = tmp;
            return true;
        }

        public virtual bool Validate(GameObject go)
        {
            if (go == null)
                return false;

            return internalValidate(go);
        }

        protected virtual bool internalValidate(GameObject go)
        {
            return true;
        }
    }

    public class NameTest : GeneralFilterBase
    {
        protected override bool internalValidate(GameObject go)
        {
            return go.Name == Value || go.EnglishName == Value;
        }
    }

    public class SpellNameTest : GeneralFilterBase
    {
        protected override bool internalValidate(GameObject go)
        {
            BattleCharacter b = go as BattleCharacter;
            if (b == null)
                return false;

            if (!b.IsCasting || b.SpellCastInfo == null)
                return false;

            return b.SpellCastInfo.Name == Value;
        }
    }

    public class SpellIdTest : GeneralFilterIntBase
    {
        protected override bool internalValidate(GameObject go)
        {
            BattleCharacter b = go as BattleCharacter;
            if (b == null)
                return false;

            if (!b.IsCasting)
                return false;

            return b.CastingSpellId == (uint)Value;
        }
    }

    public class FilterValidator
    {
        List<GeneralFilterTestInterface> _filters = new List<GeneralFilterTestInterface>();

        public bool UseSelf = false;
        public bool UseTarget = false;

        public FilterValidator()
        {
        }

        public void Add(string name, string value)
        {
            if (name == "self" && value == "1")
            {
                UseSelf = true;
                return;
            }

            if (name == "target" && value == "1")
            {
                UseTarget = true;
                return;
            }

            GeneralFilterTestInterface f = GeneralFilterFactory.create(name, value);
            if (f == null)
                return;

            _filters.Add(f);
        }

        public bool Validate(GameObject obj)
        {
            if (UseSelf)
            {
                return internalValidate(ff14bot.Core.Me);
            }

            if (UseTarget)
            {
                return internalValidate(ff14bot.Core.Me.CurrentTarget);
            }

            return internalValidate(obj);
        }

        bool internalValidate(GameObject obj)
        {
            return _filters.TrueForAll(f => f.Validate(obj));
        }
    }

    public class GeneralFilter : ComplexDrawCommand
    {
        FilterValidator validator;

        public GeneralFilter() : base()
        {
        }

        public override bool Init(YamLikeCommand cmd)
        {
            validator = new FilterValidator();
            foreach (var p in cmd.Params)
            {
                validator.Add(p.Key, p.Value);
            }

            return base.Init(cmd);
        }

        public override void Drawing(DrawingContext ctx, GameObject obj)
        {
            if (!validator.Validate(obj))
                return;

            base.Drawing(ctx, obj);
        }
    }
}
