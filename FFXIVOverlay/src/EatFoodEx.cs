//
// LICENSE:
// This work is licensed under the
//     Creative Commons Attribution-NonCommercial-ShareAlike 3.0 Unported License.
// also known as CC-BY-NC-SA.  To view a copy of this license, visit
//      http://creativecommons.org/licenses/by-nc-sa/3.0/
// or send a letter to
//      Creative Commons // 171 Second Street, Suite 300 // San Francisco, California, 94105, USA.
//
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;
using Buddy.Coroutines;
using Clio.Utilities;
using Clio.XmlEngine;
using ff14bot.Behavior;
using ff14bot.Helpers;
using ff14bot.Managers;
using ff14bot.Navigation;
using ff14bot.Objects;
using ff14bot.RemoteWindows;
using TreeSharp;
using Action = TreeSharp.Action;

namespace ff14bot.NeoProfiles
{
    [XmlElement("EatFoodEx")]
    public class EatFoodEx : ProfileBehavior
    {
        [XmlAttribute("Name")]
        [DefaultValue("")]
        public string ItemName { get; set; }

        [XmlAttribute("ItemId")]
        [DefaultValue(0)]
        public int ItemId { get; set; }


        [XmlAttribute("Aura")]
        [DefaultValue("")]
        public string AuraName { get; set; }

        [XmlAttribute("AuraId")]
        [DefaultValue(0)]
        public int AuraId { get; set; }


        [XmlAttribute("MinDuration")]
        [DefaultValue(5)]
        public int MinDuration { get; set; }

        
        [XmlAttribute("HqOnly")]
        public bool HqOnly { get; set; }

        [XmlAttribute("NqOnly")]
        public bool NqOnly { get; set; }


        protected bool _IsDone;

        #region Overrides of ProfileBehavior

        public override bool IsDone
        {
            get
            {
                return _IsDone;
            }
        }

        #endregion

        //private BagSlot itemslot;
        private Item itemData;
        private AuraResult expectedAura;

        void initItemData()
        {
            if (HqOnly && NqOnly)
            {
                throw new System.Exception("Both HqOnly and NqOnly cannot be true");
            }

            itemData = null;

            if (!string.IsNullOrWhiteSpace(ItemName))
            {
                itemData = DataManager.GetItem(ItemName);
            }

            if (itemData == null)
            {
                itemData = DataManager.GetItem((uint)ItemId);
            }
            
            if (itemData == null)
            {
                throw new System.Exception(string.Format("Couldn't locate item with id of {0} or Name {1}", ItemId, ItemName));
            }
        }
        void initAuraData()
        {
            expectedAura = null;

            bool hasAnyAura = (!string.IsNullOrWhiteSpace(AuraName) || AuraId != 0);
            if (!hasAnyAura)
            {
                Log(System.Windows.Media.Color.FromRgb(0xCC, 0, 0), "No aura info. Can eat all items.");
            }

            if (!string.IsNullOrWhiteSpace(AuraName))
            {
                DataManager.AuraCache.TryGetValue(AuraName, out expectedAura);
            }

            if (expectedAura == null)
            {
                DataManager.AuraCache.TryGetValue((uint)AuraId, out expectedAura);
            }

            if (expectedAura == null && hasAnyAura)
            {
                throw new System.Exception(
                    string.Format(
                        "Couldn't locate aura {0}{1}",
                        (string.IsNullOrWhiteSpace(AuraName) ? "" : string.Format("Name: {0} ", AuraName)),
                        (AuraId ==0 ? "" : string.Format("Id: {0}", AuraId))
                    )
                );
            }
        }

        BagSlot findItem()
        {
            if (itemData == null)
                return null;

            var validItems = InventoryManager.FilledSlots.Where(r => r.RawItemId == itemData.Id);
            if (HqOnly)
            {
                validItems = validItems.Where(r => r.IsHighQuality);
            }

            if (NqOnly)
            {
                validItems = validItems.Where(r => !r.IsHighQuality);
            }

            return validItems.OrderBy(r => r.IsHighQuality).FirstOrDefault();
        }

        protected override void OnStart()
        {
            try
            {
                initAuraData();
                initItemData();

                var anyItem = findItem();
                if (anyItem == null)
                {
                    TreeRoot.Stop(string.Format("We don't have any {0}{1} {2} in our inventory."
                        , (HqOnly ? "[HQ] " : "")
                        , itemData.CurrentLocaleName, itemData.Id)
                        );
                    return;
                }

            } 
            catch(System.Exception ex)
            {
                TreeRoot.Stop(ex.Message);
                return;
            }
        }

        protected override void OnResetCachedDone()
        {
            itemData = null;
            expectedAura = null;
            _IsDone = false;
        }

        public override string StatusText 
        {
            get
            {
                if (itemData != null)
                {
                    return "Eating " + itemData.CurrentLocaleName;
                }
                return "";
            } 
        }


        private async Task<bool> Eatfood()
        {
            uint auraId = 0;

            bool waitForAura = false;
            bool shouldEat = false;
            bool alreadyPresent = false;

            if (expectedAura != null)
            {
                auraId = expectedAura.Id;
                waitForAura = true;
                if (Core.Me.HasAura(auraId))
                {
                    var auraInfo = Core.Player.GetAuraById(auraId);
                    if (auraInfo.TimespanLeft.TotalMinutes < MinDuration)
                    {
                        shouldEat = true;
                        alreadyPresent = true;
                    }
                }
                else
                {
                    shouldEat = true;
                }
            }
            else {
                shouldEat = true;
            }

            BagSlot itemslot = null;

            if (shouldEat)
            {
                itemslot = findItem();
                if (itemslot == null)
                {
                    shouldEat = false; // silent exit
                    Log(System.Windows.Media.Color.FromRgb(0xCC, 0, 0), string.Format("We don't have any {0}{1} {2} in our inventory. Can't eat anymore."
                        , (HqOnly ? "[HQ] " : "")
                        , itemData.CurrentLocaleName, itemData.Id));
                }
            }

            if (shouldEat)
            {
                if (CraftingLog.IsOpen || CraftingManager.IsCrafting)
                {
                    await Coroutine.Wait(Timeout.InfiniteTimeSpan, () => CraftingLog.IsOpen);
                    await Coroutine.Sleep(1000);
                    CraftingLog.Close();
                    await Coroutine.Yield();
                    await Coroutine.Wait(Timeout.InfiniteTimeSpan, () => !CraftingLog.IsOpen);
                    await Coroutine.Wait(Timeout.InfiniteTimeSpan, () => !CraftingManager.AnimationLocked);
                }

                Log("Waiting until the item is usable.");
                    await Coroutine.Wait(Timeout.InfiniteTimeSpan, () => itemslot.CanUse(null));

                Log("Eating {0}",itemData.CurrentLocaleName);
                itemslot.UseItem();
                await Coroutine.Sleep(5000);

                if (waitForAura)
                {
                    if (!alreadyPresent)
                    {
                        Log("Waiting for the aura to appear");
                        await Coroutine.Wait(Timeout.InfiniteTimeSpan, () => Core.Player.HasAura(auraId));
                    }
                    else
                    {
                        Log("Waiting until the duration is refreshed");
                        await Coroutine.Wait(Timeout.InfiniteTimeSpan, () => Core.Player.GetAuraById(auraId).TimespanLeft.TotalMinutes > MinDuration);
                    }
                }

            }

            _IsDone = true;


            return true;
        }

        protected override Composite CreateBehavior()
        {
            return new ActionRunCoroutine(ctx => Eatfood());
        }
    }
}
