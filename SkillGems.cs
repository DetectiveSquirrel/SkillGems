using ExileCore;
using ExileCore.PoEMemory;
using ExileCore.PoEMemory.Components;
using ExileCore.Shared.Helpers;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Vector2 = System.Numerics.Vector2;

namespace SkillGems
{
    public class SkillGems : BaseSettingsPlugin<SkillGemsSettings>
    {
        private CancellationTokenSource _gemLevelingCts;
        private Task _gemLevelingTask;
        private Vector2 _mousePosition;

        public override bool Initialise()
        {
            Input.RegisterKey(Settings.Run);
            return true;
        }

        public void Enable()
        {
            _gemLevelingCts = new CancellationTokenSource();
        }

        public void Disable()
        {
            _gemLevelingCts.Cancel();
        }

        private void SetCursorPos(Vector2 v)
        {
            Input.SetCursorPos(GameController.Window.GetWindowRectangleTimeCache.TopLeft.ToVector2Num() + v);
        }

        private void SetCursorPos(Element e)
        {
            SetCursorPos(e.GetClientRectCache.Center.ToVector2Num());
        }

        public override Job Tick()
        {
            if (!Input.IsKeyDown(Settings.Run.Value) || !PanelVisible())
            {
                _gemLevelingCts?.Cancel();
            }
            else if (CanTick() && IsPlayerAlive() && AnythingToLevel() && PanelVisible() && _gemLevelingTask == null)
            {
                _mousePosition = Input.MousePositionNum;
                _gemLevelingCts = new CancellationTokenSource();
                _gemLevelingTask = Task.FromResult(BeginGemLevel(_gemLevelingCts.Token)).Unwrap();
                _gemLevelingTask.ContinueWith((task) =>
                {
                    _gemLevelingTask = null;
                    SetCursorPos(_mousePosition);
                });
            }

            return null;
        }

        private async Task BeginGemLevel(CancellationToken cancellationToken)
        {
            var gemsToLvlUpElements = GetLevelableGems();

            if (!gemsToLvlUpElements.Any()) return;

            var elementToClick = gemsToLvlUpElements.ToList().FirstOrDefault()?.GetChildAtIndex(1);

            var ActionDelay = Settings.DelayBetweenEachMouseEvent.Value;
            var GemDelay = Settings.DelayBetweenEachGemClick.Value;

            if (Settings.AddPingIntoDelay.Value)
            {
                ActionDelay += GameController.IngameState.ServerData.Latency;
                GemDelay += GameController.IngameState.ServerData.Latency;
            }

            SetCursorPos(elementToClick);
            await Task.Delay(ActionDelay);
            Input.LeftDown();
            await Task.Delay(ActionDelay);
            Input.LeftUp();
            await Task.Delay(GemDelay);

            if (cancellationToken.IsCancellationRequested) return;
        }

        private bool PanelVisible()
        {
            return !(GameController.Game.IngameState.IngameUi.InventoryPanel.IsVisible
                || GameController.Game.IngameState.IngameUi.Atlas.IsVisible
                || GameController.Game.IngameState.IngameUi.TreePanel.IsVisible
                || GameController.Game.IngameState.IngameUi.SyndicatePanel.IsVisible
                || GameController.Game.IngameState.IngameUi.OpenRightPanel.IsVisible
                || GameController.Game.IngameState.IngameUi.ChatTitlePanel.IsVisible
                || GameController.Game.IngameState.IngameUi.DelveWindow.IsVisible);
        }

        private bool CanTick()
        {
            return !GameController.IsLoading
                && GameController.Game.IngameState.ServerData.IsInGame
                && GameController.Player != null
                && GameController.Player.Address != 0
                && GameController.Player.IsValid
                && GameController.Window.IsForeground();
        }

        private bool IsPlayerAlive()
        {
            return GameController.Game.IngameState.Data.LocalPlayer.GetComponent<Life>().CurHP > 0;
        }

        private bool AnythingToLevel()
        {
            return GetLevelableGems().Any();
        }

        private List<Element> GetLevelableGems()
        {
            var gemsToLevelUp = new List<Element>();

            var possibleGemsToLvlUpElements = GameController.IngameState.IngameUi?.GemLvlUpPanel?.GemsToLvlUp;

            if (possibleGemsToLvlUpElements != null && possibleGemsToLvlUpElements.Any())
            {
                foreach (var possibleGemsToLvlUpElement in possibleGemsToLvlUpElements)
                {
                    foreach (var elem in possibleGemsToLvlUpElement.Children)
                    {
                        if (elem.Text != null && elem.Text.Contains("Click to level"))
                            gemsToLevelUp.Add(possibleGemsToLvlUpElement);
                    }
                }
            }

            return gemsToLevelUp;
        }
    }
}
